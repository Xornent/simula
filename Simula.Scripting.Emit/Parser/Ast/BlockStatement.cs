using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Parser.Ast
{
    public class BlockStatement : IStatement
    {
        public List<IExpression> Statements { get; set; } = new List<IExpression>();
        public string Name { get; set; } = "";
        public bool IsAnonymous { get { return string.IsNullOrEmpty(Name); } }
        
        public virtual TokenCollection Tokens { get; set; } = new TokenCollection();

        public IFunctionScope Variables { get; set; } 
            = new FunctionScope(new List<VariableDeclaration>(), new List<FunctionDeclaration>(), new List<DataDeclaration>());
    }
}
