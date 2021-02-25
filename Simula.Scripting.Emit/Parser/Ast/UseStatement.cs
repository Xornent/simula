using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Parser.Ast
{
    public class UseStatement : ICommandmentStatement
    {
        public UseStatement(string use)
        {
            this.Module = use;
        }
        
        public TokenCollection Tokens { get; set; } = new TokenCollection();
        public string Module { get; set; }
    }
}
