using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Parser.Ast
{
    public class BreakStatement : IStatement
    {
        public TokenCollection Tokens { get; set; } = new TokenCollection();
    }
}
