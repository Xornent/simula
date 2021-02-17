using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Parser.Ast
{
    public class IterateLiteralStatement : BlockStatement, IIterateStatement
    {
        public IterateLiteralStatement(Literal literal) 
        {
            this.Literal = literal;
        }
        
        public bool IsLiteralIteration { get; set; } = true;
        public bool ExplicitPosition { get; set; } = false;

        public Literal Literal { get; set; }
    }
}
