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

        public IExpression ReturnType { get; set; } = new Literal("null");
        public List<FunctionParameter> Parameters { get; set; } = new List<FunctionParameter>();
    }

    public class FunctionParameter
    {
        public List<Literal> Modifers { get; set; } = new List<Literal>();
        public Literal Identifer { get; set; } = new Literal("_");
        public IExpression Type { get; set; } = new Literal("_");
        public IExpression Default { get; set; } = new Literal("_");
    }
}
