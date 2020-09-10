using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Reflection {
    
    public class Function {
        public string Name = "";
        public string FullName = "";
        public List<Variable> Parameter = new List<Variable>();

        public List<Syntax.Statement> Startup = new List<Syntax.Statement>();
        public Documentation DocumentationSource = new Documentation();

        public bool Compiled = false;
        public Function? Conflict;
    }
}
