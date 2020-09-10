using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Reflection {

    public class Module {
        public string Name = "";
        public string FullName = "";

        public List<Module> SubModules = new List<Module>();
        public List<AbstractClass> Classes = new List<AbstractClass>();
        public List<Function> Functions = new List<Function>();
        public List<Instance> Instances = new List<Instance>();
        public List<Variable> Variables = new List<Variable>();
        public List<Syntax.Statement> Startup = new List<Syntax.Statement>();
        public Documentation DocumentationSource = new Documentation();

        public bool Compiled = false;
        public Module? Conflict;
    }
}
