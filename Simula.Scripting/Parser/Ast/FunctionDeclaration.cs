using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Parser.Ast
{
    public class FunctionDeclaration : DeclarationBlock
    {
        public FunctionDeclaration(Literal identifer) 
        {
            this.Identifer = identifer;
        }
        
        public IFunctionScope Scope { get; set; } 
            = new FunctionScope(new List<VariableDeclaration>(), new List<FunctionDeclaration>());
        public List<FunctionParameter> Parameters { get; set; } = new List<FunctionParameter>();
    }

    public class FunctionParameter
    {
        public List<Literal> Identifer { get; set; } = new List<Literal>();
        public List<IExpression> Type { get; set; } = new List<IExpression>();
        public List<IExpression> Default { get; set; } = new List<IExpression>();
    }
}
