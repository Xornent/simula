using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Text;
using Simula.Scripting.Debugging;
using Simula.Scripting.Reflection;
using Simula.Scripting.Token;

namespace Simula.Scripting.Syntax {

    public class ElseIfBlock : BlockStatement {
        public EvaluationStatement? Evaluation { get; set; } = null;
        public new void Parse(TokenCollection collection) {

            // eif [EvaluationStatement]

            if (collection.Count > 1) {
                collection.RemoveAt(0);
                Evaluation = new EvaluationStatement();
                Evaluation.Parse(collection);
            } else collection[0].Error = new TokenizerException("SS0009");
        }

        public override ExecutionResult Execute(Compilation.RuntimeContext ctx) {
            if (Evaluation == null) return new ExecutionResult();
            var eval = Evaluation.Execute(ctx);
            bool success = false;
            switch (eval.Result.Type) {
                case Reflection.MemberType.Class:
                    success = true;
                    break;
                case Reflection.MemberType.Instance:
                    if(eval.Result is ClrInstance inst)
                        if(inst.GetNative() is Type.Boolean b)
                            if(b == true)
                                success = true;
                    success = false;

                    var evalFunction = ((Instance)eval.Result).GetMember("__eval");
                    if(evalFunction.Result is Function) {
                        var result = ((Function)evalFunction.Result).Invoke(new List<Member>(), ref ctx);
                        if(result.Result is ClrInstance ins)
                            if(ins.GetNative() is Type.Boolean b)
                                if(b == true)
                                    success = true;
                        success = false;
                    } else success = true;
                    break;
                case Reflection.MemberType.Function:
                    success = true;
                    break;
                case Reflection.MemberType.Module:
                    success = true;
                    break;
                case Reflection.MemberType.Unknown:
                    success = false;
                    break;
                default:
                    break;
            }

            if (!success) return new ExecutionResult() { Flag = ExecutableFlag.Else };

            bool skipif = false;
            foreach (var item in this.Children) {
                if (item is DefinitionBlock) { } else {
                    if (item is ElseBlock || item is ElseIfBlock) {
                        if (skipif) { continue; }
                    } else {
                        if (skipif) skipif = false;
                    }

                    var result = item.Execute(ctx);

                    if (item is IfBlock || item is ElseIfBlock || item is ElseBlock) {
                        if (result.Flag == ExecutableFlag.Else) {
                            skipif = false;
                        } else {
                            skipif = true;
                        }
                    }

                    switch (result.Flag) {
                        case Debugging.ExecutableFlag.Pass:
                            break;
                        case Debugging.ExecutableFlag.Return:
                            return new ExecutionResult(result.Result, ctx) { Flag = ExecutableFlag.Return };
                        default:
                            break;
                    }
                }
            }

            return new ExecutionResult();
        }
    }
}
