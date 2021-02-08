using Simula.Scripting.Build;
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

        public override string Generate(GenerationContext ctx)
        {
            BlockStatement block = new BlockStatement();
            block.Children = this.Children;
            block.Nonmodifier = true;
            ctx.PushScope("Else");
            string str = "else " + block.Generate(ctx);
            ctx.PopScope();
            return str;
        }
    }
}
