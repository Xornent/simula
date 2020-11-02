using System;
using System.Collections.Generic;
using Simula.Scripting.Debugging;
using System.Text;
using Simula.Scripting.Reflection;

namespace Simula.Scripting.Syntax {

    public class OperatorStatement : EvaluationStatement {
        public OperatorStatement? Left { get; set; }
        public OperatorStatement? Right { get; set; }

        public virtual ExecutionResult Operate(Compilation.RuntimeContext ctx) {
            return new ExecutionResult();
        }

        public ExecutionResult OperateByName(string name, Compilation.RuntimeContext ctx) {
            if (this.Left == null) return new ExecutionResult();
            if (this.Right == null) return new ExecutionResult();
            ExecutionResult evalLeft = this.Left.Operate(ctx);
            ExecutionResult evalRight = this.Right.Operate(ctx);

            switch (evalLeft.Result.Type) {
                case Reflection.MemberType.Class:
                    break;
                case Reflection.MemberType.Instance:
                    Instance instance = (Instance)evalLeft.Result;
                    var member = instance.GetMember(name);
                    if(member.Pointer != 0) {
                        if (member.Result.Type == MemberType.Function) {
                            var result = ((Function)member.Result).Invoke(new List<Member>() { evalRight.Result }, ctx);
                            return result;
                        }
                    }
                    break;
                case Reflection.MemberType.Function:
                    break;
                case Reflection.MemberType.Module:
                    break;
                case Reflection.MemberType.Unknown:
                    break;
                default:
                    break;
            }

            return new ExecutionResult();
        }

        public ExecutionResult OperateBySignal(string signal, Compilation.RuntimeContext ctx) {
            if (Class.Operators.ContainsKey(signal))
                if (Class.Operators[signal].Type == OperatorType.Binary)
                    return OperateByName(Class.Operators[signal].MethodBinding, ctx);
            return new ExecutionResult();
        }
    }
}
