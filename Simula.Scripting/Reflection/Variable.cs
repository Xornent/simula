using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;

namespace Simula.Scripting.Reflection {

    public class Variable {
        public string Name = "";
        public string FullName = "";
        public List<string> ModuleHirachy = new List<string>();

        public bool Hidden = false;

        public Type.Var Object = new Simula.Scripting.Type._Null();
        public Variable? Conflict;
    }
}
