using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Parser.Ast
{
    public class MatchCaseExpression : BlockStatement
    {
        public MatchCaseExpression(MatchStatement parent, IStatement result) 
        {
            this.Match = parent;
            this.Result = result;
        }
        
        public MatchStatement Match { get; set; }
        public bool IsOtherwise { get; set; }
        public IExpression Result { get; set; }
    }

    public class MatchCaseEqualityExpression : MatchCaseExpression
    {
        public MatchCaseEqualityExpression(MatchStatement parent, IStatement result, IExpression condition)
            : base(parent, result)
        {
            this.Condition = condition;
        }

        public IExpression Condition { get; set; }
    }

    public class MatchCaseRelationshipExpression : MatchCaseExpression
    {
        public MatchCaseRelationshipExpression(MatchStatement parent, IStatement result, IExpression condition, Operator relation)
            : base(parent, result)
        {
            this.Relation = relation;
            this.Condition = condition;
        }

        public Operator Relation { get; set; }
        public IExpression Condition { get; set; }
    }

    public class MatchCaseDeclarativeExpression : MatchCaseExpression
    {
        public MatchCaseDeclarativeExpression(MatchStatement parent, IStatement result, List<FunctionParameter> fields)
            : base(parent, result)
        {
            this.Fields = fields;
        }

        public List<FunctionParameter> Fields { get; set; }
    }
}
