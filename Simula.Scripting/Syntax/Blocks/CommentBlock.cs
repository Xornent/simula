using Simula.Scripting.Build;
using System.Collections.Generic;

namespace Simula.Scripting.Syntax
{
    public class CommentBlock : BlockStatement
    {
        public List<string> Lines = new List<string>();

        public override string Generate(GenerationContext ctx)
        {
            if (Lines.Count == 0) return "";
            string comments = ctx.Indention() + "// " + Lines[0].Remove(0, 1).Trim() ;

            int index = 0;
            foreach (var item in Lines) {
                index++;
                if(index == 1) { continue; }
                comments += "\n" + ctx.Indention() + "// " + item.Remove(0, 1).Trim();
            }

            return comments;
        }
    }
}
