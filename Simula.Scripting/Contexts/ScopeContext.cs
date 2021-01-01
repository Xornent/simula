using System;
using System.Collections.Generic;
using System.Text;
using System.Dynamic;

namespace Simula.Scripting.Contexts
{
    public class ScopeContext
    {
        public dynamic Store = new ExpandoObject();
        public bool Permeable { get; set; } = true;
    }
}
