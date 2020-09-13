using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Syntax {
    public class SelfOperation : OperatorStatement {
        public Token.Token Self { get; set; }

        public override dynamic Operate(Compilation.RuntimeContext ctx) {
            return ctx.GetMember(Self.Value.Replace("\n",""));
        }
    }
}
