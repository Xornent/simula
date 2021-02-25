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
        
        public List<FunctionParameter> Fields { get; set; } = new List<FunctionParameter>();
        public List<IExpression> Inheritage { get; set; } = new List<IExpression>();
        public List<Literal> Assumptions { get; set; } = new List<Literal>();
    }
}
