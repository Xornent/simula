using System;
using System.Collections.Generic;

namespace Simula.Scripting.Parser 
{
    public class Program 
    {
        public List<Ast.IExpression> Body = new List<Ast.IExpression>();
        public ParserResult Result = new ParserResult();
    }
}