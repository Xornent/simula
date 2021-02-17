using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Parser
{
    public class Tokenizer
    {
        public Tokenizer()
        {
            this.Ruleset = new List<Func<TokenCollection, char, int, int, LexicalError>>()
            {
                this.TokenizeDigit,
                this.TokenizePunctuator,
                this.TokenizeAlphabet,
                this.TokenizeWhiteSpace
            };
        }

        public TokenCollection Tokenize(string source)
        {
            int lineCount = 1;
            int columnCount = 1;
            TokenCollection collection = new TokenCollection();
            string[] lines = source.Split('\n');

            foreach (string line in lines) {
                foreach (char character in line) {
                    this.Tokenize(collection, character, lineCount, columnCount);
                    columnCount++;
                }

                if (collection.Count == 0) {
                    collection.Add(new Token(Token.LineBreak.Value, new Span(lineCount, columnCount, lineCount, columnCount + 1), TokenType.Newline));
                } else if (collection.Last().ContentEquals(Token.LineContinue)) {
                    collection.RemoveLast();
                } else collection.Add(new Token(Token.LineBreak.Value, new Span(lineCount, columnCount, lineCount, columnCount + 1), TokenType.Newline));
                
                columnCount = 1;
                lineCount++;
            }

            collection.RemoveAll((token) => { return token.Type == TokenType.Whitespace; });
            return collection;
        }

        public LexicalError Tokenize(TokenCollection collection, char character, int line, int column)
        {
            LexicalError error = LexicalError.RuleNotMatch;
            int ruleSetCount = 0;
            while(error == LexicalError.RuleNotMatch) {
                error = this.Ruleset[ruleSetCount](collection, character, line, column);
                ruleSetCount++;
                if (ruleSetCount >= this.Ruleset.Count) break;
            }

            return error;
        }

        public List<Func<TokenCollection, char, int, int, LexicalError>> Ruleset = 
            new List<Func<TokenCollection, char, int, int, LexicalError>>();

        public LexicalError TokenizeDigit(TokenCollection collection, char character, int line, int column)
        {
            if (!character.IsDigit() && character != '.') return LexicalError.RuleNotMatch;

            if (collection.Count == 0) {
                if (character.IsDigit()) {
                    collection.Add(new Token(character.ToString(), new Span(line, column, line, column), TokenType.IntegerLiteral));
                    return LexicalError.Ok;
                } else return LexicalError.RuleNotMatch;
            }

            var previous = collection.Last();
            switch (previous.Type) {
                case TokenType.IntegerLiteral:
                    if (character.IsDigit()) {
                        previous.Value += character;
                        previous.Location.End = new Position(line, column);
                    } else if (character == '.') {
                        previous.Value += character;
                        previous.Type = TokenType.FloatingLiteral;
                        previous.Location.End = new Position(line, column);
                    } else return LexicalError.RuleNotMatch;
                    return LexicalError.Ok;
                case TokenType.FloatingLiteral:
                    if (character.IsDigit()) {
                        previous.Value += character;
                        previous.Location.End = new Position(line, column);
                    } else if (character == '.') return LexicalError.UnexpectedNumeral;
                    else return LexicalError.RuleNotMatch;
                    return LexicalError.Ok;

                case TokenType.Comment:
                case TokenType.StringLiteral:
                    previous.Value += character;
                    previous.Location.End = new Position(line, column);
                    return LexicalError.Ok;

                case TokenType.Identifer:
                    if (character == '.') return LexicalError.RuleNotMatch;
                    previous.Value += character;
                    previous.Location.End = new Position(line, column);
                    return LexicalError.Ok;

                default:
                    if (character.IsDigit()) {
                        collection.Add(new Token(character.ToString(), new Span(line, column, line, column), TokenType.IntegerLiteral));
                        return LexicalError.Ok;
                    } else return LexicalError.RuleNotMatch;
            }
        }

        public LexicalError TokenizeWhiteSpace(TokenCollection collection, char character, int line, int column)
        {
            if (character != ' ' && character != '\t') return LexicalError.RuleNotMatch;

            if (collection.Count == 0) {
                collection.Add(new Token(" ", new Span(line, column, line, column), TokenType.Whitespace));
                return LexicalError.Ok;
            }

            var previous = collection.Last();
            switch (previous.Type) {
                case TokenType.Comment:
                case TokenType.StringLiteral:
                    previous.Value += character;
                    previous.Location.End = new Position(line, column);
                    return LexicalError.Ok;

                default:
                    collection.Add(new Token(" ", new Span(line, column, line, column), TokenType.Whitespace));
                    return LexicalError.Ok;
            }
        }

        public LexicalError TokenizePunctuator(TokenCollection collection, char character, int line, int column)
        {
            if (!character.IsSymbol()) return LexicalError.RuleNotMatch;

            if (collection.Count == 0 ||
                (character == '(') ||
                (character == ')') ||
                (character == '[') ||
                (character == ']') ||
                (character == '{') ||
                (character == '}')  ) {
                collection.Add(new Token(character.ToString(), new Span(line, column, line, column), TokenType.Punctuator));
                return LexicalError.Ok;
            }

            var previous = collection.Last();
            switch (previous.Type) {
                case TokenType.Comment:
                    previous.Value += character;
                    previous.Location.End = new Position(line, column);
                    return LexicalError.Ok;
                case TokenType.Punctuator:
                    if ((previous.Value == "(") ||
                        (previous.Value == ")") ||
                        (previous.Value == "[") ||
                        (previous.Value == "]") ||
                        (previous.Value == "{") ||
                        (previous.Value == "}")) {
                        collection.Add(new Token(character.ToString(), new Span(line, column, line, column), TokenType.Punctuator));
                        return LexicalError.Ok;
                    } else {
                        previous.Value += character;
                        previous.Location.End = new Position(line, column);
                        return LexicalError.Ok;
                    }
                case TokenType.StringLiteral:
                    if(character == '\"') {
                        if (previous.Value.EndsWith("\\")) { }
                        else {
                            previous.Location.End = new Position(line, column);
                            collection.Add(new Token(" ", new Span(line, column, line, column), TokenType.Whitespace));
                            return LexicalError.Ok;
                        }
                    }
                    previous.Value += character;
                    previous.Location.End = new Position(line, column);
                    return LexicalError.Ok;

                default:
                    if(character == '\"') {
                        collection.Add(new Token("", new Span(line, column, line, column), TokenType.StringLiteral));
                        return LexicalError.Ok;
                    } else if(character == '\'') {
                        collection.Add(new Token("", new Span(line, column, line, column), TokenType.Comment));
                        return LexicalError.Ok;
                    }

                    collection.Add(new Token(character.ToString(), new Span(line, column, line, column), TokenType.Punctuator));
                    return LexicalError.Ok;
            }
        }

        public LexicalError TokenizeAlphabet(TokenCollection collection, char character, int line, int column)
        {
            if (!character.IsAlphabet()) return LexicalError.RuleNotMatch;

            if (collection.Count == 0) {
                if (character.IsAlphabet()) {
                    collection.Add(new Token(character.ToString(), new Span(line, column, line, column), TokenType.Identifer));
                    return LexicalError.Ok;
                } else return LexicalError.RuleNotMatch;
            }

            var previous = collection.Last();
            switch (previous.Type) {
                case TokenType.Identifer:
                    previous.Value += character;
                    previous.Location.End = new Position(line, column);
                    return LexicalError.Ok;

                case TokenType.Comment:
                case TokenType.StringLiteral:
                    previous.Value += character;
                    previous.Location.End = new Position(line, column);
                    return LexicalError.Ok;

                default:
                    if (character.IsAlphabet()) {
                        collection.Add(new Token(character.ToString(), new Span(line, column, line, column), TokenType.Identifer));
                        return LexicalError.Ok;
                    } else return LexicalError.RuleNotMatch;
            }
        }
    }
}
