using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Reflection {

    public class Module {
        public string Name = "";
        public string FullName = "";

        public Dictionary<string, Module> SubModules = new Dictionary<string, Module>();
        public Dictionary<string, AbstractClass> Classes = new Dictionary<string, AbstractClass>();
        public Dictionary<string, Function> Functions = new Dictionary<string, Function>();
        public Dictionary<string, Instance> Instances = new Dictionary<string, Instance>();
        public Dictionary<string, Variable> Variables = new Dictionary<string, Variable>();
        public List<Syntax.Statement> Startup = new List<Syntax.Statement>();
        public Documentation DocumentationSource = new Documentation();

        public bool Compiled = false;
        public Module? Conflict;
    }
}
