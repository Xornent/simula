using Simula.Scripting.Compilation;
using Simula.Scripting.Debugging;
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

        public override (dynamic value, ExecutableFlag flag) Execute(RuntimeContext ctx) {
            bool evaluate = true;
            if (Evaluation == null) evaluate = false;
            else {
                var eval = Evaluation.Execute(ctx);
                if (eval.value is Type.Boolean) {
                    if (((Type.Boolean)eval.value) == false) evaluate = false;
                } else if (eval.value is Reflection.Variable) {
                    if (((Reflection.Variable)(eval.value)).Object is Type.Boolean)
                        if (((Type.Boolean)((Reflection.Variable)(eval.value)).Object) == false) evaluate = false;
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
                            if(result.flag == ExecutableFlag.Else) {
                                skipif = false;
                            } else {
                                skipif = true;
                            }
                        }

                        if (result.flag == ExecutableFlag.Break) {
                            br = true;
                            break;
                        } else if(result.flag == ExecutableFlag.Return) {
                            return (result.value, ExecutableFlag.Return);
                        } else if(result.flag == ExecutableFlag.Continue) {
                            jump = true;
                            break;
                        }
                    }
                }

                if (br == true) break;
                if (Evaluation == null) evaluate = false;
                else {
                    var eval = Evaluation.Execute(ctx);
                    if (eval.value is Type.Boolean) {
                        if (((Type.Boolean)eval.value) == false) evaluate = false;
                    } else if (eval.value is Reflection.Variable) {
                        if (((Reflection.Variable)(eval.value)).Object is Type.Boolean)
                            if (((Type.Boolean)((Reflection.Variable)(eval.value)).Object) == false) evaluate = false;
                    }
                }
            }

            return (Type.Global.Null, Debugging.ExecutableFlag.Pass);
        }
    }
}
