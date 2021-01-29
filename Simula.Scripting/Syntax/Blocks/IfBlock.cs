using Simula.Scripting.Contexts;
using Simula.Scripting.Token;
using System;
using System.Collections.Generic;

namespace Simula.Scripting.Syntax
{

    public class IfBlock : BlockStatement
    {
        public EvaluationStatement? Evaluation { get; set; } = null;
        public List<ElseIfBlock> ElseifBlocks { get; set; } = new List<ElseIfBlock>();
        public ElseBlock? ElseBlock { get; set; } = null;

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
            if (Evaluation == null) return new Execution();
            var eval = Evaluation.Execute(ctx);
            if(!((bool)(eval.Result))) return new Execution() { Flag = ExecutionFlag.Else };

            return new BlockStatement() { Children = this.Children }.Execute(ctx);
        }
    }
}
