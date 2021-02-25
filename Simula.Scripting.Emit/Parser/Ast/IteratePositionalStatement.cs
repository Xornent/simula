using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Parser.Ast
{
    public class IteratePositionalStatement : BlockStatement, IIterateStatement
    {
        public IteratePositionalStatement(Literal variable, IExpression iter, Literal position)
        {
            this.Variable = variable;
            this.Iterator = iter;
            this.Position = position;
        }
        
        public bool IsLiteralIteration { get; set; } = false;
        public bool ExplicitPosition { get; set; } = true;

        public Literal Variable { get; set; }
        public IExpression Iterator { get; set; }
        public Literal Position { get; set; }
    }
}
