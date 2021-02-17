using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Parser
{
    public interface IFunctionScope : IVariableScope
    {
        public List<Ast.FunctionDeclaration> FunctionDeclarations { get; set; }
    }

    public class FunctionScope : IFunctionScope
    {
        public FunctionScope(List<Ast.VariableDeclaration> variables, List<Ast.FunctionDeclaration> functions)
        {
            this.VariableDeclarations = variables;
            this.FunctionDeclarations = functions;
        }

        public List<Ast.VariableDeclaration> VariableDeclarations { get; set; } = new List<Ast.VariableDeclaration>();
        public List<Ast.FunctionDeclaration> FunctionDeclarations { get; set; } = new List<Ast.FunctionDeclaration>();
    }
}
