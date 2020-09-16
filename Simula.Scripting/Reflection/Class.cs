using Simula.Scripting.Compilation;
using Simula.Scripting.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Reflection {

    public class AbstractClass : SourceBase {
        public List<(Base type, string name)> SubclassIdentifer = new List<(Base, string)>();
        public IdentityClass? Inheritage;

        public Syntax.DefinitionBlock? Definition;

        public Documentation DocumentationSource = new Documentation();

        public bool Compiled = false;
        public AbstractClass? Conflict;


        public IdentityClass? Identify(List<Base> bases, RuntimeContext ctx) {
            IdentityClass id = new IdentityClass();
            if (bases.Count != SubclassIdentifer.Count) return null;
            int count = 0;
            foreach (var item in SubclassIdentifer) {
                if (item.type != bases[count]) return null;
                else id.SubclassIdentifer.Add((item.name, bases[count]));
                count++;
            }

            id.Abstract = this;
            id.Definition = this.Definition;

            return id;
        } 
    }

    public class IdentityClass : SourceBase {
        public List<(string name, Base value)> SubclassIdentifer = new List<(string, Base)>();

        private Dictionary<string, Function> Functions = new Dictionary<string, Function>();
        private Dictionary<string, Base> Variables = new Dictionary<string, Base>();
        public List<Function> Initializers = new List<Function>();
        public Syntax.DefinitionBlock? Definition;

        public AbstractClass? Abstract;

        public bool Compiled = false;
        public IdentityClass? Conflict;


        private void Parse(Instance ins, RuntimeContext ctx) {

            // 我们每生成一个对象, 就重新初始化一组 Functions 和 Variables, 再将其传递引用到子对象的
            // Instance 中, 而不破坏原来创建的函数. 所以我们不使用 Clear, 而是 new.

            this.Functions = new Dictionary<string, Function>();
            this.Variables = new Dictionary<string, Base>();

            string module = this.FullName;
            TemperaryContext tctx = new TemperaryContext();
            ctx.CallStack.Push(tctx);

            if (Definition == null) return;
            foreach (Syntax.Statement item in Definition.Children) {
                if (item is Syntax.DefinitionBlock) {
                    var defs = item as Syntax.DefinitionBlock;
                    if (defs == null) return;

                    switch (defs.Type) {
                        case DefinitionType.Constant:
                            dynamic? result = defs.ConstantValue?.Execute(ctx).value;
                            if (result == null) break;
                            if (result is Type.Var) {
                                Reflection.Variable varia = new Reflection.Variable();
                                varia.ModuleHirachy = Utilities.GetModuleHirachy(module);
                                varia.Name = defs.ConstantName?.Value ?? "";
                                varia.Hidden = defs.Visibility == Visibility.Hidden;
                                if (module != "")
                                    varia.FullName = module + "." + varia.Name;
                                else varia.FullName = varia.Name;
                                this.Variables.Add(defs.ConstantName?.Value ?? "", varia);
                                varia.Object = (Type.Var)result;
                                varia.ModuleHirachy = new List<string>() { "<callstack>" };
                                tctx.Variables.OverflowAddVariable(varia.Name, varia);
                            } else if (result is Reflection.AbstractClass) {
                                var temp = result as Reflection.AbstractClass;
                                if (temp == null) return;
                                this.Variables.Add(defs.ConstantName?.Value ?? "", temp);
                                tctx.Classes.OverflowAddAbstractClass(defs.ConstantName?.Value ?? "", temp);
                            } else if (result is Reflection.Function) {
                                var temp = result as Reflection.Function;
                                if (temp == null) return;
                                this.Functions.Add(defs.ConstantName?.Value ?? "", temp);
                                tctx.Functions.OverflowAdd(defs.ConstantName?.Value ?? "", temp);
                            } else if (result is Reflection.IdentityClass) {
                                var temp = result as Reflection.IdentityClass;
                                if (temp == null) return;
                                this.Variables.Add(defs.ConstantName?.Value ?? "", temp);
                                tctx.IdentityClasses.OverflowAddIdentityClass(defs.ConstantName?.Value ?? "", temp);
                            } else if (result is Reflection.Instance) {
                                var temp = result as Reflection.Instance;
                                if (temp == null) return;
                                this.Variables.Add(defs.ConstantName?.Value ?? "", temp);
                                tctx.Instances.OverflowAdd(defs.ConstantName?.Value ?? "", temp);
                            } else if (result is Reflection.Variable) {
                                var temp = result as Reflection.Variable;
                                if (temp == null) return;
                                this.Variables.Add(defs.ConstantName?.Value ?? "", temp);
                                tctx.Variables.OverflowAddVariable(defs.ConstantName?.Value ?? "", temp);
                            } else if (result is Reflection.Module) {
                                var temp = result as Reflection.Module;
                                if (temp == null) return;
                                this.Variables.Add(defs.ConstantName?.Value ?? "", temp);
                                tctx.SubModules.OverflowAddModule(defs.ConstantName?.Value ?? "", temp);
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
                            func.ModuleHirachy = new List<string>() { "<callstack>" };

                            foreach (var par in defs.FunctionParameters) {
                                func.Parameters.Add((par.Type?.Execute(ctx).value ??
                                    new Reflection.IdentityClass(),
                                    par.Name?.Value ?? ""));
                            }
                            func.Parent = ins;

                            this.Functions.Add(func.Name, func);
                            tctx.Functions.OverflowAddFunction(func.Name, func);
                            break;
                    }
                }
            }

            ctx.CallStack.Pop();
        }

        public Instance? CreateInstance(List<Base> param, RuntimeContext ctx) {
            Instance ins = new Instance();

            this.Parse(ins, ctx);
            ins.Functions = this.Functions;
            ins.Variables = this.Variables;

            Initializers.Clear();
            foreach (var item in Functions) {
                if (item.Key == "_init")
                    Initializers.Add(item.Value);
            }

            foreach (var item in Initializers) {
                try {
                    var obj = item.Invoke(param, ctx);
                    break;
                } catch { }
            }
            return ins;
        }
    }

    public class Instance : SourceBase {
        public Dictionary<string, Function> Functions = new Dictionary<string, Function>();
        public Dictionary<string, Base> Variables = new Dictionary<string, Base>();

        public bool Compiled = false;
        public Instance? Conflict;

        public dynamic GetMember(string name) {
            foreach (var item in Functions) {
                if (item.Key == name) return item.Value;
            }

            foreach (var item in Variables) {
                if (item.Key == name) return item.Value;
            }

            return Type.Global.Null;
        }
    }
}