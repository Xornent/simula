using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Parser
{
    public interface IFunctionScope : IVariableScope
    {
        List<Ast.FunctionDeclaration> FunctionDeclarations { get; set; }
        List<Ast.DataDeclaration> DataDeclarations { get; set; }
    }

    public class FunctionScope : IFunctionScope
    {
        public FunctionScope(List<Ast.VariableDeclaration> variables, List<Ast.FunctionDeclaration> functions, List<Ast.DataDeclaration> datatypes)
        {
            this.VariableDeclarations = variables;
            this.FunctionDeclarations = functions;
            this.DataDeclarations = datatypes;
        }

        public List<Ast.VariableDeclaration> VariableDeclarations { get; set; } = new List<Ast.VariableDeclaration>();
        public List<Ast.FunctionDeclaration> FunctionDeclarations { get; set; } = new List<Ast.FunctionDeclaration>();
        public List<Ast.DataDeclaration> DataDeclarations { get; set; } = new List<Ast.DataDeclaration>();
    }
}
