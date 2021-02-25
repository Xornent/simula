using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Parser.Ast
{
    public class UnaryExpression : IOperatorExpression
    {
        public UnaryExpression(IExpression operant)
        {
            this.Operant = operant;
        }

        public IExpression Operant { get; set; }

        public Operator Operator { get; set; }
        public TokenCollection Tokens { get; set; } = new TokenCollection();
    }
}
