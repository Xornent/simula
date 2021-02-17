using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Simula.Scripting.Parser
{
    public enum TokenType
    {
        Undefined,
        IntegerLiteral,
        FloatingLiteral,
        Identifer,
        StringLiteral,
        Punctuator,
        Comment,
        Whitespace,
        Newline
    }

    public class Token
    {
        public Token(string value)
        {
            this.Value = value;
            this.Location = new Span(0,0,0,0);
            this.Errors = new List<LexicalError>();
        }

        public Token(string value, Span location)
        {
            this.Value = value;
            this.Location = location;
            this.Errors = new List<LexicalError>();
        }

        public Token(string value, Span location, TokenType type)
        {
            this.Value = value;
            this.Location = location;
            this.Errors = new List<LexicalError>();
            this.Type = type;
        }

        public Token(string value, Span location, List<LexicalError> errors)
        {
            this.Value = value;
            this.Location = location;
            this.Errors = errors;
        }

        public Token(string value, Span location, List<LexicalError> errors, TokenType type)
        {
            this.Value = value;
            this.Location = location;
            this.Errors = errors;
            this.Type = type;
        }

        public string Value { get; set; }
        public Span Location { get; set; }
        public TokenType Type { get; set; }
        public bool HasError { get { return Errors.Count > 0; } }
        public List<LexicalError> Errors { get; set; }

        public bool ContentEquals(Token t)
        {
            return Value == t.Value;
        }

        public bool IsValidNumberBeginning()
        {
            string lower = Value.ToLower();
            Regex reg = new Regex(@"^[0-9]+\.?[0-9]*$");
            return reg.IsMatch(lower);
        }

        public bool IsValidNameBeginning()
        {
            string lower = Value.ToLower();
            Regex reg = new Regex("^[a-z_]+[a-z0-9_]*$");
            return reg.IsMatch(lower);
        }

        public bool IsValidSymbolBeginning()
        {
            string lower = Value.ToLower();
            bool flag = true;
            foreach (var item in Value)
                if (!item.IsSymbol()) flag = false;
            return flag;
        }

        public static implicit operator string(Token t)
        {
            return t.Value;
        }

        public static explicit operator Token(string t)
        {
            return new Token(t);
        }

        public static Token LineBreak = new Token("<newline>", new Span(0,0,0,0));
        public static Token LineContinue = new Token("...", new Span(0,0,0,0));

        public override string ToString()
        {
            return Value;
        }
    }
}

namespace Simula
{
    public static partial class Extension
    {
        public static bool IsAlphabet(this char c)
        {
            string lower = new string(new char[1] { c }).ToLower();
            Regex reg = new Regex("[a-z_]");
            return reg.IsMatch(lower);
        }

        public static bool IsDigit(this char c)
        {
            string lower = new string(new char[1] { c }).ToLower();
            Regex reg = new Regex("[0-9]");
            return reg.IsMatch(lower);
        }

        public static bool IsSymbol(this char item)
        {
            if (item != '.' &&
                item != '[' &&
                item != ']' &&
                item != '{' &&
                item != '}' &&
                item != '<' &&
                item != '>' &&
                item != '=' &&
                item != '(' &&
                item != ')' &&
                item != '*' &&
                item != '&' &&
                item != '%' &&
                item != '!' &&
                item != ':' &&
                item != ';' &&
                item != ',' &&
                item != '+' &&
                item != '-' &&
                item != '/' &&
                item != '|' &&
                item != '^' &&
                item != '?' &&
                item != '~' &&
                item != '\'' &&
                item != '\"' &&
                item != '\\' &&
                item != '#') return false;
            return true;
        }

        public static string JoinString<T>(this List<T> list, string connector)
        {
            if (list.Count == 0) return "";
            else {
                string s = list[0]?.ToString() ?? "null";
                for (int i = 1; i < list.Count; i++) {
                    s += (connector + (list[i]?.ToString() ?? "null"));
                }
                return s;
            }
        }
    }
}
