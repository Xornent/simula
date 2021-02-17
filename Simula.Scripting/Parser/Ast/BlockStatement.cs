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

        public IDataScope Variables { get; set; } 
            = new DataScope(new List<VariableDeclaration>(), new List<FunctionDeclaration>());
    }
}
