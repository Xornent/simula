using System;
using System.Linq;
using System.Collections.Generic;
using Simula.Scripting.Contexts;
using System.Dynamic;
using Simula.Scripting.Types;

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

            if (ops.Count > 0) return ops[0].Operate(ctx);
            else return new Execution();
        }

        public override TypeInference InferType(CompletionContext ctx)
        {
            if (EvaluateOperators.Count == 0) return new TypeInference();
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

            if (ops.Count > 0) return ops[0].InferType(ctx);
            else return new TypeInference();
        }

        public List<Execution> FullExecution(DynamicRuntime ctx) {
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

        public List<dynamic> DynamicFullExecution(DynamicRuntime ctx)
        {
            List<dynamic> members = new List<dynamic>();
            if (EvaluateOperators.Count == 0) return new List<dynamic>();
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
                if (item == null) {
                    members.Add(Types.Null.NULL);
                    continue;
                }

                dynamic result = item.Operate(ctx).Result;
                while (result is Execution) { result = result.Result; }
                if (item == null) members.Add(Types.Null.NULL);
                
                members.Add(result);
            }

            return members;
        }
    }

    public class ParenthesisOperation : ParameterOperation { }
    public class BracketOperation : ParameterOperation { }
    public class BraceOperation : ParameterOperation 
    {
        public override Execution Operate(DynamicRuntime ctx)
        {
            Types.Array arr = new Types.Array();
            List<dynamic> members = new List<dynamic>();
            if (EvaluateOperators.Count == 0) return new Execution(ctx, arr);
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
                if (item == null) members.Add(Types.Null.NULL);
                else members.Add(item.Operate(ctx).Result);
            }

            arr = new Types.Array(members.ToArray());
            return new Execution(ctx, arr);
        }

        public override TypeInference InferType(CompletionContext ctx)
        {
            return new TypeInference(new HashSet<string> { "sys.array" }, null);
        }
    }

    public class IndexOperation : OperatorStatement
    {

    }

    public class FunctionCallOperation : OperatorStatement
    {
        public override Execution Operate(DynamicRuntime ctx)
        {
            if (this.Left == null) return new Execution();
            dynamic[] args = ((ParameterOperation)(this.Right)).DynamicFullExecution(ctx).ToArray();
            dynamic obj = this.Left.Operate(ctx).Result;

            if(obj.type == "sys.class") {
                var list = args.ToList();
                list.Insert(0, ctx);
                return new Execution(ctx, Types.Class._create._call(obj, list.ToArray()));
            } else if(obj.type == "sys.func") {
                if(this.Left is MemberOperation member) {
                    var caller = member.Left.Operate(ctx).Result;
                    return new Execution(ctx, obj._call(caller, args));
                }

                // note that a member function (when called without 'this.' prefix will be 
                // considered as a static function and passes 'null' to 'self'. however, this
                // seems not to trigger any bugs in execution.

                // a possible solution is to check if the current scope contains a 'this' 
                // variable, which will always be added to a function scope if it is called 
                // as a class member. then send that as 'self'.

                return new Execution(ctx, obj._call(null, args));
            }

            return new Execution();
        }

        public override TypeInference InferType(CompletionContext ctx)
        {
            if (this.Left == null) return new TypeInference();
            var left = this.Left.InferType(ctx);
            HashSet<string> ret = new HashSet<string>();

            if(left.Types.Contains("sys.func") || left.Types.Contains("sys.class") || left.Types.Contains("any") || left.Types.Contains("ref")) {
                if(left.Object !=null) {
                    if (left.Types.Contains("sys.func")) ret.AddRange(left.Object.ReturnTypes);
                    else if (left.Types.Contains("sys.class")) ret.Add(left.Object.FullName);
                    else ret.Add("any");
                }
            } else {
                this.Left.RawEvaluateToken.Last().Error = new Token.TokenizerException("ss1001");
            }

            return new TypeInference(ret, null);
        }
    }
}