using Simula.Scripting.Debugging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Syntax {
   
    public class SmallBracketOperation : OperatorStatement{
        public override ExecutionResult Operate(Compilation.RuntimeContext ctx) {
            List<ExecutionResult> members = new List<ExecutionResult>();
            if (EvaluateOperators.Count == 0) return new ExecutionResult();
            List<OperatorStatement?> ops = new List<OperatorStatement?>() { null };
            foreach (var item in this.EvaluateOperators) {
                if (item is SelfOperation) {
                    if (((SelfOperation)item).Self == ",") {
                        ops.Add(null);
                        continue;
                    }
                }

                ops[ops.Count - 1] = item;
            }

            foreach (var item in ops) {
                if (item == null) members.Add(new ExecutionResult());
                else members.Add(item.Operate(ctx));
            }

            if (members.Count > 0) return members[0];
            else return new ExecutionResult();
        }

        public List<ExecutionResult> OperateList(Compilation.RuntimeContext ctx) {
            List<ExecutionResult> members = new List<ExecutionResult>();
            if (EvaluateOperators.Count == 0) return new List<ExecutionResult>();
            List<OperatorStatement?> ops = new List<OperatorStatement?>() { null };
            foreach (var item in this.EvaluateOperators) {
                if (item is SelfOperation) {
                    if (((SelfOperation)item).Self == ",") {
                        ops.Add(null);
                        continue;
                    }
                }

                ops[ops.Count - 1] = item;
            }

            foreach (var item in ops) {
                if (item == null) members.Add(new ExecutionResult());
                else members.Add(item.Operate(ctx));
            }

            return members;
        }
    }
}
