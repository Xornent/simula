using Simula.Scripting.Build;
using Simula.Scripting.Contexts;
using Simula.Scripting.Types;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace Simula.Scripting.Syntax
{
    public class UnaryOperation : OperatorStatement
    {
        dynamic? operatorCache;
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

                        if (operatorCache != null) return new Execution(ctx, operatorCache._call(right, null));

                        var pair = DynamicRuntime.Registry.FirstOrDefault(((arg) => {
                            if (arg.Value.Symbol == this.Operator.Symbol &&
                                arg.Value.Type == this.Operator.Type) return true;
                            else return false;
                        }));

                        if (right._fields.ContainsKey(pair.Key)) {
                            if (operatorCache == null) operatorCache = ((Function)(right._fields[pair.Key]));
                            return new Execution(ctx, ((Function)(right._fields[pair.Key]))?._call(right, new dynamic[] { }));
                        }

                        if (operatorCache == null) operatorCache = ctx.FunctionCache[(string)right.type].Find((func) => {
                            return func.name == pair.Key;
                        });

                        return new Execution(ctx, ctx.FunctionCache[(string)right.type].Find((func) => {
                            return func.name == pair.Key;
                        })?._call(right, new dynamic[] { }));
                    } else if (this.Operator.Type == OperatorType.UnaryRight) {
                        if (this.Left == null) return new Execution();
                        var left = this.Left.Operate(ctx).Result;
                        while (left is Execution) left = left.Result;
                        if (left is Reference re) { left = re.GetDynamic(); }

                        if (operatorCache != null) return new Execution(ctx, operatorCache._call(left, null));

                        var pair = DynamicRuntime.Registry.FirstOrDefault(((arg) => {
                            if (arg.Value.Symbol == this.Operator.Symbol &&
                                arg.Value.Type == this.Operator.Type) return true;
                            else return false;
                        }));

                        if (left._fields.ContainsKey(pair.Key)) {
                            if (operatorCache == null) operatorCache = ((Function)(left._fields[pair.Key]));
                            return new Execution(ctx, ((Function)(left._fields[pair.Key]))?._call(left, new dynamic[] { }));
                        }

                        if (operatorCache == null) operatorCache = ctx.FunctionCache[(string)left.type].Find((func) => {
                            return func.name == pair.Key;
                        });

                        return new Execution(ctx, ctx.FunctionCache[(string)left.type].Find((func) => {
                            return func.name == pair.Key;
                        })?._call(left, new dynamic[] { }));
                    } else return new Execution();
            }

        }

        public override TypeInference InferType(CompletionContext ctx)
        {
            if (this.Operator.Type == OperatorType.UnaryLeft) {
                if (this.Right == null) return new TypeInference();
                if (this.Right.RawEvaluateToken.Count == 0) return new TypeInference();
                var right = this.Right.InferType(ctx);

                HashSet<string> types = new HashSet<string>();

                var pair = DynamicRuntime.Registry.FirstOrDefault(((arg) => {
                    if (arg.Value.Symbol == this.Operator.Symbol &&
                        arg.Value.Type == this.Operator.Type) return true;
                    else return false;
                }));

                foreach (var item in right.Types) {
                    if (ctx.ClassRecords.ContainsKey(item)) {
                        var find = ctx.ClassRecords[item].Children.Find((rec) => {
                            return rec.Name == pair.Key;
                        });

                        if (find != null)
                            types.AddRange(find.ReturnTypes);
                    }
                }

                return new TypeInference(types, null);

            } else if (this.Operator.Type == OperatorType.UnaryRight) {
                if (this.Left == null) return new TypeInference();
                if (this.Left.RawEvaluateToken.Count == 0) return new TypeInference();
                var left = this.Left.InferType(ctx);

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

            } else return new TypeInference();
        }

        public override string Generate(GenerationContext ctx)
        {
            string op = DynamicRuntime.Registry.First((obj) => {
                return (obj.Value.Symbol == this.Operator.Symbol && obj.Value.Type == this.Operator.Type);
            }).Key;

            switch (this.Operator.Symbol) {
                default:
                    if (this.Operator.Type == OperatorType.UnaryLeft) {
                        return this.Right?.Generate(ctx) + "." + op + "()";
                    } else if (this.Operator.Type == OperatorType.UnaryRight) {
                        return this.Left?.Generate(ctx) + "." + op + "()";
                    } else return "";
            }
        }
    }
}