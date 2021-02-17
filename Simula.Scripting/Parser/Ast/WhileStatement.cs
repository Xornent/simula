using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Parser.Ast
{
    public class WhileStatement : BlockStatement
    {
        public WhileStatement(IExpression evaluation)
        {
            this.Evaluation = evaluation;
            if (evaluation is Literal literal)
                if (literal.Type != LiteralType.Named) this.IsConstantEvaluation = true;
        }
        public bool IsConstantEvaluation { get; set; } = false;
        public IExpression Evaluation { get; set; }
    }
}
