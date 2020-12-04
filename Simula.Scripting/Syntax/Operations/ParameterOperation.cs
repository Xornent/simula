using System;
using System.Linq;
using System.Collections.Generic;
using Simula.Scripting.Contexts;

namespace Simula.Scripting.Syntax
{
    public class ParameterOperation : OperatorStatement
    {
        public override Execution Operate(DynamicRuntime ctx) {
            List<Execution> members = new List<Execution>();
            if (EvaluateOperators.Count == 0) return new Execution();
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
                if (item == null) members.Add(new Execution());
                else members.Add(item.Operate(ctx));
            }

            if (members.Count > 0) return members[0];
            else return new Execution();
        }

        public List<Execution> OperateList(DynamicRuntime ctx) {
            List<Execution> members = new List<Execution>();
            if (EvaluateOperators.Count == 0) return new List<Execution>();
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
                if (item == null) members.Add(new Execution());
                else members.Add(item.Operate(ctx));
            }

            return members;
        }
    }

    public class ParenthesisOperation : ParameterOperation { }
    public class BracketOperation : ParameterOperation { }
    public class BraceOperation : ParameterOperation { }

    public class IndexOperation : OperatorStatement
    {

    }

    public class FunctionCallOperation : OperatorStatement
    {
        
    }
}