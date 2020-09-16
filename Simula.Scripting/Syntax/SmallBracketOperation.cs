using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Syntax {
   
    public class SmallBracketOperation : OperatorStatement{
        public override dynamic Operate(Compilation.RuntimeContext ctx) {
            List<Reflection.Base> bases = new List<Reflection.Base>();
            if (EvaluateOperators.Count == 0) return Type.Global.Null;
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
                if (item == null) bases.Add(Type.Global.Null);
                else bases.Add(item.Operate(ctx));
            }

            if (bases.Count > 0) return bases[0];
            else return Type.Global.Null;
        }

        public dynamic OperateList(Compilation.RuntimeContext ctx) {
            List<Reflection.Base> bases = new List<Reflection.Base>();
            if (EvaluateOperators.Count == 0) return bases;
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
                if (item == null) bases.Add(Type.Global.Null);
                else bases.Add(item.Operate(ctx));
            }

            return bases;
        }
    }
}
