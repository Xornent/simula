using System;
using System.Linq;
using System.Collections.Generic;
using Simula.Scripting.Contexts;
using System.Dynamic;

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
                    store[self.Self] = result;
                } else if (this.Left is MemberOperation member) {
                    string raw = member.Right.RawEvaluateToken[0];
                    dynamic eval = member.Left.Operate(ctx).Result;
                    if (eval is ExpandoObject exp) {
                        IDictionary<string, object> contents = (IDictionary<string, object>)exp;
                        contents[raw] = result;
                    } else {
                        if (eval._fields.ContainsKey(raw)) {
                            eval._fields[raw] = result;
                        } else {
                            eval._fields.Add(raw, result);
                        }
                    }
                }
                return new Execution(ctx, result);

            } else {
                var left = this.Left.Operate(ctx).Result;
                var right = this.Right.Operate(ctx).Result;

                switch (this.Operator.Symbol) {
                    default:
                        var pair = DynamicRuntime.Registry.FirstOrDefault(((arg) => {
                            if (arg.Value.Symbol == this.Operator.Symbol) return true;
                            else return false;
                        }));

                        return new Execution(ctx, ((Types.Function)(left.GetType().GetField(pair.Key).GetValue(null)))
                              ._call(left, new dynamic[] { right }));
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
                    return new Execution(ctx, dict[raw]);
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

