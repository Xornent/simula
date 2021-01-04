using Simula.Scripting.Contexts;
using Simula.Scripting.Token;
using Simula.Scripting.Build;
using System.Collections.Generic;

namespace Simula.Scripting.Syntax
{

    public class BlockStatement : Statement
    {
        public List<Statement> Children = new List<Statement>();

        public override void Parse(Token.TokenCollection collection)
        {
            List<TokenCollection> lines = collection.Split(Token.Token.LineBreak);
            Stack<BlockStatement> currentBlock = new Stack<BlockStatement>();
            currentBlock.Push(this);

            IfBlock? temperaryIf = null;

            foreach (var line in lines) {
                if (line.Count > 0) {
                    if (((string)line[0]).StartsWith("'")) {
                        if (currentBlock.Peek() is CommentBlock) {
                            (currentBlock.Peek() as CommentBlock)?.Lines.Add(line[0]);
                        } else {
                            CommentBlock c = new CommentBlock();
                            c.Lines.Add(line[0]);
                            currentBlock.Push(c);
                        }
                    } else {
                        if (currentBlock.Peek() is CommentBlock) {
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

                            if (!line.Contains(new Token.Token("var")))
                                currentBlock.Push(def);
                            else currentBlock.Peek().Children.Add(def);

                            def.Parse(line);
                        } else if (line[0] == "var" ||
                                   line[0] == "class" ||
                                   line[0] == "func" ||
                                   line[0] == "prop") {
                            DefinitionBlock def = new DefinitionBlock();

                            if (!line.Contains(new Token.Token("var")))
                                currentBlock.Push(def);
                            else currentBlock.Peek().Children.Add(def);

                            line.Insert(0, new Token.Token("def"));
                            def.Parse(line);
                        } else if (line[0] == "if") {
                            IfBlock i = new IfBlock();
                            currentBlock.Push(i);

                            i.Parse(line);
                            temperaryIf = i;
                        } else if (line[0] == "eif") {
                            if (currentBlock.Peek() is IfBlock ||
                                currentBlock.Peek() is ElseIfBlock) {
                                var b = currentBlock.Pop();
                                currentBlock.Peek().Children.Add(b);

                                ElseIfBlock block = new ElseIfBlock();
                                block.Parse(line);
                                currentBlock.Push(block);
                                temperaryIf?.ElseifBlocks.Add(block);
                            } else line[0].Error = new TokenizerException("SS0007");

                        } else if (line[0] == "else") {
                            if (currentBlock.Peek() is IfBlock ||
                                currentBlock.Peek() is ElseIfBlock) {
                                var b = currentBlock.Pop();
                                currentBlock.Peek().Children.Add(b);

                                ElseBlock block = new ElseBlock();
                                block.Parse(line);
                                currentBlock.Push(block);
                                if (temperaryIf != null)
                                    temperaryIf.ElseBlock = block;
                                temperaryIf = null;
                            } else line[0].Error = new TokenizerException("SS0008");

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

                            EvaluationStatement eval = new EvaluationStatement();
                            eval.Parse(line);
                            currentBlock.Peek().Children.Add(eval);
                        }
                    }
                }
            }
        }

        public override Execution Execute(DynamicRuntime ctx)
        {
            Execution result = new Execution();
            foreach (var item in Children) {
                if (item is DefinitionBlock) { } else {
                    
                    if(item is ElseIfBlock || item is ElseBlock) {
                        if(result.Flag == ExecutionFlag.Else) {
                            result = item.Execute(ctx);
                        }
                    } else {
                        result = item.Execute(ctx);
                    }

                    switch (result.Flag) {
                        case ExecutionFlag.Pass: continue;
                        case ExecutionFlag.Return:
                            return new Execution(ctx, result.Result, ExecutionFlag.Return);
                        case ExecutionFlag.Break:
                            return new Execution(ctx, result.Result, ExecutionFlag.Break);
                        case ExecutionFlag.Else:
                            continue;
                        case ExecutionFlag.Continue:
                            return new Execution(ctx, result.Result, ExecutionFlag.Continue);
                        case ExecutionFlag.Go:
                            continue;
                        default:
                            break;
                    }
                }
            }

            return new Execution();
        }

        public override string Generate(GenerationContext ctx)
        {
            string code = ctx.Indention() + "{\n";
            ctx.IndentionLevel++;
            foreach (var item in this.Children) {
                code += (item.Generate(ctx) + "\n");
            }
            ctx.IndentionLevel--;
            code += ctx.Indention() + "}";

            return code;
        }
    }
}
