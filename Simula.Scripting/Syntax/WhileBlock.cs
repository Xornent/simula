using Simula.Scripting.Token;
using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Syntax {

    public class WhileBlock : BlockStatement {
        public EvaluationStatement? Evaluation { get; set; } = null;
        public new void Parse(TokenCollection collection) {

            // while [EvaluationStatement]

            if (collection.Count > 1) {
                collection.RemoveAt(0);
                Evaluation = new EvaluationStatement();
                Evaluation.Parse(collection);
            } else collection[0].Error = new TokenizerException("SS0009");
        }
    }
}
