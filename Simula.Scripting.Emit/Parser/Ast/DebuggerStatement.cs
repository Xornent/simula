using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Parser.Ast
{
    public class DebuggerStatement : ICommandmentStatement
    {
        public TokenCollection Tokens { get; set; } = new TokenCollection();
        public bool Toggle { get; set; } = true;
    }
}
