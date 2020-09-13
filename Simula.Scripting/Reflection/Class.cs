using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Reflection {

    public class AbstractClass : SourceBase {
        public string Name = "";
        public string FullName = "";
        public List<string> ModuleHirachy = new List<string>();
        public List<(Base type, string name)> SubclassIdentifer = new List<(Base, string)>();
        public IdentityClass? Inheritage;

        public Syntax.DefinitionBlock? Definition;

        public Documentation DocumentationSource = new Documentation();

        public bool Compiled = false;
        public AbstractClass? Conflict;

        /*
                    switch (defs.Type) {
                        case DefinitionType.Constant:
                            dynamic? result = defs.ConstantValue?.Result(ctx);
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
                                func.Parameters.Add((par.Type?.Result(ctx) ?? new Reflection.IdentityClass(),
                                    par.Name?.Value ?? ""));
                            }

                            Utilities.SetFunction(func.ModuleHirachy, func, ctx.Modules);
                            break;
                    }
         */

        public static Base CreateVariable(string )

        public IdentityClass? Identify(List<Base> bases) {
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
        public string Name = "";
        public string FullName = "";
        public List<string> ModuleHirachy = new List<string>();
        public List<(string name, Base value)> SubclassIdentifer = new List<(string, Base)>();

        public List<Function> Initializers = new List<Function>();
        public Syntax.DefinitionBlock? Definition;

        public AbstractClass? Abstract;

        public bool Compiled = false;
        public IdentityClass? Conflict;
    }

    public class Instance : SourceBase {
        public string Name = "";
        public string FullName = "";
        public List<string> ModuleHirachy = new List<string>();

        public List<Function> Functions = new List<Function>();
        public List<Variable> Variables = new List<Variable>();

        public bool Compiled = false;
        public Instance? Conflict;

        public dynamic GetMember(string name) {
            foreach (var item in Functions) {
                if (item.Name == name) return item;
            }

            foreach (var item in Variables) {
                if (item.Name == name) return item;
            }

            return Type.Global.Null;
        }
    }
}
