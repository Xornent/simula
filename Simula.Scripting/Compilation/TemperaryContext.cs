using Simula.Scripting.Reflection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Compilation {
   
    public class TemperaryContext {

        // [IMP] 虽然我们在语言的现行内置定义中并没有定义以模块为对象, 但我们有可能会在接下来的版本中
        //       实现它. (如果有, 参见 Type.Module 内置类)

        public Dictionary<string, Module> SubModules = new Dictionary<string, Module>();
        public Dictionary<string, AbstractClass> Classes = new Dictionary<string, AbstractClass>();
        public Dictionary<string, IdentityClass> IdentityClasses = new Dictionary<string, IdentityClass>();
        public Dictionary<string, Function> Functions = new Dictionary<string, Function>();
        public Dictionary<string, Instance> Instances = new Dictionary<string, Instance>();
        public Dictionary<string, Variable> Variables = new Dictionary<string, Variable>();

        private dynamic? GetMember(string name) {
            if (this.Classes.ContainsKey(name)) return this.Classes[name];
            if (this.Functions.ContainsKey(name)) return this.Functions[name];
            if (this.IdentityClasses.ContainsKey(name)) return this.IdentityClasses[name];
            if (this.Instances.ContainsKey(name)) return this.Instances[name];
            if (this.Variables.ContainsKey(name)) return this.Variables[name];
            if (this.SubModules.ContainsKey(name)) return this.SubModules[name];
            return null;
        }

        private dynamic? GetMemberModulePrior(string name) {
            if (this.SubModules.ContainsKey(name)) return this.SubModules[name];
            if (this.Functions.ContainsKey(name)) return this.Functions[name];
            if (this.IdentityClasses.ContainsKey(name)) return this.IdentityClasses[name];
            if (this.Instances.ContainsKey(name)) return this.Instances[name];
            if (this.Variables.ContainsKey(name)) return this.Variables[name];
            if (this.Classes.ContainsKey(name)) return this.Classes[name];
            return null;
        }

        public void SetMember(string name, dynamic obj) {
            dynamic? m = GetMember(name);

            if (m == null) {
                if (obj is Type.Var) {
                    Variable v = new Variable();
                    v.Name = name;
                    v.Object = (Type.Var)obj;
                    this.Variables.OverflowAddVariable(v.Name, v);
                    v.ModuleHirachy = new List<string>() { "<callstack>" };
                } else if (obj is Variable) {
                    this.Variables.OverflowAddVariable(name, (Variable)obj);
                } else if (obj is Module) {
                    this.SubModules.OverflowAddModule(name, (Module)obj);
                } else if (obj is AbstractClass) {
                    this.Classes.OverflowAddAbstractClass(name, (AbstractClass)obj);
                } else if (obj is IdentityClass) {
                    this.IdentityClasses.OverflowAddIdentityClass(name, (IdentityClass)obj);
                } else if (obj is Instance) {
                    this.Instances.OverflowAddInstance(name, (Instance)obj);
                } else if (obj is Function) {
                    this.Functions.OverflowAddFunction(name, (Function)obj);
                }
            } else {
                List<string> keys = new List<string>();
                if (m is Variable) {
                    keys = this.Variables.RemoveValue((Variable)m);
                } else if (m is Module) {
                    keys = this.SubModules.RemoveValue((Module)m);
                } else if (m is AbstractClass) {
                    keys = this.Classes.RemoveValue((AbstractClass)m);
                } else if (m is IdentityClass) {
                    keys = this.IdentityClasses.RemoveValue((IdentityClass)m);
                } else if (m is Instance) {
                    keys = this.Instances.RemoveValue((Instance)m);
                } else if (m is Function) {
                    keys = this.Functions.RemoveValue((Function)m);
                }

                if (obj is Type.Var) {
                    Variable v = new Variable();
                    v.Name = name;
                    v.Object = (Type.Var)obj;
                    foreach (var item in keys) {
                        this.Variables.OverflowAddVariable(item, v);
                    }
                } else if (obj is Variable) {
                    foreach (var item in keys) {
                        this.Variables.OverflowAddVariable(item, (Variable)obj);
                    }
                } else if (obj is Module) {
                    foreach (var item in keys) {
                        this.SubModules.OverflowAddModule(item, (Module)obj);
                    }
                } else if (obj is AbstractClass) {
                    foreach (var item in keys) {
                        this.Classes.OverflowAddAbstractClass(item, (AbstractClass)obj);
                    }
                } else if (obj is IdentityClass) {
                    foreach (var item in keys) {
                        this.IdentityClasses.OverflowAddIdentityClass(item, (IdentityClass)obj);
                    }
                } else if (obj is Instance) {
                    foreach (var item in keys) {
                        this.Instances.OverflowAddInstance(item, (Instance)obj);
                    }
                } else if (obj is Function) {
                    foreach (var item in keys) {
                        this.Functions.OverflowAddFunction(item, (Function)obj);
                    }
                }
            }
        }
    }
}
