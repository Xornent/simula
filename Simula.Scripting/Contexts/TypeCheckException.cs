using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Contexts
{
    public class TypeCheckException : ScriptException
    {
        public TypeCheckException(string id) : base(id) { }
    }
}
