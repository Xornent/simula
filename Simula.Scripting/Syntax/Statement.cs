using Simula.Scripting.Contexts;
using Simula.Scripting.Token;

namespace Simula.Scripting.Syntax
{
    public class Statement
    {
        public virtual void Parse(TokenCollection sentence)
        {
            return;
        }

        public virtual Execution Execute(DynamicRuntime ctx)
        {
            return new Execution() { Flag = ExecutionFlag.Go };
        }
    }
}
