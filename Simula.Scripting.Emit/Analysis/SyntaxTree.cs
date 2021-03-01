using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Simula.Scripting.Parser;
using Simula.Scripting.Parser.Ast;

namespace Simula.Scripting.Analysis
{
    // the syntax tree is a superset of Parser.Program class which provides a fully parsed file structure.
    // it takes consideration of surrounding contexts.

    // it turns explicit define statements into declarations and generate the scopes of data-types and 
    // functions to fill the parser syntax trees. and form a number of source file programs into a large
    // syntax tree.

    // such a full, complete syntax tree is needed to build the analysis enviroment, and to provide services
    // like auto-completion and code emission.

    public class SyntaxTree
    {
        public static SyntaxTree Analyse(params Parser.Program[] program)
        {
            return new SyntaxTree(program);
        }

        public static SyntaxTree Analyse(ParserOptions options = null, params string[] source)
        {
            List<Parser.Program> programs = new List<Parser.Program>();
            Tokenizer tokenizer = new Tokenizer();
            Parser.Parser parser = new Parser.Parser();

            int index = 0;
            foreach (var item in source) {
                programs.AddIfNotNull(parser.Parse(tokenizer.Tokenize(item), options ?? ParserOptions.Default, index.ToString()));
                index++;
            }

            return Analyse(programs.ToArray());
        }

        public static SyntaxTree Analyse(ParserOptions options = null, params FileStream[] source)
        {
            List<Parser.Program> programs = new List<Parser.Program>();
            Tokenizer tokenizer = new Tokenizer();
            Parser.Parser parser = new Parser.Parser();

            foreach (var item in source) {
                using (StreamReader reader = new StreamReader(item)) {
                    programs.AddIfNotNull(parser.Parse(tokenizer.Tokenize(reader.ReadToEnd()),
                        options ?? ParserOptions.Default, item.Name));
                }
            }

            return Analyse(programs.ToArray());
        }

        private SyntaxTree(params Parser.Program[] program)
        {
            // a syntax tree contains all declarative statements in a pack of files. it ignores other
            // executive statements and the sequence of them.

            foreach (var selected in program) {
                prog?.Result.Diagnostics.AddRange(selected.Result.Diagnostics);
                foreach (var stmt in selected.Body) {
                    this.Diagnostics.File = selected.Result.File;
                    if(stmt is DeclarationBlock decl) {
                        VisitBlock(decl);
                        prog?.Body.Add(decl);
                    }
                }
            }
        }

        private void VisitBinary(BinaryExpression binary)
        {
            if (binary.Left is BlockStatement blockStmt)
                VisitBlock(blockStmt);
            else if (binary.Left is BinaryExpression binaryStmt)
                VisitBinary(binaryStmt);
            else if (binary.Left is UnaryExpression unary)
                VisitUnary(unary);
            else if (binary.Left is IndexExpression index)
                VisitIndex(index);
            else if (binary.Left is CallExpression call)
                VisitCall(call);
            else if (binary.Left is LazyExpression lazy)
                VisitLazy(lazy);

            if (binary.Right is BlockStatement block)
                VisitBlock(block);
            else if (binary.Right is BinaryExpression binaryStmt)
                VisitBinary(binaryStmt);
            else if (binary.Right is UnaryExpression unary)
                VisitUnary(unary);
            else if (binary.Right is IndexExpression index)
                VisitIndex(index);
            else if (binary.Left is CallExpression call)
                VisitCall(call);
            else if (binary.Right is LazyExpression lazy)
                VisitLazy(lazy);
        }

        private void VisitUnary(UnaryExpression expr)
        {
            if (expr.Operant is BlockStatement blockStmt)
                VisitBlock(blockStmt);
            else if (expr.Operant is BinaryExpression binaryStmt)
                VisitBinary(binaryStmt);
            else if (expr.Operant is UnaryExpression unary)
                VisitUnary(unary);
            else if (expr.Operant is IndexExpression index)
                VisitIndex(index);
            else if (expr.Operant is CallExpression call)
                VisitCall(call);
            else if (expr.Operant is LazyExpression lazy)
                VisitLazy(lazy);
        }

        private void VisitIndex(IndexExpression expr)
        {
            foreach (var item in expr.Arguments) {
                if (item is BlockStatement blockStmt)
                    VisitBlock(blockStmt);
                else if (item is BinaryExpression binary)
                    VisitBinary(binary);
                else if (item is UnaryExpression unary)
                    VisitUnary(unary);
                else if (item is IndexExpression index)
                    VisitIndex(index);
                else if (item is CallExpression call)
                    VisitCall(call);
                else if (item is LazyExpression lazy)
                    VisitLazy(lazy);
            }
        }

        private void VisitLazy(LazyExpression expr)
        {
            if (expr.Expression is BlockStatement blockStmt)
                VisitBlock(blockStmt);
            else if (expr.Expression is BinaryExpression binary)
                VisitBinary(binary);
            else if (expr.Expression is UnaryExpression unary)
                VisitUnary(unary);
            else if (expr.Expression is IndexExpression index)
                VisitIndex(index);
            else if (expr.Expression is CallExpression call)
                VisitCall(call);
            else if (expr.Expression is LazyExpression lazy)
                VisitLazy(lazy);
        }

        private void VisitCall(CallExpression expr)
        {
            foreach (var item in expr.Arguments) {
                if (item is BlockStatement blockStmt)
                    VisitBlock(blockStmt);
                else if (item is BinaryExpression binary)
                    VisitBinary(binary);
                else if (item is UnaryExpression unary)
                    VisitUnary(unary);
                else if (item is IndexExpression index)
                    VisitIndex(index);
                else if (item is CallExpression call)
                    VisitCall(call);
                else if (item is LazyExpression lazy)
                    VisitLazy(lazy);
            }
        }

        private void VisitBlock(BlockStatement block)
        {
            foreach (var item in block.Statements) {
                if (item is BlockStatement blockStmt)
                    VisitBlock(blockStmt);
                else if (item is BinaryExpression binary)
                    VisitBinary(binary);
                else if (item is UnaryExpression unary)
                    VisitUnary(unary);
                else if (item is IndexExpression index)
                    VisitIndex(index);
                else if (item is CallExpression call)
                    VisitCall(call);
                else if (item is LazyExpression lazy)
                    VisitLazy(lazy);
            }

            for (int id = 0; id < block.Statements.Count; id++) {
                var stmt = block.Statements[id];

                // detects for embedded function declaration.

                if (stmt is FunctionDeclaration function) {
                    block.Variables.FunctionDeclarations.Add(function);
                    if (block is SequenceExpression) continue;
                    block.Statements.Remove(function);

                    // detects for variable definitions, variables are defined in assignment expression, if
                    // the left-hand-side object is a literal, and has not yet been defined.

                    // there are two types of static literals that can be defined as variables. 
                    // (1) a literal, e.g.: 'a = 1'
                    // (2) a grouped tuple or matrix, e.g.:
                    //     [a, b] = [12, 23]
                    //     (energy, pos1, pos2, pos3) = function("something")

                } else if (stmt is BinaryExpression binary) {
                    if (binary.Left is Literal literal && literal.Type == LiteralType.Named) {
                        if (block.Variables.VariableDeclarations.FindAll((decl) => {
                            if (decl.Name == literal.Value) return true;
                            else return false;
                        }).Count == 0) {
                            block.Variables.VariableDeclarations.Add(new VariableDeclaration(literal)
                            {
                                Expression = binary.Right,
                                Name = literal.Value
                            });
                        }

                    } else if (binary.Left is SequenceExpression sequence) {

                        // check if all of the sequence are literals, otherwise, the sequence is invalid.
                        // this will throw an error, that a readonly variable cannot be assigned with values.

                        bool flag = true;
                        foreach (var literals in sequence.Statements) {
                            if (!(literals is Literal literal_ && literal_.Type == LiteralType.Named)) {
                                flag = false;
                                this.Diagnostics.AddFatal(SyntaxError.AssignToReadonlyValues, literals.Tokens[0]);
                            }
                        }

                        if (flag) {
                            foreach (Literal literals in sequence.Statements) {
                                if (block.Variables.VariableDeclarations.FindAll((decl) => {
                                    if (decl.Name == literals.Value) return true;
                                    else return false;
                                }).Count == 0) {
                                    block.Variables.VariableDeclarations.Add(new VariableDeclaration(literals)
                                    {
                                        Expression = binary.Right,
                                        Name = literals.Value
                                    });
                                }
                            }
                        }
                    }

                } else if (stmt is DataDeclaration data) {
                    block.Variables.DataDeclarations.Add(data);
                    if (block is SequenceExpression) continue;
                    block.Statements.Remove(data);
                }
            }
        }

        private Parser.Program prog = new Parser.Program();
        public List<IExpression> Statements => prog?.Body ?? new List<IExpression>();
        public ParserResult Diagnostics => prog?.Result ?? new ParserResult();
    }
}
