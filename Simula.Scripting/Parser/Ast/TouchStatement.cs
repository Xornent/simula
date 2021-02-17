using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Parser.Ast
{
    public class TouchStatement : ICommandmentStatement
    {
        public TouchStatement(string toucher)
        {
            this.Toucher = toucher;
        }
        
        public TokenCollection Tokens { get; set; } = new TokenCollection();
        public string Toucher { get; set; }
    }
}
