using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Parser
{
    public class TokenCollection : List<Token>
    {
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

        public new bool Contains(Token token)
        {
            bool flag = false;
            foreach (var item in this)
                if (item.ContentEquals(token)) {
                    flag = true; break;
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
