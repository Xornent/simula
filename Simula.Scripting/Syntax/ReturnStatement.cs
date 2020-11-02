using Simula.Scripting.Compilation;
using Simula.Scripting.Debugging;
using Simula.Scripting.Token;
using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Syntax {
    
    public class ReturnStatement : Statement {
        EvaluationStatement? Evaluation = null;
        public override void Parse(TokenCollection sentence) {
            if(sentence.Count == 1) {
                return;
            } else {
                sentence.RemoveAt(0);
                Evaluation = new EvaluationStatement();
                Evaluation.Parse(sentence);
            }
        }

        public override ExecutionResult Execute(RuntimeContext ctx) {
            if (Evaluation == null) return new ExecutionResult();
            else {
                var result = Evaluation.Execute(ctx);
                result.Flag = ExecutableFlag.Return;
                return result;
            }
        }
    }
}
