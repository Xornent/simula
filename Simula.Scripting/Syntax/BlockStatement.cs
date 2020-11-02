using System;
using System.Collections.Generic;
using System.Text;
using Simula.Scripting.Compilation;
using Simula.Scripting.Debugging;
using Simula.Scripting.Token;

namespace Simula.Scripting.Syntax {

    public class BlockStatement : Statement {
        public List<Statement> Children = new List<Statement>();

        public override void Parse(Token.TokenCollection collection) {
            List<TokenCollection> lines = collection.Split(Token.Token.LineBreak);
            Stack<BlockStatement> currentBlock = new Stack<BlockStatement>();
            currentBlock.Push(this);

            foreach (var line in lines) {
                if(line.Count>0) {
                    if (((string)line[0]).StartsWith("'")) {
                        if (currentBlock.Peek() is CommentBlock) {
                            (currentBlock.Peek() as CommentBlock)?.Lines.Add((string)line[0]);
                        } else {
                            CommentBlock c = new CommentBlock();
                            c.Lines.Add((string)line[0]);
                            currentBlock.Push(c);
                        }
                    } else {
                        if(currentBlock.Peek() is CommentBlock) {
                            var c = currentBlock.Pop();
                            currentBlock.Peek().Children.Add(c);
                        }

                        if (line[0] == "use") {
                            UseStatement use = new UseStatement();
                            use.Parse(line);
                            currentBlock.Peek().Children.Add(use);
                        } else if (line[0] == "module") {
                            ModuleStatement module = new ModuleStatement();
                            module.Parse(line);
                            currentBlock.Peek().Children.Add(module);
                        } else if (line[0] == "expose" ||
                                   line[0] == "hidden" ||
                                   line[0] == "def") {
                            DefinitionBlock def = new DefinitionBlock();

                            if (!line.Contains(new Token.Token("=")))
                                currentBlock.Push(def);
                            else currentBlock.Peek().Children.Add(def);

                            def.Parse(line);
                        } else if (line[0] == "if") {
                            IfBlock i = new IfBlock();
                            currentBlock.Push(i);

                            i.Parse(line);
                        } else if (line[0] == "eif") {
                            if (currentBlock.Peek() is IfBlock || currentBlock.Peek() is ElseIfBlock) {
                                var b = currentBlock.Pop();
                                currentBlock.Peek().Children.Add(b);
                            } else line[0].Error = new TokenizerException("SS0007");

                            ElseIfBlock block = new ElseIfBlock();
                            block.Parse(line);
                            currentBlock.Push(block);
                        } else if (line[0] == "else") {
                            if (currentBlock.Peek() is IfBlock || currentBlock.Peek() is ElseIfBlock) {
                                var b = currentBlock.Pop();
                                currentBlock.Peek().Children.Add(b);
                            } else line[0].Error = new TokenizerException("SS0008");

                            ElseBlock block = new ElseBlock();
                            block.Parse(line);
                            currentBlock.Push(block);
                        } else if (line[0] == "while") {
                            WhileBlock w = new WhileBlock();
                            currentBlock.Push(w);

                            w.Parse(line);
                        } else if (line[0] == "enum") {
                            EnumerableBlock e = new EnumerableBlock();
                            currentBlock.Push(e);

                            e.Parse(line);
                        } else if (line[0] == "pass") {
                            PassStatement l = new PassStatement();
                            l.Parse(line);
                            currentBlock.Peek().Children.Add(l);
                        } else if (line[0] == "break") {
                            BreakStatement l = new BreakStatement();
                            l.Parse(line);
                            currentBlock.Peek().Children.Add(l);
                        } else if (line[0] == "return") {
                            ReturnStatement l = new ReturnStatement();
                            l.Parse(line);
                            currentBlock.Peek().Children.Add(l);
                        } else if (line[0] == "end") {
                            var block = currentBlock.Pop();
                            currentBlock.Peek().Children.Add(block);
                        } else {

                            // 此处解析语句

                            // 一个语句可以选择是否赋值, 赋值操作的语句中含有且仅含有一个 '=' 符号
                            // 而此前的部分为一个可写的内容, 此后为可读（任意）的内容

                            if (line.Contains(new Token.Token("="))) {
                                AssignStatement a = new AssignStatement();
                                a.Parse(line);
                                currentBlock.Peek().Children.Add(a);
                            } else {
                                EvaluationStatement eval = new EvaluationStatement();
                                eval.Parse(line);
                                currentBlock.Peek().Children.Add(eval);
                            }
                        }
                    }
                }
            }
        }

        public override ExecutionResult Execute(Compilation.RuntimeContext ctx) {
            bool skipif = false;
            foreach (var item in this.Children) {
                if (item is DefinitionBlock) { }
                else {
                    if (item is ElseBlock || item is ElseIfBlock) {
                        if (skipif) { continue; }
                    } else {
                        if (skipif) skipif = false;
                    }

                    var result = item.Execute(ctx);

                    if (item is IfBlock || item is ElseIfBlock || item is ElseBlock) {
                        if (result.Flag == ExecutableFlag.Else) {
                            skipif = false;
                        } else {
                            skipif = true;
                        }
                    }

                    switch (result.Flag) {
                        case Debugging.ExecutableFlag.Pass:
                            break;
                        case Debugging.ExecutableFlag.Return:
                            return new ExecutionResult(result.Pointer, ctx, ExecutableFlag.Return);
                        case Debugging.ExecutableFlag.Break:
                            return new ExecutionResult(result.Pointer, ctx, ExecutableFlag.Break);
                        case Debugging.ExecutableFlag.Continue:
                            return new ExecutionResult(result.Pointer, ctx, ExecutableFlag.Continue);
                        default:
                            break;
                    }
                }
            }

            return new ExecutionResult(0, ctx, ExecutableFlag.Pass);
        }
    }
}
