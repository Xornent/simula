using Simula.Scripting.Compilation;
using Simula.Scripting.Debugging;
using Simula.Scripting.Reflection;
using Simula.Scripting.Token;
using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Syntax {

    public class WhileBlock : BlockStatement {
        public EvaluationStatement? Evaluation { get; set; } = null;
        public new void Parse(TokenCollection collection) {

            // while [EvaluationStatement]

            if (collection.Count > 1) {
                collection.RemoveAt(0);
                Evaluation = new EvaluationStatement();
                Evaluation.Parse(collection);
            } else collection[0].Error = new TokenizerException("SS0009");
        }
        
        public override ExecutionResult Execute(RuntimeContext ctx) {
            bool evaluate = true;
            if (Evaluation == null) evaluate = false;
            else {
                var eval = Evaluation.Execute(ctx);
                switch (eval.Result.Type) {
                    case Reflection.MemberType.Class:
                        evaluate = true;
                        break;
                    case Reflection.MemberType.Instance:
                        if(eval.Result is ClrInstance inst)
                            if(inst.GetNative() is Type.Boolean b)
                                if(b == true)
                                    evaluate = true;
                        evaluate = false;

                        var evalFunction = ((Instance)eval.Result).GetMember("__eval");
                        if (evalFunction.Result is Function) {
                            var result = ((Function)evalFunction.Result).Invoke(new List<Member>(), ref ctx);
                            if(result.Result is ClrInstance ins)
                                if(ins.GetNative() is Type.Boolean b)
                                    if(b == true)
                                        evaluate = true;
                            evaluate = false;
                        } else evaluate = true;
                        break;
                    case Reflection.MemberType.Function:
                        evaluate = true;
                        break;
                    case Reflection.MemberType.Module:
                        evaluate = true;
                        break;
                    case Reflection.MemberType.Unknown:
                        evaluate = false;
                        break;
                    default:
                        break;
                }
            }

            while (evaluate) {
                bool jump = false;
                bool br = false;

                bool skipif = false;

                foreach (var item in this.Children) {
                    if (item is DefinitionBlock) { } else {

                        if(item is ElseBlock || item is ElseIfBlock) {
                            if(skipif) { continue; }
                        } else {
                            if (skipif) skipif = false;
                        }

                        var result = item.Execute(ctx);

                        if (item is IfBlock || item is ElseIfBlock || item is ElseBlock) {
                            if(result.Flag == ExecutableFlag.Else) {
                                skipif = false;
                            } else {
                                skipif = true;
                            }
                        }

                        if (result.Flag == ExecutableFlag.Break) {
                            br = true;
                            break;
                        } else if (result.Flag == ExecutableFlag.Return) {
                            return new ExecutionResult(result.Result, ctx) { Flag = ExecutableFlag.Return };
                        } else if(result.Flag == ExecutableFlag.Continue) {
                            jump = true;
                            break;
                        }
                    }
                }

                if (br == true) break;
                if (Evaluation == null) evaluate = false;
                else {
                    var eval = Evaluation.Execute(ctx);
                    switch (eval.Result.Type) {
                        case Reflection.MemberType.Class:
                            evaluate = true;
                            break;
                        case Reflection.MemberType.Instance:
                            if (eval.Pointer == 3) {
                                evaluate = true;
                                break;
                            } else if (eval.Pointer == 4) {
                                evaluate = false;
                                break;
                            }

                            var evalFunction = ((Instance)eval.Result).GetMember("__eval");
                            if (evalFunction.Result is Function) {
                                var result = ((Function)evalFunction.Result).Invoke(new List<Member>(), ref ctx);
                                if (result.Pointer == 3) evaluate = true;
                                else evaluate = false;
                            } else evaluate = true;
                            break;
                        case Reflection.MemberType.Function:
                            evaluate = true;
                            break;
                        case Reflection.MemberType.Module:
                            evaluate = true;
                            break;
                        case Reflection.MemberType.Unknown:
                            evaluate = false;
                            break;
                        default:
                            break;
                    }
                }
            }

            return new ExecutionResult();
        }
    }
}
