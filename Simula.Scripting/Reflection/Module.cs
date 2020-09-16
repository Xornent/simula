using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Reflection {

    public class Module : Base {
        public Dictionary<string, Module> SubModules = new Dictionary<string, Module>();
        public Dictionary<string, AbstractClass> Classes = new Dictionary<string, AbstractClass>();
        public Dictionary<string, IdentityClass> IdentityClasses = new Dictionary<string, IdentityClass>();
        public Dictionary<string, Function> Functions = new Dictionary<string, Function>();
        public Dictionary<string, Instance> Instances = new Dictionary<string, Instance>();
        public Dictionary<string, Variable> Variables = new Dictionary<string, Variable>();
        public List<Syntax.Statement> Startup = new List<Syntax.Statement>();
        public Documentation DocumentationSource = new Documentation();

        public bool Compiled = false;
        public Module? Conflict;

        public dynamic GetMember(string name) {

            // 这是 RuntimeContext.GetMember 的衍生.

            // 先从调用栈的顶层(如果有)寻找对象.

            var current = this;
            if (current.Classes.ContainsKey(name)) return current.Classes[name];
            if (current.Functions.ContainsKey(name)) return current.Functions[name];
            if (current.IdentityClasses.ContainsKey(name)) return current.IdentityClasses[name];
            if (current.Instances.ContainsKey(name)) return current.Instances[name];
            if (current.Variables.ContainsKey(name)) return current.Variables[name];
            if (current.SubModules.ContainsKey(name)) return current.SubModules[name];

            return Type.Global.Null;
        }

        public dynamic GetMemberModulePrior(string name) {

            // 这是 RuntimeContext.GetMemberModulePrior 的衍生.

            // 先从调用栈的顶层(如果有)寻找对象.

            var current = this;

            if (current.SubModules.ContainsKey(name)) return current.SubModules[name];
            if (current.Classes.ContainsKey(name)) return current.Classes[name];
            if (current.Functions.ContainsKey(name)) return current.Functions[name];
            if (current.IdentityClasses.ContainsKey(name)) return current.IdentityClasses[name];
            if (current.Instances.ContainsKey(name)) return current.Instances[name];
            if (current.Variables.ContainsKey(name)) return current.Variables[name];

            return Type.Global.Null;
        }

        private dynamic? GetMemberNull(string name) {
            if (this.Classes.ContainsKey(name)) return this.Classes[name];
            if (this.Functions.ContainsKey(name)) return this.Functions[name];
            if (this.IdentityClasses.ContainsKey(name)) return this.IdentityClasses[name];
            if (this.Instances.ContainsKey(name)) return this.Instances[name];
            if (this.SubModules.ContainsKey(name)) return this.SubModules[name];
            if (this.Variables.ContainsKey(name)) return this.Variables[name];
            return null;
        }

        public void SetMember(string name, dynamic obj) {
            dynamic? m = GetMemberNull(name);

            if (m == null) {
                if (obj is Type.Var) {
                    Variable v = new Variable();
                    v.Name = name;
                    v.Object = (Type.Var)obj;
                    this.Variables.OverflowAddVariable(v.Name, v);
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
                        this.Variables.OverflowAddVariable(item, (Variable) obj);
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
