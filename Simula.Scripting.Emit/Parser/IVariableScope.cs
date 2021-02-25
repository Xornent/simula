using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Parser
{
    public interface IVariableScope
    {
        List<Ast.VariableDeclaration> VariableDeclarations { get; set; }
    }

    public class VariableScope : IVariableScope
    {
        public VariableScope(List<Ast.VariableDeclaration> declarations) 
        {
            this.VariableDeclarations = declarations;
        }

        public List<Ast.VariableDeclaration> VariableDeclarations { get; set; } 
            = new List<Ast.VariableDeclaration>();
    }
}
