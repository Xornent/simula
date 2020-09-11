using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Simula.Scripting.Syntax;
using System.Linq;
using System.Reflection;

namespace Simula.Scripting.Compilation {

    public abstract class CompilationUnit {
        public abstract void Register(RuntimeContext ctx);
        public abstract void Unregister(RuntimeContext ctx);
    }

    public class SourceCompilationUnit : CompilationUnit {

        public string Content { get; private set; } = "";
        public string File { get; private set; } = "";

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
            return block;
        }

        private void Generate(RuntimeContext ctx) {

            List<Reflection.AbstractClass> classes = new List<Reflection.AbstractClass>();
            List<Reflection.IdentityClass> identityClasses = new List<Reflection.IdentityClass>();
            List<Reflection.Instance> instances = new List<Reflection.Instance>();
            List<Reflection.Variable> variables = new List<Reflection.Variable>();
            List<Reflection.Function> functions = new List<Reflection.Function>();

            // 在解析过程的最开始, 我们分析 use 和 module 语句, 已确定可以使用的对象的声明
            // 以及获取本源文件定义的模块名称.

            BlockStatement block = this.Body;
            string? mod = "";
            foreach (var item in block.Children) {
                if (item is UseStatement) {
                    Reflection.Module? m = Utilities.GetModule(Utilities.GetModuleHirachy((item as UseStatement)?.FullName), ctx.Modules);
                    if (m == null) break;
                    foreach (var funcs in m.Functions) 
                        functions.Add(funcs.Value);
                    foreach (var clss in m.Classes)
                        classes.Add(clss.Value);
                    foreach (var ins in m.Instances)
                        instances.Add(ins.Value);
                    foreach (var vars in m.Variables)
                        variables.Add(vars.Value);
                    foreach (var id in m.IdentityClasses)
                        identityClasses.Add(id.Value);
                }

                if(item is ModuleStatement) {
                    mod = (item as ModuleStatement)?.FullName;
                }
            }

            string module = "";
            if (!string.IsNullOrEmpty(mod)) module = (string)mod;

            foreach (var item in block.Children) {
                // 值得注意的是, 一个注册进模块中的变量只能是声明通用的变量, 即一个 class (AbstractClass)
                // 类型只能由 def class 生成, 一个 func (Function) 类型只能由 def func 和 def var 生成,
                // 类型, dimension<1> 只能由 def var 生成. 而运行时产生的局部变量在初始化类时注册进
                // Instance, 在执行函数时注册进函数调用栈.

                if (item is DefinitionBlock) {
                    var defs = item as DefinitionBlock;
                    if (defs == null) continue;

                    switch (defs.Type) {
                        case DefinitionType.Constant:
                            dynamic? result = defs.ConstantValue?.Result();
                            if (result == null) break;
                            if (result is Type.Var) {
                                Reflection.Variable varia = new Reflection.Variable();
                                varia.ModuleHirachy = Utilities.GetModuleHirachy(module);
                                varia.Name = defs.ConstantName?.Value ?? "";
                                varia.Hidden = defs.Visibility == Visibility.Hidden;
                                if (module != "")
                                    varia.FullName = module + "." + varia.Name;
                                else varia.FullName = varia.Name;
                                Utilities.SetVariable(varia.ModuleHirachy, varia, ctx.Modules);
                            } else if (result is Reflection.AbstractClass) {
                                var m = Utilities.GetModule(Utilities.GetModuleHirachy(module), ctx.Modules);

                                if (m?.Classes.ContainsKey(defs.ConstantName?.Value ?? "") ?? false) {
                                    var temp = result as Reflection.AbstractClass;
                                    if (temp == null) return;
                                    temp.Conflict = m.Classes[defs.ConstantName?.Value ?? ""];
                                    m.Classes[defs.ConstantName?.Value ?? ""] = result;
                                } else m?.Classes.Add(defs.ConstantName?.Value ?? "", result);
                            } else if (result is Reflection.Function) {
                                var m = Utilities.GetModule(Utilities.GetModuleHirachy(module), ctx.Modules);

                                if (m?.Functions.ContainsKey(defs.ConstantName?.Value ?? "") ?? false) {
                                    var temp = result as Reflection.Function;
                                    if (temp == null) return;
                                    temp.Conflict = m.Functions[defs.ConstantName?.Value ?? ""];
                                    m.Functions[defs.ConstantName?.Value ?? ""] = result;
                                } else m?.Functions.Add(defs.ConstantName?.Value ?? "", result);
                            } else if (result is Reflection.IdentityClass) {
                                var m = Utilities.GetModule(Utilities.GetModuleHirachy(module), ctx.Modules);

                                if (m?.IdentityClasses.ContainsKey(defs.ConstantName?.Value ?? "") ?? false) {
                                    var temp = result as Reflection.IdentityClass;
                                    if (temp == null) return;
                                    temp.Conflict = m.IdentityClasses[defs.ConstantName?.Value ?? ""];
                                    m.IdentityClasses[defs.ConstantName?.Value ?? ""] = result;
                                } else m?.IdentityClasses.Add(defs.ConstantName?.Value ?? "", result);
                            } else if (result is Reflection.Instance) {
                                var m = Utilities.GetModule(Utilities.GetModuleHirachy(module), ctx.Modules);

                                if (m?.Instances.ContainsKey(defs.ConstantName?.Value ?? "") ?? false) {
                                    var temp = result as Reflection.Instance;
                                    if (temp == null) return;
                                    temp.Conflict = m.Instances[defs.ConstantName?.Value ?? ""];
                                    m.Instances[defs.ConstantName?.Value ?? ""] = result;
                                } else m?.Instances.Add(defs.ConstantName?.Value ?? "", result);
                            } else if (result is Reflection.Variable) {
                                var m = Utilities.GetModule(Utilities.GetModuleHirachy(module), ctx.Modules);

                                if (m?.Variables.ContainsKey(defs.ConstantName?.Value ?? "") ?? false) {
                                    var temp = result as Reflection.Variable;
                                    if (temp == null) return;
                                    temp.Conflict = m.Variables[defs.ConstantName?.Value ?? ""];
                                    m.Variables[defs.ConstantName?.Value ?? ""] = result;
                                } else m?.Variables.Add(defs.ConstantName?.Value ?? "", result);
                            } else if (result is Reflection.Module) {
                                var m = Utilities.GetModule(Utilities.GetModuleHirachy(module), ctx.Modules);

                                if (m?.SubModules.ContainsKey(defs.ConstantName?.Value ?? "") ?? false) {
                                    m.SubModules[defs.ConstantName?.Value ?? ""] = result;
                                } else m?.SubModules.Add(defs.ConstantName?.Value ?? "", result);
                            } else break;
                            break;
                        case DefinitionType.Function:
                            Reflection.Function func = new Reflection.Function();
                            func.Compiled = false;
                            if (module == "") func.FullName = defs.FunctionName?.Value ?? "";
                            else func.FullName = module + "." + defs.FunctionName?.Value ?? "";
                            func.ModuleHirachy = Utilities.GetModuleHirachy(module);
                            func.Name = defs.FunctionName?.Value ?? "";
                            func.Startup = defs.Children;

                            foreach (var par in defs.FunctionParameters) {
                                func.Parameters.Add((par.Type?.Result() ?? new Reflection.IdentityClass(),
                                    par.Name?.Value ?? ""));
                            }
                            break;
                        case DefinitionType.Class:
                            Reflection.AbstractClass cls = new Reflection.AbstractClass();
                            cls.Compiled = false;
                            if (module == "") cls.FullName = defs.ClassName?.Value ?? "";
                            else cls.FullName = module + "." + defs.ClassName?.Value ?? "";
                            cls.ModuleHirachy = Utilities.GetModuleHirachy(module);
                            cls.Name = defs.ClassName?.Value ?? "";
                            cls.Definition = defs;
                            cls.Inheritage = defs.ClassInheritage?.Result();

                            foreach (var par in defs.ClassParameters) {
                                cls.SubclassIdentifer.Add((par.Type?.Result() ?? new Reflection.IdentityClass(),
                                    par.Name?.Value ?? ""));
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        public override void Register(RuntimeContext ctx) {
            Generate(ctx);
            ctx.Registry.Add(this);
        }

        public override void Unregister(RuntimeContext ctx) {

            // 不实现反注册方法

            ctx.Registry.Remove(this);
        }

        public void Run(RuntimeContext ctx) {
            bool exist = false;
            foreach (var item in ctx.Registry) {
                if (item is SourceCompilationUnit)
                    if (((item as SourceCompilationUnit)?.File ?? "") == this.File)
                        exist = true;
            }

            if (!exist) this.Register(ctx);
            foreach (var item in this.Body.Children) {
                if (item is DefinitionBlock) { }
                else {
                    item.Operate(ctx);
                }
            }
        }
    }

    public class LibraryCompilationUnit : CompilationUnit {
        
        public string File { get; private set; } = "";

        public LibraryCompilationUnit(string path, FileMode mode) {
            this.File = path;
        }

        private List<Reflection.Variable> Variables = new List<Reflection.Variable>();

        private void Generate() {
            Assembly asm = Assembly.LoadFrom(File);
            System.Type[] types = asm.GetTypes();

            // 从已编译的程序集中加载所有类, 筛选出所有用 ExposeAttribute 标识的类.

            Variables = Utilities.GetAccessableClassVariable(asm);
        }

        public override void Register(RuntimeContext ctx) {
            foreach (var item in Variables) {
                Utilities.SetVariable(item.ModuleHirachy, item, ctx.Modules);
            }
            ctx.Registry.Add(this);
        }

        public override void Unregister(RuntimeContext ctx) {
            foreach (var item in Variables) {
                Utilities.RemoveVariable(item.ModuleHirachy, item, ctx.Modules);
            }
            ctx.Registry.Remove(this);
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

        public override void Unregister(RuntimeContext ctx) {

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

        public static List<string> GetModuleHirachy(string? s) {
            if (string.IsNullOrEmpty(s)) return new List<string>() { "" };
            if (s.StartsWith("Simula.Scripting.Type.")) s = s.Replace("Simula.Scripting.Type.", "");
            if (s.StartsWith("Simula.Scripting.Type")) s = s.Replace("Simula.Scripting.Type", "");
            if (s.Contains(".")) {
                return s.Split('.').ToList();
            } else return new List<string>() { s };
        }

        public static List<System.Type> GetAccessableClassType(Assembly asm) {
            List<System.Type> types = new List<System.Type>();
            foreach(var type in asm.GetTypes()) {
                Reflection.Markup.ExposeAttribute? expose = type.GetCustomAttribute<Reflection.Markup.ExposeAttribute>();
                if(expose != null) {
                    if (expose.Alias == "<global>") continue;
                    types.Add(type);
                }
            }
            return types;
        }

        public static List<Reflection.Variable> GetAccessableClassVariable(Assembly asm) {
            List<Reflection.Variable> classes = new List<Reflection.Variable>();
            foreach (var type in asm.GetTypes()) {
                Reflection.Markup.ExposeAttribute? expose = type.GetCustomAttribute<Reflection.Markup.ExposeAttribute>();
                if (expose != null) {
                    if (expose.Alias == "<global>") continue;
                    Reflection.Variable var = new Reflection.Variable();
                    var.FullName = GetModuleHirachy(type.Namespace).Connect(".");
                    if (var.FullName == "")
                        var.FullName += expose.Alias;
                    else var.FullName += ("." + expose.Alias);
                    var.Name = expose.Alias;
                    var.Hidden = expose.ToSystemOnly;
                    var.Object = new Type.Class(type);
                    var.ModuleHirachy = GetModuleHirachy(type.Namespace);
                    classes.Add(var);
                }
            }
            return classes;
        }

        public static void SetVariable(List<string> module, Reflection.Variable variable, Dictionary<string, Reflection.Module> destinationArray) {
            Reflection.Module? current = null;
            foreach (var submod in module) {
                if(current == null) {
                    if (destinationArray.ContainsKey(submod))
                        current = destinationArray[submod];
                    else {
                        current = new Reflection.Module();
                        current.Name = submod;
                        current.Compiled = true;
                        destinationArray.Add(submod, current);
                    }
                } else if(current.SubModules.ContainsKey(submod)) {
                    current = current.SubModules[submod];
                } else {
                    var i = new Reflection.Module();
                    i.Name = submod;
                    i.Compiled = true;
                    current.SubModules.Add(submod, i);
                    current = i;
                }
            }

            if (current.Variables.ContainsKey(variable.Name)) {
                variable.Conflict = current.Variables[variable.Name];
                current.Variables[variable.Name] = variable;
            } else
                current.Variables.Add(variable.Name, variable);
        }

        public static void SetAbstractClass(List<string> module, Reflection.AbstractClass variable, Dictionary<string, Reflection.Module> destinationArray) {
            Reflection.Module? current = null;
            foreach (var submod in module) {
                if (current == null) {
                    if (destinationArray.ContainsKey(submod))
                        current = destinationArray[submod];
                    else {
                        current = new Reflection.Module();
                        current.Name = submod;
                        current.Compiled = true;
                        destinationArray.Add(submod, current);
                    }
                } else if (current.SubModules.ContainsKey(submod)) {
                    current = current.SubModules[submod];
                } else {
                    var i = new Reflection.Module();
                    i.Name = submod;
                    i.Compiled = true;
                    current.SubModules.Add(submod, i);
                    current = i;
                }
            }

            if (current.Classes.ContainsKey(variable.Name)) {
                variable.Conflict = current.Classes[variable.Name];
                current.Classes[variable.Name] = variable;
            } else
                current.Classes.Add(variable.Name, variable);
        }

        public static void SetIdentityClass(List<string> module, Reflection.IdentityClass variable, Dictionary<string, Reflection.Module> destinationArray) {
            Reflection.Module? current = null;
            foreach (var submod in module) {
                if (current == null) {
                    if (destinationArray.ContainsKey(submod))
                        current = destinationArray[submod];
                    else {
                        current = new Reflection.Module();
                        current.Name = submod;
                        current.Compiled = true;
                        destinationArray.Add(submod, current);
                    }
                } else if (current.SubModules.ContainsKey(submod)) {
                    current = current.SubModules[submod];
                } else {
                    var i = new Reflection.Module();
                    i.Name = submod;
                    i.Compiled = true;
                    current.SubModules.Add(submod, i);
                    current = i;
                }
            }

            if (current.IdentityClasses.ContainsKey(variable.Name)) {
                variable.Conflict = current.IdentityClasses[variable.Name];
                current.IdentityClasses[variable.Name] = variable;
            } else
                current.IdentityClasses.Add(variable.Name, variable);
        }

        public static void SetInstance(List<string> module, Reflection.Instance variable, Dictionary<string, Reflection.Module> destinationArray) {
            Reflection.Module? current = null;
            foreach (var submod in module) {
                if (current == null) {
                    if (destinationArray.ContainsKey(submod))
                        current = destinationArray[submod];
                    else {
                        current = new Reflection.Module();
                        current.Name = submod;
                        current.Compiled = true;
                        destinationArray.Add(submod, current);
                    }
                } else if (current.SubModules.ContainsKey(submod)) {
                    current = current.SubModules[submod];
                } else {
                    var i = new Reflection.Module();
                    i.Name = submod;
                    i.Compiled = true;
                    current.SubModules.Add(submod, i);
                    current = i;
                }
            }

            if (current.Instances.ContainsKey(variable.Name)) {
                variable.Conflict = current.Instances[variable.Name];
                current.Instances[variable.Name] = variable;
            } else
                current.Instances.Add(variable.Name, variable);
        }

        public static void SetFunction(List<string> module, Reflection.Function variable, Dictionary<string, Reflection.Module> destinationArray) {
            Reflection.Module? current = null;
            foreach (var submod in module) {
                if (current == null) {
                    if (destinationArray.ContainsKey(submod))
                        current = destinationArray[submod];
                    else {
                        current = new Reflection.Module();
                        current.Name = submod;
                        current.Compiled = true;
                        destinationArray.Add(submod, current);
                    }
                } else if (current.SubModules.ContainsKey(submod)) {
                    current = current.SubModules[submod];
                } else {
                    var i = new Reflection.Module();
                    i.Name = submod;
                    i.Compiled = true;
                    current.SubModules.Add(submod, i);
                    current = i;
                }
            }

            if (current.Functions.ContainsKey(variable.Name)) {
                variable.Conflict = current.Functions[variable.Name];
                current.Functions[variable.Name] = variable;
            } else
                current.Functions.Add(variable.Name, variable);
        }

        public static void RemoveVariable(List<string> module, Reflection.Variable variable, Dictionary<string, Reflection.Module> destinationArray) {
            Reflection.Module? current = null;
            foreach (var submod in module) {
                if (current == null) {
                    if (destinationArray.ContainsKey(submod))
                        current = destinationArray[submod];
                    else return;
                } else if (current.SubModules.ContainsKey(submod)) {
                    current = current.SubModules[submod];
                } else return;
            }

            if (current.Variables.ContainsKey(variable.Name)) {
                Reflection.Variable varia = current.Variables[variable.Name];
                while(varia != variable) {
                    if (varia.Conflict == null) return;
                    varia = varia.Conflict;

                    if(varia.Conflict == variable) {
                        varia.Conflict = variable.Conflict;
                        return;
                    }
                }
            } else return;
        }

        public static void RemoveAbstractClass(List<string> module, Reflection.AbstractClass variable, Dictionary<string, Reflection.Module> destinationArray) {
            Reflection.Module? current = null;
            foreach (var submod in module) {
                if (current == null) {
                    if (destinationArray.ContainsKey(submod))
                        current = destinationArray[submod];
                    else return;
                } else if (current.SubModules.ContainsKey(submod)) {
                    current = current.SubModules[submod];
                } else return;
            }

            if (current.Classes.ContainsKey(variable.Name)) {
                Reflection.AbstractClass varia = current.Classes[variable.Name];
                while (varia != variable) {
                    if (varia.Conflict == null) return;
                    varia = varia.Conflict;

                    if (varia.Conflict == variable) {
                        varia.Conflict = variable.Conflict;
                        return;
                    }
                }
            } else return;
        }

        public static void RemoveIdentityClass(List<string> module, Reflection.IdentityClass variable, Dictionary<string, Reflection.Module> destinationArray) {
            Reflection.Module? current = null;
            foreach (var submod in module) {
                if (current == null) {
                    if (destinationArray.ContainsKey(submod))
                        current = destinationArray[submod];
                    else return;
                } else if (current.SubModules.ContainsKey(submod)) {
                    current = current.SubModules[submod];
                } else return;
            }

            if (current.IdentityClasses.ContainsKey(variable.Name)) {
                Reflection.IdentityClass varia = current.IdentityClasses[variable.Name];
                while (varia != variable) {
                    if (varia.Conflict == null) return;
                    varia = varia.Conflict;

                    if (varia.Conflict == variable) {
                        varia.Conflict = variable.Conflict;
                        return;
                    }
                }
            } else return;
        }

        public static void RemoveFunction(List<string> module, Reflection.Function variable, Dictionary<string, Reflection.Module> destinationArray) {
            Reflection.Module? current = null;
            foreach (var submod in module) {
                if (current == null) {
                    if (destinationArray.ContainsKey(submod))
                        current = destinationArray[submod];
                    else return;
                } else if (current.SubModules.ContainsKey(submod)) {
                    current = current.SubModules[submod];
                } else return;
            }

            if (current.Functions.ContainsKey(variable.Name)) {
                Reflection.Function varia = current.Functions[variable.Name];
                while (varia != variable) {
                    if (varia.Conflict == null) return;
                    varia = varia.Conflict;

                    if (varia.Conflict == variable) {
                        varia.Conflict = variable.Conflict;
                        return;
                    }
                }
            } else return;
        }

        public static void RemoveInstance(List<string> module, Reflection.Instance variable, Dictionary<string, Reflection.Module> destinationArray) {
            Reflection.Module? current = null;
            foreach (var submod in module) {
                if (current == null) {
                    if (destinationArray.ContainsKey(submod))
                        current = destinationArray[submod];
                    else return;
                } else if (current.SubModules.ContainsKey(submod)) {
                    current = current.SubModules[submod];
                } else return;
            }

            if (current.Instances.ContainsKey(variable.Name)) {
                Reflection.Instance varia = current.Instances[variable.Name];
                while (varia != variable) {
                    if (varia.Conflict == null) return;
                    varia = varia.Conflict;

                    if (varia.Conflict == variable) {
                        varia.Conflict = variable.Conflict;
                        return;
                    }
                }
            } else return;
        }

        public static Reflection.Module? GetModule(List<string> module, Dictionary<string, Reflection.Module> destinationArray) {
            Reflection.Module? current = null;
            foreach (var submod in module) {
                if (current == null) {
                    if (destinationArray.ContainsKey(submod))
                        current = destinationArray[submod];
                    else return null;
                } else if (current.SubModules.ContainsKey(submod)) {
                    current = current.SubModules[submod];
                } else return null;
            }
            return current;
        }

    }
}
