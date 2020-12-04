using Simula.Scripting.Contexts;
using Simula.Scripting.Token;

namespace Simula.Scripting.Syntax
{

    public class BreakStatement : Statement
    {

        public override void Parse(TokenCollection sentence)
        {

        }

        public override Execution Execute(DynamicRuntime ctx)
        {
            return new Execution() { Flag = ExecutionFlag.Break };
        }
    }
}
