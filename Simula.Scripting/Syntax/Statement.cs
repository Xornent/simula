using Simula.Scripting.Contexts;
using Simula.Scripting.Token;
using Simula.Scripting.Build;

namespace Simula.Scripting.Syntax
{
    public class Statement
    {
        public Token.TokenCollection RawToken = new TokenCollection();

        public virtual void Parse(TokenCollection sentence)
        {
            return;
        }

        public virtual Execution Execute(DynamicRuntime ctx)
        {
            return new Execution() { Flag = ExecutionFlag.Go };
        }

        public virtual string Generate(GenerationContext ctx) { return ""; }
    }
}
