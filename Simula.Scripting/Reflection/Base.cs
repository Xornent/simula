using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Reflection {
    
    public class Base {
        public string Name = "";
        public string FullName = "";
        public List<string> ModuleHirachy = new List<string>();
    }

    public class SourceBase : Base {
    
    }

    public class CompiledBase : Base {

    }
}
