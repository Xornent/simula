using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Simula.Scripting.Syntax;
using System.Linq;
using System.Reflection;
using Simula.Scripting.Reflection;
using Simula.Scripting.Debugging;

namespace Simula.Scripting.Compilation {

    public abstract class CompilationUnit {
        public abstract void Register(RuntimeContext ctx);
    }

    public class SourceCompilationUnit : CompilationUnit {
        public string Content { get; private set; } = "";
        public string File { get; private set; } = "";
        public Token.TokenDocument? Tokens { get; set; }

        private BlockStatement? body = null;
        public BlockStatement Body {
            get { if (body == null) body = Parse(); return body; }
        }

        public SourceCompilationUnit(string path, FileMode mode) {
            this.File = path;
            FileStream stream = new FileStream(path, mode);
            StreamReader reader = new StreamReader(stream);
            this.Content = reader.ReadToEnd();
            reader.Close();
            stream.Close();
        }

        public SourceCompilationUnit(string content) {
            this.Content = content;
        }

        private BlockStatement Parse() {
            BlockStatement block = new BlockStatement();
            Token.TokenDocument doc = new Token.TokenDocument();
            doc.Tokenize(this.Content);
            block.Parse(doc.Tokens);
            this.Tokens = doc;
            return block;
        }

        public override void Register(RuntimeContext ctx) {

            BlockStatement block = this.Body;
            TemperaryContext runstack;
            if(ctx.CallStack.Count>0) {
                runstack = ctx.CallStack.Peek();
            } else {
                runstack = new TemperaryContext(ctx);
                ctx.CallStack.Push(runstack);
            }

            runstack.Name = "<全局>";
            runstack.Permeable = true;

            string currentModel = "";
            foreach (var item in block.Children) {
                if (item is ModuleStatement mod) {
                    currentModel = mod.FullName;
                }
            }

            Locator locModule = Utilities.GetLocator(currentModel);
            Simula.Scripting.Reflection.Module? module = ctx.AllocateMpdule(locModule);
            if (module == null) {
                ctx.Errors.Add(new RuntimeError(0, "使用了无效的模块命名: 一个模块要么是缺省的, 要么采用一个不和当前域下任何变量重名的名称命名."));
                return;
            }

            foreach (var item in block.Children) {

                // 值得注意的是, 一个注册进模块中的变量只能是声明在最外层的变量, 假如一个变量声明在函数的里层
                // 或者一个类型的里层, 在这里是不会被解析的. 当类型实例化的时候会解析类型中声明的函数和变量,
                // 在函数被执行的时候会解析局部的变量. (事实上, 局部的变量不需要声明, 在赋值语句调用时就会自行添加)

                if (item is DefinitionBlock) {
                    var defs = item as DefinitionBlock;
                    if (defs == null) continue;

                    switch (defs.Type) {
                        case DefinitionType.Constant:
                            ExecutionResult? result = defs.ConstantValue?.Execute(ctx);
                            if (result == null) break;
                            if(result.Result.Name != "") {
                                ExecutionResult renamed = new ExecutionResult(result.Result, ctx.MaximumAllocatedPointer, ctx);
                                ctx.MaximumAllocatedPointer++;
                                module.SetMember(defs.ConstantName ?? "", result.Result);
                            } else {
                                result.Result.ModuleHierarchy = locModule;
                                result.Result.Name = defs.ConstantName ?? "";
                                module.SetMember(result.Result.Name, result.Result);
                            }
                            break;
                        case DefinitionType.Function:
                            Reflection.Function func = new Reflection.Function(ref ctx);
                            func.ModuleHierarchy = locModule;
                            func.Name = defs.FunctionName?.Value ?? "";
                            func.Startup = defs.Children;

                            foreach (var par in defs.FunctionParameters) {
                                var typeResult = par.Type?.Execute(ctx).Result;
                                if(typeResult.Type != MemberType.Class) {
                                    ctx.Errors.Add(new RuntimeError(1, "函数的参数类型不是可用的类型.", defs.FunctionName?.Location));
                                    break;
                                }
                                func.Parameters.Add(new NamedType(par.Name?.Value ?? "", (Class)typeResult));
                            }

                            ExecutionResult funcResult = new ExecutionResult(func, ctx);
                            module.SetMember(func.Name, func);
                            break;
                        case DefinitionType.Class:
                            Class cls = new Class(ref ctx);
                            cls.ModuleHierarchy = locModule;
                            cls.Name = defs.ClassName?.Value ?? "";
                            cls.Definition = defs;

                            var classInheritage = defs.ClassInheritage?.Execute(ctx).Result;
                            if(classInheritage!= null) 
                                if(classInheritage.Type != MemberType.Class) {
                                    ctx.Errors.Add(new RuntimeError(2, "类型的继承对象不是可用的类型", defs.ClassName?.Location));
                                    break;
                                }
                            cls.Inheritage = (Class?)classInheritage;
                            ExecutionResult clsResult = new ExecutionResult(cls, ctx);
                            module.SetMember(cls.Name, cls);
                            break;
                        default:
                            break;
                    }
                }
            }

            ctx.CallStack.Pop();
            ctx.Registry.Add(this);
        }

        public dynamic Run(RuntimeContext ctx) {
            bool exist = false;
            foreach (var item in ctx.Registry) {
                if (item is SourceCompilationUnit)
                    if (((item as SourceCompilationUnit)?.File ?? "") == this.File)
                        exist = true;
            }

            if (!exist) this.Register(ctx);

            // 在解析过程的最开始, 我们分析 use 和 module 语句, 已确定可以使用的对象的声明
            // 以及获取本源文件定义的模块名称.

            TemperaryContext runstack = new TemperaryContext(ctx);
            runstack.Permeable = true;
            ctx.CallStack.Push(runstack);
            BlockStatement block = this.Body;
            foreach (var item in block.Children) {
                if (item is UseStatement use) {
                    var locator = Utilities.GetLocator(use.FullName);
                    runstack.Usings.Add(locator);
                }
            }

            var result = this.Body.Execute(ctx);
            ctx.CallStack.Pop();

            return result.Result;
        }
    }

    public class LibraryCompilationUnit : CompilationUnit {
        
        public string File { get; private set; } = "";

        public LibraryCompilationUnit(string path, FileMode mode) {
            this.File = path;
        }

        public override void Register(RuntimeContext ctx) {

            // 注册公用变量.

            System.Reflection.Assembly asm = Assembly.LoadFrom(this.File);
            foreach (var type in asm.GetTypes()) {
                Locator locModule = Utilities.GetLocator(type.Namespace?.Replace("Simula.Scripting.Type.", "").Replace("Simula.Scripting.Type", "") ?? "");
                Simula.Scripting.Reflection.Module? module = ctx.AllocateMpdule(locModule);
                Reflection.Markup.ExposeAttribute? expose = type.GetCustomAttribute<Reflection.Markup.ExposeAttribute>();
                if (expose != null) {
                    if (expose.Alias == "<global>") {
                        foreach (var item in type.GetFields()) {
                            expose = item.GetCustomAttribute<Reflection.Markup.ExposeAttribute>();
                            if(expose!= null) {
                                var value = item.GetValue(null);
                                if (value == null) break;
                                if(value is Member) {
                                    Reflection.Module glb = new Reflection.Module(ctx);
                                    ExecutionResult moduleresult = new ExecutionResult(glb, ctx);
                                    if (!ctx.PredefinedObjects.ContainsKey("")) ctx.PredefinedObjects.Add("", new Metadata(moduleresult.Pointer, MemberType.Module));
                                    if (ctx.PredefinedObjects.ContainsKey("")) 
                                        ((Reflection.Module)(ctx.GetMemberByMetadata(ctx.PredefinedObjects[""]))).SetMember(item.Name, new ClrInstance(item, ref ctx)); 
                                }
                            }
                        }

                        foreach (var item in type.GetMethods()) {
                            expose = item.GetCustomAttribute<Reflection.Markup.ExposeAttribute>();
                            if (expose != null) {
                                module?.SetMember(expose.Alias, new ClrFunction(item, ref ctx));
                            }
                        }
                    } else {
                        module?.SetMember(expose.Alias, ClrClass.Create(type, ref ctx));
                    }
                }
            }

            ctx.Registry.Add(this);
        }
    }

    public class ObjectCompilationUnit : CompilationUnit {

        public string File { get; private set; } = "";

        public ObjectCompilationUnit(string path, FileMode mode) {
            this.File = path;
        }

        private List<Reflection.Module> modules = new List<Reflection.Module>();
        public List<Reflection.Module> Modules {
            get { if (modules.Count == 0) modules = Generate(); return modules; }
        }

        private List<Reflection.Module> Generate() {
            List<Reflection.Module> mods = new List<Reflection.Module>();
            return mods;
        }

        public override void Register(RuntimeContext ctx) {

        }
    }

    public static class Utilities {
        public static string Connect(this List<string> list, string connection) {
            if (list.Count == 0) return "";
            string s = list[0];
            for(int i = 1; i<list.Count;i++) {
                s = s + connection + list[i];
            }
            return s;
        }

        public static Locator GetLocator(string? s) {
            if (string.IsNullOrEmpty(s)) {
                var loca = new Locator(false);
                loca.Add("");
                return loca;
            }

            if (s.StartsWith("Simula.Scripting.Type.")) s = s.Replace("Simula.Scripting.Type.", "");
            if (s.StartsWith("Simula.Scripting.Type")) s = s.Replace("Simula.Scripting.Type", "");
            if (s.Contains(".")) {
                Locator loc = new Locator(false);
                var hirachy = s.Split('.').ToList();
                foreach (var item in hirachy) {
                    loc.Add(item);
                }
                return loc;
            } else {
                Locator loc = new Locator(false);
                loc.Add(s);
                return loc;
            }
        }
    }
}
