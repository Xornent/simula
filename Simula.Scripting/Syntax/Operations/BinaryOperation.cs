using System;
using System.Linq;
using System.Collections.Generic;
using Simula.Scripting.Contexts;

namespace Simula.Scripting.Syntax
{
    public class BinaryOperation : OperatorStatement
    {
        Execution? execution;
        Types.Boolean bt = new Types.Boolean(true);
        public override Execution Operate(DynamicRuntime ctx) {
            if(execution==null) execution=new Execution(ctx,bt);
            /*if(this.Left == null) return new Execution();
            if(this.Right == null) return new Execution();*/
            if(this.Operator.Symbol == "=") {
                //IDictionary<string, object> store = (IDictionary<string, object>) ctx.Store;
                //var result = this.Left?.Operate(ctx).Result;
                //store[this.Left.RawEvaluateToken[0]] = this.Right?.Operate(ctx).Result ?? Types.Null.NULL;
                //return new Execution(ctx, result);

            } else {
                //var left = this.Left.Operate(ctx).Result;
                //var right = this.Right.Operate(ctx).Result;

                switch (this.Operator.Symbol) {
                    default: 
                        var pair = DynamicRuntime.Registry.First(((arg) => {
                            if(arg.Value.Symbol == this.Operator.Symbol) return true;
                            else return false;
                        }));
                    /*
                        foreach(var op in DynamicRuntime.Registry) {
                            if(op.Value.Symbol == this.Operator.Symbol) {
                                // return new Execution(ctx, ((Types.Function)(left.GetType().GetField(op.Key).GetValue(null)))
                                //        ._call(left, new dynamic[] {right}));
                            }
                        } */break;
                }
            }

            return execution ?? new Execution(ctx, bt);
        }
    }
}

