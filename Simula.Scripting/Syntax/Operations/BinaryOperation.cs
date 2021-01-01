using System;
using System.Linq;
using System.Collections.Generic;
using Simula.Scripting.Contexts;
using System.Dynamic;
using Simula.Scripting.Types;

namespace Simula.Scripting.Syntax
{
    public class BinaryOperation : OperatorStatement
    {
        IDictionary<string, object> store = new Dictionary<string, object>();
        public override Execution Operate(DynamicRuntime ctx)
        {
            if (store.Count == 0) store = (IDictionary<string, object>)ctx.Store;
            if (this.Left == null) return new Execution();
            if (this.Right == null) return new Execution();
            if (this.Operator.Symbol == "=") {
                dynamic result = this.Right.Operate(ctx).Result;

                if (this.Left is SelfOperation self) {
                    ctx.SetMemberReferenceCheck(self.Self, result);
                } else if (this.Left is MemberOperation member) {
                    string raw = member.Right.RawEvaluateToken[0];
                    dynamic eval = member.Left.Operate(ctx).Result;
                    if (eval is ExpandoObject exp) {
                        ctx.SetMemberReferenceCheck(exp, raw, result, eval.fullName[0]);
                    } else {
                        ctx.SetMemberReferenceCheck(eval._fields, raw, result, eval.fullName[0]);
                    }
                }

                return new Execution(ctx, result);

            } else {
                var left = this.Left.Operate(ctx).Result;
                var right = this.Right.Operate(ctx).Result;
                while (left is Execution) left = left.Result;
                while (right is Execution) right = right.Result;
                if (left is Reference re) { left = re.GetDynamic(); }
                if (right is Reference refer) { right = refer.GetDynamic(); }

                switch (this.Operator.Symbol) {
                    default:
                        var pair = DynamicRuntime.Registry.FirstOrDefault(((arg) => {
                            if (arg.Value.Symbol == this.Operator.Symbol) return true;
                            else return false;
                        }));

                        if (left._fields.ContainsKey(pair.Key)) {
                            return new Execution(ctx, ((Function)(left._fields[pair.Key]))?._call(left, new dynamic[] { right }));
                        }

                        return new Execution(ctx, ctx.FunctionCache[(string)left.type].Find((func) => {
                                  return func.name == pair.Key;
                              })?._call(left, new dynamic[] { right }));
                }
            }
        }
    }

    public class MemberOperation : BinaryOperation
    {
        public override Execution Operate(DynamicRuntime ctx)
        {
            string raw = this.Right.RawEvaluateToken[0];
            var left = this.Left.Operate(ctx).Result;
            if (left is ExpandoObject expando) {
                IDictionary<string, object> dict = (IDictionary<string, object>)expando;
                if (dict.ContainsKey(raw))
                    if (dict[raw] is Reference re)
                        return new Execution(ctx, re.GetDynamic());
                    else return new Execution(ctx, dict[raw]);
                else return new Execution();
            } else {
                if (left._fields.ContainsKey(raw)) {
                    return new Execution(ctx, left._fields[raw]);
                }

                string type = left.type;
                if (!ctx.FunctionCache.ContainsKey(type)) {
                    ctx.CacheFunction(type, left.GetType());
                }

                var function = ctx.FunctionCache[type].Find((func) => {
                    if (func.name == raw) return true;
                    else return false;
                });
                if (function == null) return new Execution();
                return new Execution(ctx, function);
            }
        }
    }
}

