using Simula.Scripting.Contexts;
using Simula.Scripting.Types;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace Simula.Scripting.Syntax
{
    public class UnaryOperation : OperatorStatement
    {
        IDictionary<string, object> store = new Dictionary<string, object>();
        public override Execution Operate(DynamicRuntime ctx)
        {
            if (store.Count == 0) store = (IDictionary<string, object>)ctx.Store;

            switch (this.Operator.Symbol) {
                default:
                    if (this.Operator.Type == OperatorType.UnaryLeft) {
                        if (this.Right == null) return new Execution();
                        var right = this.Right.Operate(ctx).Result;
                        while (right is Execution) right = right.Result;
                        if (right is Reference refer) { right = refer.GetDynamic(); }

                        var pair = DynamicRuntime.Registry.FirstOrDefault(((arg) => {
                            if (arg.Value.Symbol == this.Operator.Symbol &&
                                arg.Value.Type == this.Operator.Type) return true;
                            else return false;
                        }));

                        if (right._fields.ContainsKey(pair.Key)) {
                            return new Execution(ctx, ((Function)(right._fields[pair.Key]))?._call(right, new dynamic[] { }));
                        }

                        return new Execution(ctx, ctx.FunctionCache[(string)right.type].Find((func) => {
                            return func.name == pair.Key;
                        })?._call(right, new dynamic[] { }));
                    } else if (this.Operator.Type == OperatorType.UnaryRight) {
                        if (this.Left == null) return new Execution();
                        var left = this.Left.Operate(ctx).Result;
                        while (left is Execution) left = left.Result;
                        if (left is Reference re) { left = re.GetDynamic(); }

                        var pair = DynamicRuntime.Registry.FirstOrDefault(((arg) => {
                            if (arg.Value.Symbol == this.Operator.Symbol &&
                                arg.Value.Type == this.Operator.Type) return true;
                            else return false;
                        }));

                        if (left._fields.ContainsKey(pair.Key)) {
                            return new Execution(ctx, ((Function)(left._fields[pair.Key]))?._call(left, new dynamic[] { }));
                        }

                        return new Execution(ctx, ctx.FunctionCache[(string)left.type].Find((func) => {
                            return func.name == pair.Key;
                        })?._call(left, new dynamic[] { }));
                    } else return new Execution();
            }

        }
    }
}