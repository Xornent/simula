using System;
using System.Collections.Generic;
using Simula.Scripting.Syntax;
using System.Text;

namespace Simula.Scripting.Contexts
{
    public class CompletionCaret
    {
        public Stack<Statement> Cascade = new Stack<Statement>();
        public CompletionCaret(int line, int column, Syntax.BlockStatement block)
        {
            Locate(line, column, block);
            if (Cascade.Count > 0) {
                var b = Cascade.Peek();
                var list = b.RawToken;
                Token.TokenCollection collection = new Token.TokenCollection();
                foreach (var item in list) {
                    if (item.Location.Start.Line < line) {
                        if (item.Value == Token.Token.LineBreak.Value) collection.Clear();
                        collection.Add(item);
                    } else if (item.Location.Start.Line == line && item.Location.Start.Column + 1 <= column) {
                        if (item.Value == Token.Token.LineBreak.Value) collection.Clear();
                        collection.Add(item);
                    } else break;
                }

                b.RawToken = collection;
            }
        }

        public void Locate(int line, int column, Statement block)
        {
            if (block.RawToken.Count > 0) {
                var first = block.RawToken[0];
                var last = block.RawToken.Last();

                bool flag = false;
                if (((first.Location.Start.Line < line) ||
                    (first.Location.Start.Line == line && first.Location.Start.Column + 1 <= column)) &&
                    ((last.Location.End.Line > line) ||
                    (last.Location.End.Line == line && last.Location.End.Column + 1 >= column - 1))) {

                    Cascade.Push(block);
                    if (block is BlockStatement b) {
                        foreach (var item in b.Children) {
                            Locate(line, column, item);
                        }
                    }
                }
            }
        }
    }
}
