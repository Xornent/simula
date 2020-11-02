using Simula.Scripting.Debugging;
using Simula.Scripting.Token;
using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Syntax {

    public class ElseBlock : BlockStatement {

        public override void Parse(TokenCollection sentence) {

        }

        public override ExecutionResult Execute(Compilation.RuntimeContext ctx) {
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
