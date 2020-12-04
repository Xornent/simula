using Simula.Scripting.Contexts;

namespace Simula.Scripting.Syntax
{

    public class OperatorStatement : EvaluationStatement
    {
        public OperatorStatement? Left { get; set; }
        public OperatorStatement? Right { get; set; }
        public Operator Operator { get; set; }

        public virtual Execution Operate(DynamicRuntime ctx)
        {
            return new Execution();
        }
    }
}
