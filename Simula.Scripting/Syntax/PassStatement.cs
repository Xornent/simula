using Simula.Scripting.Debugging;
using Simula.Scripting.Token;
using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Syntax {
   
    public class PassStatement : Statement {

        public override void Parse(TokenCollection sentence) {

        }

        public override ExecutionResult Execute(Compilation.RuntimeContext ctx) {
            return new ExecutionResult(0, ctx, ExecutableFlag.Pass);
        }
    }
}
