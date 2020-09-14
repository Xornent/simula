using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Syntax {

    public class OperatorStatement : EvaluationStatement {
        public OperatorStatement? Left { get; set; }
        public OperatorStatement? Right { get; set; }

        public dynamic OperateByName(string name, Compilation.RuntimeContext ctx) {
            if (this.Left == null) return Type.Global.Null;
            if (this.Right == null) return Type.Global.Null;
            dynamic evalLeft = this.Left.Operate(ctx);
            dynamic evalRight = this.Right.Operate(ctx);

            dynamic rightObj = evalRight;
            if (evalRight is Reflection.Variable) rightObj = evalRight.Object;
            if (evalLeft is Reflection.Variable) {
                try {
                    return ((Reflection.Variable)evalLeft).InvokeMember(name, new List<Reflection.Base>() { rightObj });
                } catch {
                    this.RawEvaluateToken[0].Error = new Token.TokenizerException("SS0020");
                }
            } else if (evalLeft is Type.Var) {
                try {
                    Reflection.Variable varia = new Reflection.Variable();
                    varia.Object = (Type.Var)evalLeft;
                    return varia.InvokeMember(name, new List<Reflection.Base>() { rightObj });
                } catch {
                    this.RawEvaluateToken[0].Error = new Token.TokenizerException("SS0020");
                }
            } else if (evalLeft is Reflection.Instance) {
                try {
                    var member = ((Reflection.Instance)evalLeft).GetMember(name);
                    if (member == null) return Type.Global.Null;
                    if (member == Type.Global.Null) return Type.Global.Null;
                    if (member is Reflection.Function) {
                        return ((Reflection.Function)member).Invoke(new List<Reflection.Base>() { rightObj }, ctx) ?? Type.Global.Null;
                    } else return Type.Global.Null;
                } catch {
                    this.RawEvaluateToken[0].Error = new Token.TokenizerException("SS0020");
                }
            } else if (evalLeft is Type._Null || evalRight is Type._Null) {
                return Type.Global.Null;
            } else {
                this.RawEvaluateToken[0].Error = new Token.TokenizerException("SS0019");
            }

            return Type.Global.Null;
        }
    }
}
