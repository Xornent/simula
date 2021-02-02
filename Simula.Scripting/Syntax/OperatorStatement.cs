using Simula.Scripting.Build;
using Simula.Scripting.Contexts;
using System.Collections.Generic;

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

        public override TypeInference InferType(CompletionContext ctx)
        {
            return new TypeInference( new HashSet<string>() { "any" }, null);
        }

        public override string Generate(GenerationContext ctx)
        {
            return "";
        }
    }
}
