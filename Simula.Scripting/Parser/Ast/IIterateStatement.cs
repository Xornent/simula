using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Parser.Ast
{
    public interface IIterateStatement
    {
        public bool IsLiteralIteration { get; set; }
        public bool ExplicitPosition { get; set; }
    }
}
