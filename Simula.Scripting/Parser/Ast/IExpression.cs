using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Parser.Ast
{
    public interface IExpression
    {
        public TokenCollection Tokens { get; set; }
    }
}
