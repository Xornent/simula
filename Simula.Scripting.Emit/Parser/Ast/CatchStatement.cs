using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Parser.Ast
{
    public class CatchStatement : BlockStatement
    {
        public CatchStatement(TryStatement trystmt, Literal exceptionName) 
        {
            this.Try = trystmt;
            this.ExceptionName = exceptionName;
        }

        public TryStatement Try { get; set; }
        public Literal ExceptionName { get; set; }
    }
}
