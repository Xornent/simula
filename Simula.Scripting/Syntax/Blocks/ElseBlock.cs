using Simula.Scripting.Contexts;
using Simula.Scripting.Token;

namespace Simula.Scripting.Syntax
{
    public class ElseBlock : BlockStatement
    {
        public override void Parse(TokenCollection sentence)
        {
            this.RawToken.AddRange(sentence);
            foreach (var item in this.Children) {
                this.RawToken.AddRange(item.RawToken);
            }
        }

        public override Execution Execute(DynamicRuntime ctx)
        {
            return new BlockStatement() { Children = this.Children }.Execute(ctx);
        }
    }
}
