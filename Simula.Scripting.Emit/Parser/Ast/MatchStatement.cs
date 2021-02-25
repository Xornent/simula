using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Parser.Ast
{
    public class MatchStatement : BlockStatement
    {
        public MatchStatement(IExpression returnType, IExpression matcher) 
        {
            this.ReturnValueType = returnType;
            this.MatcherValue = matcher;
        }
        
        public bool Completeness { get; set; } = false;
        public IExpression ReturnValueType { get; set; }
        public IExpression MatcherValue { get; set; }
    }
}
