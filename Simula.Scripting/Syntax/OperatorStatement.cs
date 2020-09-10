using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Syntax {

    public class OperatorStatement : EvaluationStatement {
        public OperatorStatement? Left { get; set; }
        public OperatorStatement? Right { get; set; }
    }
}
