using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Parser.Ast
{
    public class EmptyStatement : IStatement
    {
        public TokenCollection Tokens { get; set; } = new TokenCollection();
    }
}
