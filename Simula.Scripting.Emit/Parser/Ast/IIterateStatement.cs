using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Parser.Ast
{
    public interface IIterateStatement
    {
        bool IsLiteralIteration { get; set; }
        bool ExplicitPosition { get; set; }
    }
}
