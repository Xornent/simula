using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Parser.Ast
{
    public class ConditionalStatement : BlockStatement
    {
        public ConditionalStatement(IExpression type) 
        {
            this.ConditionalType = type;
        }

        public IExpression ConditionalType { get; set; }
    }
}
