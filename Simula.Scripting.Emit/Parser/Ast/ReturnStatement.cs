using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Parser.Ast
{
    public class ReturnStatement : IStatement
    {
        public ReturnStatement(IExpression returns) 
        {
            if(returns == null) this.IsEmpty = true;
            else {
                this.IsEmpty = false;
                this.ReturnExpression = returns;
            }
        }
        
        public TokenCollection Tokens { get; set; } = new TokenCollection();
        public bool IsEmpty { get; set; } = true;
        public IExpression ReturnExpression { get; set; }
    }
}
