using Simula.Scripting.Build;
using Simula.Scripting.Contexts;
using Simula.Scripting.Token;

namespace Simula.Scripting.Syntax
{
    public class BreakStatement : Statement
    {
        public override void Parse(TokenCollection sentence)
        {
            this.RawToken.AddRange(sentence);
        }

        public override Execution Execute(DynamicRuntime ctx)
        {
            return new Execution() { Flag = ExecutionFlag.Break };
        }

        public override string Generate(GenerationContext ctx)
        {
            string code = ctx.Indention() + "break;";
            return code;
        }
    }
}
