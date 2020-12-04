using Simula.Scripting.Contexts;
using Simula.Scripting.Token;

namespace Simula.Scripting.Syntax
{
    public class ElseBlock : BlockStatement
    {
        public override void Parse(TokenCollection sentence)
        {

        }

        public override Execution Execute(DynamicRuntime ctx)
        {
            return new BlockStatement() { Children = this.Children }.Execute(ctx);
        }
    }
}
