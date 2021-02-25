using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Parser.Ast
{
    public class LazyExpression : IExpression
    {
        public LazyExpression(IExpression expression)
        {
            this.Expression = expression;
        }

        public TokenCollection Tokens { get; set; } = new TokenCollection();
        public IExpression Expression { get; set; }

        // the returning type of the expression.
        public IExpression Returns { get; set; }
    }
}
