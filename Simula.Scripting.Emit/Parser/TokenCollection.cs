using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Parser
{
    public class TokenCollection : List<Token>
    {
        // the methods Split, Contains has a corresponding *Cascade version. this means the method
        // will check whether the specified symbol is in identical level of blocks as the baseline.
        // taking nested blocks into consideration.
        
        // for example, the following token collection:
        //     data a ( int : c ) : b

        // applying Split(":"), the sections are:
        // [1] data a ( int
        // [2] c ) 
        // [3] b

        // applying SplitCascade(":"), the sections are:
        // [1] data a ( int : c ) 
        // [2] b

        public List<TokenCollection> Split(Token token)
        {
            List<TokenCollection> result = new List<TokenCollection>();
            TokenCollection collection = new TokenCollection();

            foreach (var item in this) {
                if (item.ContentEquals(token)) {
                    result.Add(collection);
                    collection = new TokenCollection();
                } else {
                    collection.Add(item);
                }
            }

            if (collection.Count > 0) result.Add(collection);
            return result;
        }

        public List<TokenCollection> SplitCascade(Token token)
        {
            return SplitCascade(token.Value);
        }

        public List<TokenCollection> SplitCascade(string token)
        {
            List<TokenCollection> result = new List<TokenCollection>();
            TokenCollection collection = new TokenCollection();

            int blockLevel = 0;
            foreach (var item in this) {
                if (item.Value == token && blockLevel == 0) {
                    result.Add(collection);
                    collection = new TokenCollection();
                    continue;
                }

                switch (item.Value) {
                    case "iter":
                    case "while":
                    case "conditional":
                    case "data":
                    case "func":
                    case "config":
                    case "if":
                    case "match":
                    case "try":
                    case "(":
                    case "[":
                    case "{": blockLevel++; break;

                    case ")":
                    case "]":
                    case "}":
                    case "end": blockLevel--; break;
                    default: break;
                }

                collection.Add(item);
            }

            if (collection.Count > 0) result.Add(collection);
            return result;
        }

        public new bool Contains(Token token)
        {
            return Contains(token.Value);
        }

        public bool Contains(string token)
        {
            bool flag = false;
            foreach (var item in this)
                if (item.Value == token) {
                    flag = true; break;
                }

            return flag;
        }

        public bool ContainsCascade(Token token)
        {
            return ContainsCascade(token.Value);
        }

        public bool ContainsCascade(string token)
        {
            bool flag = false;
            int blockLevel = 0;
            foreach (var item in this) {
                if (item.Value == token && blockLevel == 0) {
                    flag = true; break;
                }

                switch (item.Value) {
                    case "iter":
                    case "while":
                    case "conditional":
                    case "data":
                    case "func":
                    case "config":
                    case "if":
                    case "match":
                    case "try":
                    case "(":
                    case "[":
                    case "{": blockLevel++; break;

                    case ")":
                    case "]":
                    case "}":
                    case "end": blockLevel--; break;
                    default: break;
                }
            }

            return flag;
        }

        public Token Last()
        {
            return this[Count - 1];
        }

        public void RemoveLast()
        {
            RemoveAt(Count - 1);
        }

        public string ToString(TokenFormatterOption option = TokenFormatterOption.Default)
        {
            switch (option) {
                case TokenFormatterOption.Default:
                    return "[Token Collection]";
                case TokenFormatterOption.SpaceSeparatedList:
                    return this.JoinString(" ");
                case TokenFormatterOption.CommaSeparatedList:
                    return this.JoinString(", ");
                case TokenFormatterOption.Table:
                    string tableHeader = "No\tLs\tCs\tLt\tCt\tIdentifer\n" +
                                         "---\t---\t---\t---\t---\t------------\n";
                    List<string> content = new List<string>();
                    int id = 0;
                    foreach (var item in this) {
                        content.Add(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", id++,
                            item.Location.Start.Line, item.Location.Start.Column,
                            item.Location.End.Line, item.Location.End.Column, item.Value));
                    }

                    return tableHeader + content.JoinString("\n");
            }

            return "[Token Collection]";
        }
    }

    public enum TokenFormatterOption
    {
        Default,
        SpaceSeparatedList,
        CommaSeparatedList,
        Table
    }
}

namespace Simula
{
    public static partial class Extension
    {
        public static Scripting.Parser.TokenCollection JoinTokens(this List<Scripting.Parser.TokenCollection> tokens)
        {
            Scripting.Parser.TokenCollection collections = new Scripting.Parser.TokenCollection();
            foreach (var item in tokens) {
                collections.AddRange(item);
                collections.Add(Scripting.Parser.Token.LineBreak);
            }
            return collections;
        }
    }
}