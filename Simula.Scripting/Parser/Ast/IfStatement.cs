using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Parser.Ast
{
    public class IfStatement : BlockStatement
    {
        public IfStatement(IExpression condition, BlockStatement? otherwise) 
        {
            this.Condition = condition;
            this.Otherwise = otherwise;
        }
        
        public IExpression Condition;
        public BlockStatement? Otherwise;
    }
}
