using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Syntax {
   
    public class NoLessThanOperation : OperatorStatement{
        public override dynamic Operate(Compilation.RuntimeContext ctx) {
            return OperateByName("_nolessthan", ctx);
        }
    }
}
