using System.Collections.Generic;

namespace Simula.Scripting.Token
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

        public bool Contains(Token token)
        {
            bool flag = false;
            foreach (var item in this) {
                if (item.ContentEquals(token)) {
                    flag = true; break;
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

        public override string ToString()
        {
            string s = "";
            foreach (var item in this) {
                s = s + item.Value;
            }
            return s;
        }
    }

    public class TokenDocument
    {
        public TokenCollection Tokens { get; set; } = new TokenCollection();

        public void Tokenize(string source)
        {
            Tokens.Clear();

            int insideBracket = 0;
            int linenum = 0;
            int columnnum = 0;
            string[] lines = source.Split('\n');
            foreach (var line in lines) {
                linenum++;
                columnnum = 0;

                Token token = new Token("");
                Position start = new Position(linenum, 1);
                Position end = new Position(linenum, 1);

                foreach (var column in line) {
                    columnnum++;

                    if (token.Value.StartsWith("\"")) {
                        if (token.Value[token.Value.Length - 1] != '\\') {
                            if (column == '\"') {
                                token.Value += '\"';
                                end = new Position(linenum, columnnum);
                                token.Location = new Span(start, end);
                                Tokens.Add(token);
                                token = new Token("");
                                start = new Position(linenum, columnnum + 1);
                            } else {
                                token.Value += column;
                            }
                        } else {
                            token.Value += column;
                        }
                    } else if (token.Value.StartsWith("'")) {
                        token.Value += column;
                    } else {

                        if (column == '(' ||
                            column == ')' ||
                            column == '[' ||
                            column == ']' ||
                            column == '{' ||
                            column == '}') {
                            end = new Position(linenum, columnnum - 1);
                            token.Location = new Span(start, end);
                            if(token!="") Tokens.Add(token);
                            Tokens.Add(new Token(new string(new char[] { column })) { 
                                Location = new Span(new Position(linenum, columnnum), new Position(linenum, columnnum))
                            });

                            if (column == '{' || column == '[' || column == '(') insideBracket++;
                            else if (column == '}' || column == ']' || column == ')') insideBracket--;

                            token = new Token("");
                            start = new Position(linenum, columnnum + 1);
                            continue;
                        } else if (token == ";" && insideBracket == 0) {
                            end = new Position(linenum, columnnum - 1);
                            token.Location = new Span(start, end);
                            token = new Token("");
                            start = new Position(linenum, columnnum + 1);
                            Tokens.Add(new Token("<newline>", new Span(
                                new Position(linenum, columnnum), new Position(linenum, columnnum))));
                            continue;
                        }

                        if (column == ' ') {
                            if (token != "") {
                                end = new Position(linenum, columnnum - 1);
                                token.Location = new Span(start, end);
                                Tokens.Add(token);
                                token = new Token("");
                                start = new Position(linenum, columnnum + 1);
                            } else {
                                start = new Position(linenum, columnnum + 1);
                            }
                        } else {
                            if (token != "") {
                                if (token.IsValidNameBeginning()) {
                                    if (column.IsAlphabet())
                                        token.Value += column;
                                    else {
                                        end = new Position(linenum, columnnum - 1);
                                        token.Location = new Span(start, end);
                                        Tokens.Add(token);
                                        token = new Token(new string(new char[1] { column }));
                                        start = new Position(linenum, columnnum);
                                    }
                                } else if (token.IsValidNumberBeginning()) {
                                    if (column.IsAlphabet() || column == '\"' || (new Token(new string(new char[] { column })).IsValidSymbolBeginning() && column != '.')) {
                                        end = new Position(linenum, columnnum - 1);
                                        token.Location = new Span(start, end);
                                        Tokens.Add(token);
                                        token = new Token(new string(new char[1] { column }));
                                        start = new Position(linenum, columnnum);
                                    } else {
                                        token.Value += column;
                                    }
                                } else if (token.IsValidSymbolBeginning() || column == '\"') {
                                    if (new Token(new string(new char[1] { column })).IsValidSymbolBeginning()) {
                                        token.Value += column;
                                    } else {
                                        end = new Position(linenum, columnnum - 1);
                                        token.Location = new Span(start, end);
                                        Tokens.Add(token);
                                        token = new Token(new string(new char[1] { column }));
                                        start = new Position(linenum, columnnum);
                                    }
                                } else {
                                    token.Value += column;
                                }
                            } else token.Value += column;
                        }
                    }
                }

                if (Tokens.Count > 0) {
                    if (Tokens.Last().ContentEquals(Token.Continue)) {
                        Tokens.RemoveLast();
                    } else {
                        if (token != "") {
                            end = new Position(linenum, columnnum - 1);
                            token.Location = new Span(start, end);
                            Tokens.Add(token);
                        }
                        token = new Token("");
                        start = new Position(linenum + 1, 1);
                        if (insideBracket == 0)
                            Tokens.Add(new Token("<newline>", new Span(
                                new Position(linenum, columnnum), new Position(linenum, columnnum))));
                        else Tokens.Add(new Token(";", new Span(
                           new Position(linenum, columnnum), new Position(linenum, columnnum))));
                    }
                } else {
                    if (token != "") {
                        if (token.ContentEquals(Token.Continue)) { continue; } else {
                            end = new Position(linenum, columnnum - 1);
                            token.Location = new Span(start, end);
                            Tokens.Add(token);
                            token = new Token("");
                            start = new Position(linenum + 1, 1);
                        }
                    }

                    if (insideBracket == 0)
                        Tokens.Add(new Token("<newline>", new Span(
                           new Position(linenum, columnnum), new Position(linenum, columnnum))));
                    else Tokens.Add(new Token(";", new Span(
                           new Position(linenum, columnnum), new Position(linenum, columnnum))));
                }
            }

            for (int i = 0; i < Tokens.Count; i++) {
                if (Tokens[i] == "" ||
                    Tokens[i] == "\n" ||
                    Tokens[i] == "\r") {
                    Tokens.RemoveAt(i);
                    i--;
                }
            }
        }
    }
}
