using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Parser.Ast
{
    public class DataDeclaration : DeclarationBlock
    {
        public DataDeclaration(Literal identifer) 
        {
            this.Identifer = identifer;
        }
        
        public IDataScope Scope { get; set; } 
            = new DataScope(new List<VariableDeclaration>(), new List<FunctionDeclaration>());
        public IExpression? Inheritage { get; set; }
        public List<Literal> Assumptions { get; set; } = new List<Literal>();
    }
}
