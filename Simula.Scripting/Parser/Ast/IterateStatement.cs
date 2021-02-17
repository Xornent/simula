using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Parser.Ast
{
    public class IterateStatement : BlockStatement, IIterateStatement
    {
        public IterateStatement(Literal literal, IExpression iter)
        {
            this.Variable = literal;
            this.Iterator = iter;
        }
        
        public bool IsLiteralIteration { get; set; } = false;
        public bool ExplicitPosition { get; set; } = false;

        public Literal Variable { get; set; }
        public IExpression Iterator { get; set; }
    }
}
