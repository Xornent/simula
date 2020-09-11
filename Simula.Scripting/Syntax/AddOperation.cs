using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Syntax {
   
    public class AddOperation : OperatorStatement {

        public new dynamic? Operate(Compilation.RuntimeContext ctx) {
            if (this.Left == null) return null;
            if (this.Right == null) return null;
            return null;
        }
    }
}
