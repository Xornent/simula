using Simula.Scripting.Token;
using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Syntax {

    public class BreakStatement : Statement {

        public override void Parse(TokenCollection sentence) {

        }

        public override (dynamic value, Debugging.ExecutableFlag flag) Execute(Compilation.RuntimeContext ctx) {
            return (Type.Global.Null, Debugging.ExecutableFlag.Break);
        }
    }
}
