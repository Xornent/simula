using System;
using System.Collections.Generic;
using System.Text;
using Simula.Scripting.Debugging;
using Simula.Scripting.Token;

namespace Simula.Scripting.Syntax {

    public class AssignStatement : Statement {
        public EvaluationStatement? Left = null;
        public EvaluationStatement? Right = null;

        public new void Parse(Token.TokenCollection token) {
            List<TokenCollection> lr = token.Split(new Token.Token("="));
            if(lr.Count!=2) {
                Left = null;
                Right = null;
                foreach (var item in token) {
                    if(item == "=") {
                        item.Error = new TokenizerException("SS0002");
                    }
                }
                return;
            }
            Left = new EvaluationStatement();
            Left.Parse(lr[0]);
            Right = new EvaluationStatement();
            Right.Parse(lr[1]);
        }

        public override ExecutionResult Execute(Compilation.RuntimeContext ctx) {
            if (Left == null) {
                this.Right?.Execute(ctx);
                return new ExecutionResult();
            };
            if (Right == null) return new ExecutionResult();

            ExecutionResult? evalLeft = this.Left?.Execute(ctx);
            ExecutionResult? evalRight = this.Right?.Execute(ctx);

            if (evalLeft == null) return new ExecutionResult();
            if (evalRight == null) return new ExecutionResult();

            if (evalLeft.Pointer == 0) { 
                if(Left.EvaluateOperators.Count == 1) 
                    if(Left.EvaluateOperators[0] is SelfOperation)
                        ctx.CallStack.Peek().SetMember(((SelfOperation)Left.EvaluateOperators[0]).Self.Value, evalRight.Result);
            } else {
                ctx.CallStack.Peek().SetMember(evalLeft.Pointer, evalRight.Result);
            }
            return new ExecutionResult(evalRight.Result, ctx) { Flag = ExecutableFlag.Pass };
        }
    }
}
