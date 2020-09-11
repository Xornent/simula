using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Reflection {
   
    public class AbstractClass {
        public string Name = "";
        public string FullName = "";
        public List<string> ModuleHirachy = new List<string>();
        public List<(IdentityClass type, string name)> SubclassIdentifer = new List<(IdentityClass, string)>();
        public IdentityClass? Inheritage;

        public Syntax.DefinitionBlock? Definition;

        public Documentation DocumentationSource = new Documentation();

        public bool Compiled = false;
        public AbstractClass? Conflict;
    }

    public class IdentityClass {
        public string Name = "";
        public string FullName = "";
        public List<string> ModuleHirachy = new List<string>();
        public List<(string name, Variable value)> SubclassIdentifer = new List<(string, Variable)>();

        public AbstractClass? Abstract;

        public bool Compiled = false;
        public IdentityClass? Conflict;
    }

    public class Instance : Variable {
        public new string Name = "";
        public new string FullName = "";
        public new List<string> ModuleHirachy = new List<string>();

        public List<Function> Functions = new List<Function>();
        public List<Variable> Variables = new List<Variable>();

        public bool Compiled = false;
        public new Instance? Conflict;
    }
}
