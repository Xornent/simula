using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Parser.Ast
{
    public class ErrorStatement : IStatement
    {
        public TokenCollection Tokens { get; set; } = new TokenCollection();
        public int ErrorId { get; set; } = 0;

        private string _errorText = "";
        public string ErrorText { get { return ErrorId == 0 
            ? _errorText : Resources.Loc((StringTableIndex) ErrorId); } set { _errorText = value; } }
    }
}
