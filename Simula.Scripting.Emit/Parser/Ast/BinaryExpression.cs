using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Parser.Ast
{
    public class BinaryExpression : IOperatorExpression
    {
        public BinaryExpression(IExpression left, IExpression right)
        {
            this.Left = left;
            this.Right = right;
        }

        public IExpression Left { get; set; }
        public IExpression Right { get; set; }

        public Operator Operator { get; set; }
        public TokenCollection Tokens { get; set; } = new TokenCollection();
    }
}
