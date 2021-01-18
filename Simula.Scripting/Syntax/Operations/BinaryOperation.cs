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
                        ctx.SetMemberReferenceCheck(exp, raw, result, eval.fullName.Count == 0 ? "" : eval.fullName[0]);
                    } else {
                        ctx.SetMemberReferenceCheck(eval._fields, raw, result, eval.fullName.Count == 0 ? "" : eval.fullName[0]);
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
                            if (arg.Value.Symbol == this.Operator.Symbol &&
                                arg.Value.Type == this.Operator.Type) return true;
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

        public override TypeInference InferType(CompletionContext ctx)
        {
            if (this.Left == null) return new TypeInference();
            if (this.Right == null) return new TypeInference();
            var left = this.Left.InferType(ctx);
            var right = this.Right.InferType(ctx);

            if(this.Operator.Symbol == "=") {
                if(left.Object != null) {
                    left.Object.Type.AddRange(right.Types);
                    left.Object.Cache = new Contexts.Data.LocalData(left.Object.Name,
                         "(local) [" + left.Object.Type.JoinString(", ") + "] " + left.Object.Name, "");
                    return new TypeInference(left.Object);
                } else {
                    if (this.Left is SelfOperation self) {
                        var record = new CompletionRecord();
                        record.Type = right.Types;
                        record.Name = self.Self;

                        foreach (var item in record.Type) {
                            record.Children.Add(new CompletionTypeRecord(item));
                        }

                        record.Cache = new Contexts.Data.LocalData(record.Name,
                              "(local) [" + record.Type.JoinString(", ") + "] " + record.Name, "");
                        ctx.AccessibleRoots.Add(record);

                        return new TypeInference(record);
                    } else return new TypeInference();
                }

            } else {
                HashSet<string> types = new HashSet<string>();

                var pair = DynamicRuntime.Registry.FirstOrDefault(((arg) => {
                    if (arg.Value.Symbol == this.Operator.Symbol &&
                        arg.Value.Type == this.Operator.Type) return true;
                    else return false;
                }));

                foreach (var item in left.Types) {
                    if (ctx.ClassRecords.ContainsKey(item)) {
                        var find = ctx.ClassRecords[item].Children.Find((rec) => {
                            return rec.Name == pair.Key;
                        });

                        if (find != null)
                            types.AddRange(find.ReturnTypes);
                    }
                }

                return new TypeInference(types, null);
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

        public override TypeInference InferType(CompletionContext ctx)
        {
            if (this.Right == null) return new TypeInference();
            if (this.Left == null) return new TypeInference();
            if (this.Left.RawEvaluateToken.Count == 0) return new TypeInference();
            if (this.Right.RawEvaluateToken.Count == 0) return new TypeInference();
            var left = this.Left.InferType(ctx);
            bool existKey = false;
            CompletionRecord? record = null;
            foreach (var leftType in left.Types) {
                if ( leftType == "module") {
                    if (left.Object != null) {
                        var find = left.Object.Children.Find((rec) => {
                            return rec.Name == this.Right.RawEvaluateToken[0];
                        });
                        if (find != null) {
                            record = find;
                            existKey = true;
                            break;
                        }
                    }
                } else {
                    if(ctx.ClassRecords.ContainsKey(leftType)) {
                        var find = ctx.ClassRecords[leftType].Children.Find((rec) => {
                            return rec.Name == this.Right.RawEvaluateToken[0];
                        });
                        if (find != null) {
                            record = find;
                            existKey = true;
                            break;
                        }
                    }
                }
            }

            if (!existKey && record == null) {
                this.Right.RawEvaluateToken[0].Error = new Token.TokenizerException("ss1002");
                return new TypeInference();
            } else return new TypeInference(record);
        }
    }
}

