using Simula.Scripting.Debugging;
using Simula.Scripting.Reflection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Windows;

namespace Simula.Scripting.Syntax {
   
    public class FunctionCallOperation : OperatorStatement {
        public override ExecutionResult Operate(Compilation.RuntimeContext ctx) {
            if (this.Left == null) return new ExecutionResult();
            if (this.Right == null) return new ExecutionResult();
            var evalLeft = Left.Operate(ctx);
            SmallBracketOperation angle = (SmallBracketOperation)this.Right;
            List<ExecutionResult> parameterResults = angle.OperateList(ctx);
            List<Member> raws = new List<Member>();

            Function? target = null;
            switch (evalLeft.Result.Type) {
                case MemberType.Class:
                    var abstracts = ((Class)evalLeft.Result);
                    foreach (var item in parameterResults) 
                        raws.Add(item.Result);
                    return new ExecutionResult(abstracts.CreateInstance(raws, ctx), ctx);
                case MemberType.Instance:
                    var instance = ((Instance)evalLeft.Result).GetMember("_call");
                    if (instance.Result.Type == MemberType.Function)
                        target = (Function)instance.Result;
                    else return new ExecutionResult();
                    goto case MemberType.Function;
                case MemberType.Function:
                    if(target == null) 
                        target = (Function)evalLeft.Result;
                    foreach (var item in parameterResults) 
                        raws.Add(item.Result);
                    return target.Invoke(raws, ctx);
                case MemberType.Module:
                    break;
                case MemberType.Unknown:
                    break;
                default:
                    break;
            }
            return new ExecutionResult();
        }
    }
}
