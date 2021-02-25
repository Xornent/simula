using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Parser.Ast
{
    public class CallExpression : IExpression
    {
        public CallExpression(IExpression callee) 
        {
            this.Callee = callee;
        }
        
        public List<IExpression> Arguments { get; set; } = new List<IExpression>();
        public IExpression Callee { get; set; }
        public TokenCollection Tokens { get; set; } = new TokenCollection();
    }
}
