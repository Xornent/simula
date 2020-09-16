using System;
using System.Collections.Generic;
using System.Text;
using Simula.Scripting.Debugging;
using Simula.Scripting.Token;

namespace Simula.Scripting.Syntax {

    public class IfBlock : BlockStatement {
        public EvaluationStatement? Evaluation { get; set; } = null;
        public new void Parse(TokenCollection collection) {

            // If [EvaluationStatement]

            if (collection.Count > 1) {
                collection.RemoveAt(0);
                Evaluation = new EvaluationStatement();
                Evaluation.Parse(collection);
            } else collection[0].Error = new TokenizerException("SS0009");
        }

        public override (dynamic value, Debugging.ExecutableFlag flag) Execute(Compilation.RuntimeContext ctx) {
            if (Evaluation == null) return (Type.Global.Null, Debugging.ExecutableFlag.Else);
            var eval = Evaluation.Execute(ctx);
            if(eval.value is Type.Boolean) {
                if (((Type.Boolean)eval.value) == false) return (Type.Global.Null, Debugging.ExecutableFlag.Else);
            } else if(eval.value is Reflection.Variable) {
                if (((Reflection.Variable)(eval.value)).Object is Type.Boolean)
                    if (((Type.Boolean)((Reflection.Variable)(eval.value)).Object) == false) return (Type.Global.Null, Debugging.ExecutableFlag.Else);
            }

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
                        if (result.flag == ExecutableFlag.Else) {
                            skipif = false;
                        } else {
                            skipif = true;
                        }
                    }

                    switch (result.flag) {
                        case Debugging.ExecutableFlag.Pass:
                            break;
                        case Debugging.ExecutableFlag.Break:
                            return (Type.Global.Null, ExecutableFlag.Break);
                        case Debugging.ExecutableFlag.Continue:
                            return (Type.Global.Null, ExecutableFlag.Continue);
                        case Debugging.ExecutableFlag.Return:
                            return (result.value, Debugging.ExecutableFlag.Return);
                        default:
                            break;
                    }
                }
            }

            return (Type.Global.Null, Debugging.ExecutableFlag.Pass);
        }
    }
}
