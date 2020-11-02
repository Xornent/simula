﻿using Simula.Scripting.Debugging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Syntax {

    public  class NoMoreThanOperation: OperatorStatement {
        public override ExecutionResult Operate(Compilation.RuntimeContext ctx) {
            return OperateBySignal("<=", ctx);
        }
    }
}
