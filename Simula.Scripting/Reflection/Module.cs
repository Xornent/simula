using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Reflection {

    public class Module : Base {
        public string Name = "";
        public string FullName = "";

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
            if (current.SubModules.ContainsKey(name)) return current.SubModules[name];
            if (current.Variables.ContainsKey(name)) return current.Variables[name];

            return Type.Global.Null;
        }
    }
}
