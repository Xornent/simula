using Simula.Scripting.Debugging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Syntax {

    public class SquareBracketOperation : OperatorStatement {

        // 这应该返回一个 dimension 对象. 因为它在程序中较 {} 和 () 更灵活.

        public override ExecutionResult Operate(Compilation.RuntimeContext ctx) {
#if DEBUG
            throw new NotImplementedException();
#endif
            return new ExecutionResult();
        }
    }
}
