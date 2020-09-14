using System;
using System.Collections.Generic;
using System.Text;
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

        public override dynamic Operate(Compilation.RuntimeContext ctx) {
            if (Left == null) { this.Right?.Operate(ctx); return Type.Global.Null; };
            if (Right == null) { return Type.Global.Null; }
            dynamic? evalLeft = this.Left?.Operate(ctx);
            dynamic? evalRight = this.Right?.Operate(ctx);

            if (evalLeft == null) return Type.Global.Null;
            if (evalRight == null) return Type.Global.Null;
            if (evalLeft is Type._Null) { 
                if(Left.EvaluateOperators.Count == 1) 
                    if(Left.EvaluateOperators[0] is SelfOperation)
                        ctx.CallStack.Peek().SetMember(((SelfOperation)Left.EvaluateOperators[0]).Self.Value, evalRight);
            } else {
                ctx.SetMember(evalLeft, evalRight);
            }
            return evalRight;
        }
    }
}
