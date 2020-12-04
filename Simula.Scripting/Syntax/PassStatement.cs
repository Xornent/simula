using Simula.Scripting.Contexts;
using Simula.Scripting.Token;

namespace Simula.Scripting.Syntax
{

    public class PassStatement : Statement
    {

        public override void Parse(TokenCollection sentence)
        {

        }

        public override Execution Execute(DynamicRuntime ctx)
        {
            return new Execution() { Flag = ExecutionFlag.Go };
        }
    }
}
