using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Reflection {
   
    public class AbstractClass {
        public string Name = "";
        public string FullName = "";
        public List<IdentityClass> SubclassIdentifer = new List<IdentityClass>();
        public IdentityClass? Inheritage;

        public Documentation DocumentationSource = new Documentation();

        public bool Compiled = false;
        public AbstractClass? Conflict;
    }

    public class IdentityClass {
        public string Name = "";
        public string FullName = "";
        public List<Variable> SubclassIdentifer = new List<Variable>();

        public bool Compiled = false;
        public IdentityClass? Conflict;
    }

    public class Instance {
        public string Name = "";
        public string FullName = "";

        public List<Function> Functions = new List<Function>();
        public List<Variable> Variables = new List<Variable>();
        public List<Syntax.Statement> Startup = new List<Syntax.Statement>();

        public bool Compiled = false;
        public IdentityClass? Conflict;
    }
}
