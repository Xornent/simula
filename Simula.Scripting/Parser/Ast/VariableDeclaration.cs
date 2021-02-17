using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Parser.Ast
{
    public class VariableDeclaration : DeclarationBlock
    {
        public VariableDeclaration(Literal identifer) 
        {
            this.Identifer = identifer;
        }
        
        public IExpression DataType { get; set; }
    }
}
