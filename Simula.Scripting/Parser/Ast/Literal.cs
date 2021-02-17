using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Simula.Scripting.Parser.Ast
{
    public class Literal : IExpression
    {
        public Literal(string literal)
        {
            Regex integral = new Regex("^[0-9]+$");
            Regex floating = new Regex("^[0-9]+[.]?[0-9]+$");
            Regex str = new Regex("^[\"].*[\"]$");
            Regex named = new Regex("^[a-z_]+[a-z0-9_]*$");

            this.Value = literal;
            if (integral.IsMatch(literal)) this.Type = LiteralType.Integer;
            else if (floating.IsMatch(literal)) this.Type = LiteralType.Float;
            else if (str.IsMatch(literal)) this.Type = LiteralType.String;
            else if (named.IsMatch(literal)) this.Type = LiteralType.Named;
            else this.Type = LiteralType.Illegal;
        }

        public TokenCollection Tokens { get; set; } = new TokenCollection();
        public string Value { get; set; }
        public LiteralType Type { get; set; }
    }

    public enum LiteralType
    {
        Float,
        Integer,
        String,
        Named,

        Illegal
    }
}
