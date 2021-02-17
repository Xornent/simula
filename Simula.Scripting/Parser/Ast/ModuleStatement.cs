using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Parser.Ast
{
    public class ModuleStatement : ICommandmentStatement
    {
        public ModuleStatement(string module)
        {
            this.Module = module;
        }
        
        public TokenCollection Tokens { get; set; } = new TokenCollection();
        public string Module { get; set; }
    }
}
