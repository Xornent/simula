using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Parser.Ast
{
    public class MatchCaseExpression : BlockStatement
    {
        public MatchCaseExpression(MatchStatement parent, IExpression condition, IStatement result) 
        {
            this.Match = parent;
            this.Condition = condition;
            this.Result = result;
        }
        
        public MatchStatement Match { get; set; }
        public bool IsOtherwise { get; set; }
        public IExpression Condition { get; set; }
        public IExpression Result { get; set; }
    }
}
