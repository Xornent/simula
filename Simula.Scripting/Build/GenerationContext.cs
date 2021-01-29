using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Build
{
    public class GenerationContext
    {
        public int Indent = 4;
        public int IndentionLevel = 0;
    }

    public static class Generation
    {
        public static string Indention(this GenerationContext ctx)
        {
            if (ctx.Indent == 0) return "";
            string indention = "";
            int count = 0;
            while(count < ctx.Indent * ctx.IndentionLevel) {
                indention += " ";
                count++;
            }

            return indention;
        }
    }
}
