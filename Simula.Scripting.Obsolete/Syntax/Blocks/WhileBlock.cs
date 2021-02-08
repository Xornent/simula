using Simula.Scripting.Build;
using Simula.Scripting.Contexts;
using Simula.Scripting.Token;
using System;
using System.Collections.Generic;

namespace Simula.Scripting.Syntax
{

    public class WhileBlock : BlockStatement
    {
        public EvaluationStatement? Evaluation { get; set; } = null;
        public new void Parse(TokenCollection collection)
        {
            this.RawToken.AddRange(collection);
            if (collection.Count > 1) {
                collection.RemoveAt(0);
                Evaluation = new EvaluationStatement();
                Evaluation.Parse(collection);
            } else collection[0].Error = new TokenizerException("SS0009");

            foreach (var item in this.Children) {
                this.RawToken.AddRange(item.RawToken);
            }
        }

        public override Execution Execute(DynamicRuntime ctx)
        {
            var code = new BlockStatement() { Children = this.Children };
            var evaluation = (bool?)(this.Evaluation?.Execute(ctx).Result) ?? false;
            while (evaluation) {
                var result = code.Execute(ctx);
                if(result.Flag == ExecutionFlag.Continue) continue;
                if(result.Flag == ExecutionFlag.Break) break;

                evaluation = (bool?)(this.Evaluation?.Execute(ctx).Result) ?? false;
            }
            
            return new Execution();
        }

        public override string Generate(GenerationContext ctx)
        {
            BlockStatement block = new BlockStatement();
            block.Children = this.Children;
            block.Nonmodifier = true;
            ctx.PushScope("While");
            string str = "while ( " + (this.Evaluation?.Generate(ctx) ?? "true") + " )" + block.Generate(ctx);
            ctx.PopScope();
            return str;
        }
    }
}
