using System;
using System.Linq;
using System.Collections.Generic;
using Simula.Scripting.Contexts;
using System.Dynamic;
using Simula.Scripting.Types;
using Simula.Scripting.Build;

namespace Simula.Scripting.Syntax
{
    public class BinaryOperation : OperatorStatement
    {
        IDictionary<string, object> store = new Dictionary<string, object>();
        dynamic? temp;
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
                } else if(this.Left is IndexOperation index) {
                    if (index.Left == null) return new Execution();
                    if (index.Right == null) return new Execution();
                    dynamic left = index.Left.Operate(ctx).Result;
                    dynamic right = index.Right.Operate(ctx).Result;

                    if (left is Matrix mleft) {
                        if (right is INumericalMatrix mright) {
                            mleft.Set(mright.ToIntegerMatrix(), result);
                        }
                    }

                } else if(this.Left is BraceOperation matrixRaw) {

                    // a syntax sugar. that the following syntax is allowed:
                    //     { a, b, c } = { expr1, expr2, expr3 }

                    // and it is equivalent as writing
                    //     a = expr1
                    //     b = expr2
                    //     c = expr3

                    List<OperatorStatement?> ops = new List<OperatorStatement?>();
                    foreach (var item in matrixRaw.EvaluateOperators) {
                        if(item is SelfOperation so)
                            if(so.Self == ";" || so.Self == ",") { continue; }
                        ops.Add(item);
                    }

                    if(result is Matrix mat) {
                        if(mat.total >= ops.Count && mat.dimension == 1) {
                            for (int i = 0; i < ops.Count; i++) {
                                BinaryOperation bin = new BinaryOperation();
                                bin.Operator = this.Operator;
                                bin.Left = ops[i];
                                bin.Right = new ObjectTransferOperation( mat.Get(new NumericalMatrix<int>(new int[1] { i + 1 }) ) );

                                bin.Operate(ctx);
                            }
                        }
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

                if(temp != null) {
                    return new Execution(ctx, temp._call(left, new dynamic[] { right }));
                }

                if( left is ValueType && right is ValueType) {
                    switch(this.Operator.Symbol) {
                        case "+": return new Execution(ctx, left + right);
                        case "-": return new Execution(ctx, left - right);
                        case "*": return new Execution(ctx, left * right);
                        case "/": return new Execution(ctx, left / right);
                        case "%": return new Execution(ctx, left % right);
                        case "<=": return new Execution(ctx, left <= right);
                        case "<": return new Execution(ctx, left < right);
                        case ">": return new Execution(ctx, left > right);
                        case ">=": return new Execution(ctx, left >= right);
                        case "==": return new Execution(ctx, left == right);
                    }
                }

                switch (this.Operator.Symbol) {
                    default:
                        var pair = DynamicRuntime.Registry.FirstOrDefault(((arg) => {
                            if (arg.Value.Symbol == this.Operator.Symbol &&
                                arg.Value.Type == this.Operator.Type) return true;
                            else return false;
                        }));

                        if (left._fields.ContainsKey(pair.Key)) {
                            if (temp == null) temp = left._fields[pair.Key];
                            return new Execution(ctx, ((Function)(left._fields[pair.Key]))?._call(left, new dynamic[] { right }));
                        }

                        if (temp == null) temp = ctx.FunctionCache[(string)left.type].Find((func) => {
                            return func.name == pair.Key;
                        });

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
                    left.Object.IsPublic = false;
                    left.Object.Type.AddRange(right.Types);
                    left.Object.Cache = new Contexts.Data.LocalData(left.Object.Name,
                         "(local) [" + left.Object.Type.JoinString(", ") + "] " + left.Object.Name, "");
                    return new TypeInference(left.Object);
                } else {
                    if (this.Left is SelfOperation self) {
                        var record = new CompletionRecord();
                        record.IsPublic = false;
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

        public override string Generate(GenerationContext ctx)
        {
            if (this.Left == null) return "";
            if (this.Right == null) return "";
            if (this.Operator.Symbol == "=") {
                if (this.Left is SelfOperation self) {
                    var check = ctx.ContainsMember(self.Self);
                    if (check == null) return "";
                    if (check?.Container.StartsWith("`") ?? true) {
                        string str = check?.Container.Remove(0, 1) ?? "";
                        if (str.StartsWith("`")) return "global." + self.Self + " = " + this.Right.Generate(ctx);
                        else return "scopes[" + str + "]." + self.Self + " = " + this.Right.Generate(ctx);
                    } else return check?.Container + self.Self + " = " + this.Right.Generate(ctx);
                } else if (this.Left is MemberOperation member) {
                    return member.Generate(ctx) + " = " + this.Right.Generate(ctx);
                } else if (this.Left is IndexOperation index) {
                    return index.Generate(ctx) + " = " + this.Right.Generate(ctx);
                } else if (this.Left is BraceOperation matrixRaw) {

                    List<OperatorStatement?> ops = new List<OperatorStatement?>();
                    foreach (var item in matrixRaw.EvaluateOperators) {
                        if (item is SelfOperation so)
                            if (so.Self == ";" || so.Self == ",") { continue; }
                        ops.Add(item);
                    }

                    string tempName = Guid.NewGuid().ToString().Replace("-", "_").ToLower();
                    string code = "dynamic " + tempName + " = " + this.Right.Generate(ctx) + "";
                    int i = 0;
                    foreach (var item in ops) {
                        code += ";\n" + ctx.Indention() + item.Generate(ctx) + " = " + tempName + ".__linear_get(" + i + ")";
                        i++;
                    }

                    return code;
                }

                return "";

            } else {
                switch (this.Operator.Symbol) {
                    default:
                        var pair = DynamicRuntime.Registry.FirstOrDefault(((arg) => {
                            if (arg.Value.Symbol == this.Operator.Symbol &&
                                arg.Value.Type == this.Operator.Type) return true;
                            else return false;
                        }));

                        return this.Left.Generate(ctx) + "." + pair.Key + "(new dynamic[]{" + this.Right.Generate(ctx) + "})";

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

        public override string Generate(GenerationContext ctx)
        {
            return Left?.Generate(ctx) + "." + Right?.Generate(ctx);
        }
    }
}

