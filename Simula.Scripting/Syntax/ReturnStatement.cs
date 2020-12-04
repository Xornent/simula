using Simula.Scripting.Contexts;
using Simula.Scripting.Token;

namespace Simula.Scripting.Syntax
{

    public class ReturnStatement : Statement
    {
        private EvaluationStatement? Evaluation = null;
        public override void Parse(TokenCollection sentence)
        {
            if (sentence.Count == 1) {
                return;
            } else {
                sentence.RemoveAt(0);
                Evaluation = new EvaluationStatement();
                Evaluation.Parse(sentence);
            }
        }

        public override Execution Execute(DynamicRuntime ctx)
        {
            if (Evaluation == null) return new Execution() { Flag = ExecutionFlag.Return };
            else {
                var result = Evaluation.Execute(ctx);
                result.Flag = ExecutionFlag.Return;
                return result;
            }
        }
    }
}
