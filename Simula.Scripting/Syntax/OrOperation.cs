using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Syntax {
   
    public class OrOperation : OperatorStatement {
        public override dynamic Operate(Compilation.RuntimeContext ctx) {
            return OperateByName("_or", ctx);
        }
    }
}
