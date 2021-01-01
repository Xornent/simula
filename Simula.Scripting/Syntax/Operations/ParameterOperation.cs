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

                return new Execution(ctx, obj._call(null, args));
            }

            return new Execution();
        }
    }
}