﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Simula.Scripting.Parser.Ast;

namespace Simula.Scripting.Parser
{
    public class Parser
    {
        public Program Parse(TokenCollection collection, ParserOptions options = null, string file = "") 
        {
            Program program = new Program();
            List<Ast.IExpression> statements = new List<Ast.IExpression>();
            TokenCollection tokens = new TokenCollection();
            foreach (var item in collection) {
                if (item.Type != TokenType.Comment) tokens.Add(item);
            }

            ParserResult result = Parse(statements, tokens, new ParserState(), options ?? ParserOptions.Default, file);
            program.Body = statements;
            program.Result = result;
            if(!result.Successful) return program;
            else return program;
        }

        public ParserResult Parse(List<IExpression> statements, TokenCollection tokens, ParserState state, ParserOptions options, string file = "") 
        {
            ParserResult result = new ParserResult();
            result.File = file;
            BlockStatement block = ParseBlockStatement(tokens, state, options, result);
            if (block == null) return result;
            foreach (var item in block.Statements)
                statements.Add(item);
            return result;
        }

        // navigates to all other block statements, and parse to the expression block by default.
        // a block is a segment of expressions and statements surrounded by 'block' and 'end' pair.
        // a built-in datatype is for the block expression: 'expr'

        // namedExpr = block r = a + b; alert(r) end
        // namedExpr has an autoinferred type of expr.

        public BlockStatement ParseBlockStatement(TokenCollection tokens, ParserState state, 
            ParserOptions options, ParserResult result, BlockStatement parent = null)
        {
            if (tokens.Count == 0) {
                result.AddWarning(SyntaxError.EmptyTokenForBlock, Span.Default);
                return new BlockStatement();
            }

            switch (tokens[0].Value) {
                case "iter": state.Markers.Push(StackItem.Iterate); break;
                case "while": state.Markers.Push(StackItem.While); break;
                case "conditional": state.Markers.Push(StackItem.Conditional); break;
                case "data": state.Markers.Push(StackItem.Data); break;
                case "func": state.Markers.Push(StackItem.Func); break;
                case "config": state.Markers.Push(StackItem.Configure); break;
                case "if": state.Markers.Push(StackItem.If); break;
                case "eif": state.Markers.Push(StackItem.Eif); break;
                case "else": state.Markers.Push(StackItem.Else); break;
                case "match": state.Markers.Push(StackItem.Match); break;
                case "try": state.Markers.Push(StackItem.Try); break;
                case "catch": state.Markers.Push(StackItem.Catch); break;
                default: state.Markers.Push(StackItem.Block); break;
            }

            BlockStatement block;
            switch (tokens[0].Value) {
                case "iter": block = ParseIterateStatement(tokens, state, options, result, parent); break;
                case "while": block = ParseWhileStatement(tokens, state, options, result, parent); break;
                case "conditional": block = ParseConditionalStatement(tokens, state, options, result, parent); break;
                case "data": block = ParseDataStatement(tokens, state, options, result, parent); break;
                case "func": block = ParseFunctionStatement(tokens, state, options, result, parent); break;
                case "config": block = ParseConfigureStatement(tokens, state, options, result, parent); break;
                case "if": block = ParseIfStatement(tokens, state, options, result, parent); break;
                case "eif": block = ParseEIfStatement(tokens, state, options, result, parent); break;
                case "else": block = ParseElseStatement(tokens, state, options, result, parent); break;
                case "match": block = ParseMatchStatement(tokens, state, options, result, parent); break;
                case "try": block = ParseTryStatement(tokens, state, options, result, parent); break;
                case "catch": block = ParseCatchStatement(tokens, state, options, result, parent); break;
                case "(":
                case "[":
                case "{": block = ParseSequenceExpression(tokens, state, options, result); break;
                default:
                    block = new BlockStatement();
                    block.Name = "";
                    block.Tokens.AddRange(tokens);
                    if (tokens[0].Value == "block") {
                        tokens.RemoveAt(0);
                        if (tokens.Last().Value == "end")
                            tokens.RemoveLast();
                    }
                    block = ParseStatementList(block, tokens, state, options, result);
                    break;
            }

            state.Markers.Pop();
            return block; 
        }

        // try statements are used to avoid and ignore all errors(fatal and warnings) and
        // optionally jumps out of the block, or jumps to its corresbonding catch block.

        // 1.  try
        //         ...
        //     end

        // 2.  try
        //         ...
        //     catch exception 
        //     end

        public BlockStatement ParseTryStatement(TokenCollection tokens, ParserState state,
            ParserOptions options, ParserResult result, BlockStatement parent = null)
        {
            TryStatement tryStatement = new TryStatement(null);
            tryStatement.Tokens.AddRange(tokens);
            if (tokens.Last().Value == "end") tokens.RemoveLast();
            if (tokens[0].Value == "try") tokens.RemoveAt(0);

            return ParseStatementList(tryStatement, tokens, state, options, result);
        }

        // iterate statement provides alternative way of looping with simplified grammar. one can loop
        // for a certain times or enumerate the elements inside a collection.

        // the following code shows the iteration of elements in a collection, using iter ... in ...
        // clause. you can also have additional knowledge on where the enumerator is by adding
        // iter ... in ... at ... to obtain the information.
        //
        // 1.  iter item in collection
        //         item++
        //     end
        //
        // 2.  iter item in collection at position
        //         alert(position)
        //         ...
        //     end

        // if you just want to repeat the code for a certain time, and do not want to know anything more,
        // just iter + constant can have a good performance.
        //
        // 3.  iter 193
        //         ... (this will be performed 193 times)
        //     end

        public BlockStatement ParseIterateStatement(TokenCollection tokens, ParserState state,
            ParserOptions options, ParserResult result, BlockStatement parent = null)
        {
            TokenCollection storeToken = new TokenCollection();
            storeToken.AddRange(tokens);

            if (tokens.Last().Value == "end") tokens.RemoveLast();
            if (tokens[0].Value == "iter") tokens.RemoveAt(0);
            TokenCollection controller = tokens.SplitCascade(Token.LineBreak)[0];
            var content = tokens.SplitCascade(Token.LineBreak);
            content.RemoveAt(0);
            tokens.Clear();
            tokens.AddRange(content.JoinTokens());

            Literal atLiteral = null;
            if (controller.ContainsCascade("at")) {
                var atStmts = controller.SplitCascade("at");
                if(atStmts.Count!= 2) {
                    result.AddFatal(SyntaxError.IterateAtSyntaxError, controller.Last());
                    return null;
                }

                if (atStmts[1].Count > 1) {
                    result.AddWarning(SyntaxError.IterateAtSyntaxError, controller.Last());
                }

                atLiteral = new Literal(atStmts[1].First());
                controller = atStmts[0];
            }

            if (controller.ContainsCascade("in")) {
                var inStmts = controller.SplitCascade("in");
                if (inStmts.Count != 2) {
                    result.AddFatal(SyntaxError.IterateInSyntaxError, controller.Last());
                    return null;
                }

                if (inStmts[0].Count != 1)
                    result.AddWarning(SyntaxError.IterateInSyntaxError, inStmts[0].Last());

                if(atLiteral == null) {
                    IterateStatement iterate = new IterateStatement(new Literal(inStmts[0].First()),
                        ParseExpression(inStmts[1], state, options, result));
                    iterate.Tokens = storeToken;
                    return ParseStatementList(iterate, tokens, state, options, result);
                } else {
                    IteratePositionalStatement iterate = new IteratePositionalStatement(new Literal(inStmts[0].First()),
                        ParseExpression(inStmts[1], state, options, result), atLiteral);
                    iterate.Tokens = storeToken;
                    return ParseStatementList(iterate, tokens, state, options, result);
                }

            } else {
                if(controller.Count != 1) {
                    result.AddWarning(SyntaxError.IterateConstantSyntaxError, controller[0]);
                }

                IterateLiteralStatement iterateLiteral = new IterateLiteralStatement(new Literal(controller[0]));
                iterateLiteral.Tokens = storeToken;
                return ParseStatementList(iterateLiteral, tokens, state, options, result);
            }
        }

        // while is a sentinel loop, repeat doing the block statement until its evaluation turned false.
        
        // 1.  while condition != false
        //         ...
        //     end

        public BlockStatement ParseWhileStatement(TokenCollection tokens, ParserState state,
            ParserOptions options, ParserResult result, BlockStatement parent = null)
        {
            WhileStatement whileStatement = new WhileStatement(new Literal("none"));
            whileStatement.Tokens.AddRange(tokens);
            if (tokens.Last().Value == "end") tokens.RemoveLast();
            if (tokens[0].Value == "while") tokens.RemoveAt(0);
            TokenCollection controller = tokens.SplitCascade(Token.LineBreak)[0];

            if (controller.Count > 0) whileStatement.Evaluation = ParseExpression(controller, state, options, result);
            else result.AddFatal(SyntaxError.WhileConditionMissing, whileStatement.Tokens[0]);

            var content = tokens.SplitCascade(Token.LineBreak);
            content.RemoveAt(0);
            tokens.Clear();
            tokens.AddRange(content.JoinTokens());
            return ParseStatementList(whileStatement, tokens, state, options, result);
        }

        // conditional statements are the assertions used in assertion classes.
        // it evaluates whether an assertion class implements the base class.

        // 1.  data baseClass ( int32 baseField ) assert assertion
        //         ...
        //     end
        //
        //     data assertion ( int32 additionalField )
        //         conditional baseClass
        //             return base.baseField == 0
        //         end
        //     end

        public BlockStatement ParseConditionalStatement(TokenCollection tokens, ParserState state,
            ParserOptions options, ParserResult result, BlockStatement parent = null)
        {
            ConditionalStatement conditional = new ConditionalStatement(new Literal("none"));
            conditional.Tokens.AddRange(tokens);
            if (tokens.Last().Value == "end") tokens.RemoveLast();
            if (tokens[0].Value == "conditional") tokens.RemoveAt(0);
            TokenCollection controller = tokens.SplitCascade(Token.LineBreak)[0];

            if (controller.Count > 0) conditional.ConditionalType = ParseExpression(controller, state, options, result);
            else result.AddFatal(SyntaxError.UndefinedConditionalTarget, conditional.Tokens[0]);

            var content = tokens.SplitCascade(Token.LineBreak);
            content.RemoveAt(0);
            tokens.Clear();
            tokens.AddRange(content.JoinTokens());
            return ParseStatementList(conditional, tokens, state, options, result);
        }

        // data statements are declarations of user-datatype, i.e. classes in the program blocks.
        // data types contains members(fields) and functions declarations.

        // 1.  data (int32 field1, string|null field2)
        //         ...
        //     end

        // data types don't have to declare all members in use, if the program has another type as its base,
        // inheriting can initialize a data type from another.

        // 1.  data baseType (int32 baseField)
        //         ...
        //     end
        //     data newType (string field) : baseType
        //         ... (note that field and baseField are available here)
        //     end

        // assertion types are a special form of inherit base types, that the class can refer to the member
        // of the assertion base type only when its condition is reached.

        // 1.  data numeral (int32 value) assert odd
        //     end
        //     data odd()
        //         conditional numeral
        //             return base.value % 2 == 1
        //         end
        //         double = func () => numeral
        //             return numeral(value * 2)
        //         end
        //     end
        
        public BlockStatement ParseDataStatement(TokenCollection tokens, ParserState state,
            ParserOptions options, ParserResult result, BlockStatement parent = null)
        {
            DataDeclaration dataStatement = new DataDeclaration(new Literal(""));
            dataStatement.Tokens.AddRange(tokens); 
            if (tokens.Last().Value == "end") tokens.RemoveLast();
            if (tokens[0].Value == "data") tokens.RemoveAt(0);
            var content = tokens.SplitCascade(Token.LineBreak);

            if (content.Count == 0) {
                result.AddFatal(SyntaxError.DataDeclarationMissing, dataStatement.Tokens[0]);
                return null;
            }

            var declarator = content[0];
            if(declarator.ContainsCascade(new Token("assert"))) {
                var assertSecs = declarator.SplitCascade(new Token("assert"));
                if(assertSecs.Count!= 2) {
                    result.AddFatal(SyntaxError.DataAssertionSyntaxError, dataStatement.Tokens[0]);
                    return null;
                }
                var assertionList = assertSecs[1];
                var items = assertionList.SplitCascade(new Token(","));
                foreach (var assertion in items) {
                    if(assertion.Count == 0) {
                        result.AddFatal(SyntaxError.DataAssertionSyntaxError, dataStatement.Tokens[0]);
                        return null;
                    }

                    dataStatement.Assertions.Add(ParseExpression(assertion, state, options, result));
                }
                declarator = assertSecs[0];
            }

            if (declarator.ContainsCascade(new Token(":"))) {
                var inheritages = declarator.SplitCascade(new Token(":"));
                if (inheritages.Count != 2) {
                    result.AddFatal(SyntaxError.DataInheritageSyntaxError, dataStatement.Tokens[0]);
                    return null;
                }
                var inhlist = inheritages[1];
                var items = inhlist.SplitCascade(new Token(","));
                foreach (var inheritage in items) {
                    if (inheritage.Count == 0) {
                        result.AddFatal(SyntaxError.DataInheritageSyntaxError, dataStatement.Tokens[0]);
                        return null;
                    }

                    dataStatement.Inheritage.Add(ParseExpression(inheritage, state, options, result));
                }
                declarator = inheritages[0];
            }

            if (declarator.Last().Value == ")") declarator.RemoveLast();
            if (declarator[0].Value == "(") declarator.RemoveAt(0);
            List<TokenCollection> param = new List<TokenCollection>();
            int blockLevel = 0;
            TokenCollection temp = new TokenCollection();
            foreach (var item in declarator) {
                if(blockLevel == 0 && item.Value == ",") {
                    param.Add(temp);
                    temp = new TokenCollection();
                    continue;
                }

                switch(item.Value) {
                    case "(": case "[": case "{":
                        blockLevel++;
                        break;
                    case ")": case "]": case "}":
                        blockLevel--;
                        break;
                }

                temp.Add(item);
            }

            if (temp.Count > 0) param.Add(temp);
            foreach (var parameter in param) {
                if (parameter.Count > 0)
                    dataStatement.Fields.Add(ParseFunctionParameter(parameter, state, options, result));
            }

            content.RemoveAt(0);
            tokens.Clear();
            tokens.AddRange(content.JoinTokens());
            return ParseStatementList(dataStatement, tokens, state, options, result);
        }

        // functions declarations
        
        // 1.  add = func (int32 i, int32 j) => int32
        //         return i + j
        //     end

        public BlockStatement ParseFunctionStatement(TokenCollection tokens, ParserState state,
            ParserOptions options, ParserResult result, BlockStatement parent = null)
        {
            FunctionDeclaration funcDecl = new FunctionDeclaration(new Literal(""));
            funcDecl.Tokens.AddRange(tokens);
            if (tokens.Last().Value == "end") tokens.RemoveLast();
            if (tokens[0].Value == "func") tokens.RemoveAt(0);
            var content = tokens.SplitCascade(Token.LineBreak);

            if (content.Count == 0) {
                result.AddFatal(SyntaxError.FunctionDeclarationMissing, funcDecl.Tokens[0]);
                return null;
            }

            var declarator = content[0];
            
            if (declarator.ContainsCascade(new Token("=>"))) {
                var returnSecs = declarator.SplitCascade(new Token("=>"));
                if (returnSecs.Count != 2) {
                    result.AddFatal(SyntaxError.FunctionReturnTypeSyntaxError, funcDecl.Tokens[0]);
                    return null;
                }
                var ret = returnSecs[1];
                funcDecl.ReturnType = ParseExpression(ret, state, options, result);
                declarator = returnSecs[0];
            }

            if (declarator.Last().Value == ")") declarator.RemoveLast();
            if (declarator[0].Value == "(") declarator.RemoveAt(0);
            List<TokenCollection> param = new List<TokenCollection>();
            int blockLevel = 0;
            TokenCollection temp = new TokenCollection();
            foreach (var item in declarator) {
                if (blockLevel == 0 && item.Value == ",") {
                    param.Add(temp);
                    temp = new TokenCollection();
                    continue;
                }

                switch (item.Value) {
                    case "(":
                    case "[":
                    case "{":
                        blockLevel++;
                        break;
                    case ")":
                    case "]":
                    case "}":
                        blockLevel--;
                        break;
                }

                temp.Add(item);
            }

            if (temp.Count > 0) param.Add(temp);
            foreach (var parameter in param) {
                if (parameter.Count > 0)
                    funcDecl.Parameters.Add(ParseFunctionParameter(parameter, state, options, result));
            }

            content.RemoveAt(0);
            tokens.Clear();
            tokens.AddRange(content.JoinTokens());
            return ParseStatementList(funcDecl, tokens, state, options, result);
        }

        // configure statements are a short and efficient way when one want to modify many properties
        // or fields inside one object. this omitted the operant.

        // 1.  configure obj
        //         prop1 = 0
        //         prop2 = 0
        //         invokeFunction()
        //     end

        // configure block in execution returns the object modified. this can be written when initializing
        // a tree of value type using the following pattern (with the help of sequence insert operators |-)
        // or you can specify the object in return.
        
        // 2.  config window.children
        //      |- buttonDetails
        //      |- buttonRun
        //      |- taskButtonHelp
        //      |- config radioButtonGroup
        //          |- radioButtonApple = radioButton(text: "Apple")
        //          |- radioButtonPear
        //          |- radioButtonOrange
        //         end                        ' this by default returns the enumerable type radioButtonGroup
        //      |- config panelBottom.children
        //          |- buttonOK
        //          |- buttonCancel
        //          |- buttonExit
        //             return panelButtom
        //         end
        //         return window
        //     end

        public BlockStatement ParseConfigureStatement(TokenCollection tokens, ParserState state,
            ParserOptions options, ParserResult result, BlockStatement parent = null)
        {
            ConfigureStatement configure = new ConfigureStatement(new Literal("none"));
            configure.Tokens.AddRange(tokens);
            if (tokens.Last().Value == "end") tokens.RemoveLast();
            if (tokens[0].Value == "config") tokens.RemoveAt(0);
            TokenCollection controller = tokens.SplitCascade(Token.LineBreak)[0];

            if (controller.Count > 0) configure.ConfigurationObject = ParseExpression(controller, state, options, result);
            else result.AddFatal(SyntaxError.UndefinedConfigureTarget, configure.Tokens[0]);

            var content = tokens.SplitCascade(Token.LineBreak);
            content.RemoveAt(0);
            tokens.Clear();
            tokens.AddRange(content.JoinTokens());
            return ParseStatementList(configure, tokens, state, options, result);
        }

        // match statement represent a pattern-match execution. witha specified object to match and corresbonding
        // return value of the match.

        // 1.  result = match matchObject => int32
        //     !~> int32   => -1
        //         0       => 1
        //         1       => 1
        //       < 0       => -1
        //         int32 n => fib(n-1) + fib(n-2)
        //         any x   => -1
        //     end

        // match branches are conditional branches that are uniquely available in a pattern matching section.
        // it is splited by a '=>' signal whose left is the declarative pattern and whose right is the action
        // to take out. the right part, or the last expression of the right part if it is a block, is regarded
        // as the output value of match statement. the match branch has 3 forms.
        
        // 1.  this form is the simpliest form of match branch, specifying a constant value in the left, and 
        //     a return value in the right. this means if(matchObject == 0) return 1.
        //
        //     0 => 1
        //
        //     the right part can also be written in a block, and carry out extra commands. the last expression
        //     '1' is the return type of match statement.
        //
        //     0 => block
        //         alert("inside 0 pattern match branch")
        //         1
        //     end

        // 2.  match can also carry out relationship operators as a condition, when the left part begins with
        //     a valid relationship operator. the following branch means if(matchObject < 0) return -1.
        //
        //     < 0 => -1

        // 3.  the left part can also be written as a pair of target fields in a customized data type, and that
        //     filters any type with the given fields exist. the name of the selected type is given in the context
        //     in the evaluation process of the right.
        //
        //     (string name, int32 id) => return (name + id) -> string

        // when the pattern match detects no match in your given branches, this will throw a runtime error
        // so it is wise for you to specify a default branch to handle all other cases, like the (3) suggests,
        // you can write 'any' identifer to refer to all types:
        //
        //     any x => ...
        //
        // or you can write the following placeholder statement if you need not to know the value.
        //
        //     otherwise => ...

        public BlockStatement ParseMatchStatement(TokenCollection tokens, ParserState state,
            ParserOptions options, ParserResult result, BlockStatement parent = null)
        {
            MatchStatement matchBlock = new MatchStatement(new Literal(""), new Literal(""));
            matchBlock.Tokens.AddRange(tokens);
            if (tokens.Last().Value == "end") tokens.RemoveLast();
            if (tokens[0].Value == "match") tokens.RemoveAt(0);
            var content = tokens.SplitCascade(Token.LineBreak);

            if (content.Count == 0) {
                result.AddFatal(SyntaxError.MatchDeclarationMissing, matchBlock.Tokens[0]);
                return null;
            }

            var declarator = content[0];
            if(!declarator.ContainsCascade(new Token("=>"))) { /* todo */ }

            return matchBlock;
        }
        
        // the decision making branches if/eif/else.
        // try the conditions one by one until they meet at least one condition true, or the else statement,
        
        // 1.  if (condition1 == true)
        //         ...
        //     eif (condition2 == true)
        //         ...
        //     eif (conditionN == true)
        //         ...
        //     else
        //         ...
        //     end

        public BlockStatement ParseIfStatement(TokenCollection tokens, ParserState state,
            ParserOptions options, ParserResult result, BlockStatement parent = null)
        {
            IfStatement ifStatement = new IfStatement(new Literal("none"), null);
            ifStatement.Tokens.AddRange(tokens);
            if (tokens.Last().Value == "end") tokens.RemoveLast();
            if (tokens[0].Value == "if") tokens.RemoveAt(0);
            TokenCollection controller = tokens.SplitCascade(Token.LineBreak)[0];

            if (controller.Count > 0) ifStatement.Condition = ParseExpression(controller, state, options, result);
            else result.AddFatal(SyntaxError.IfConditionMissing, ifStatement.Tokens[0]);

            var content = tokens.SplitCascade(Token.LineBreak);
            content.RemoveAt(0);
            tokens.Clear();
            tokens.AddRange(content.JoinTokens());
            return ParseStatementList(ifStatement, tokens, state, options, result);
        }

        public BlockStatement ParseEIfStatement(TokenCollection tokens, ParserState state,
            ParserOptions options, ParserResult result, BlockStatement parent = null)
        {
            if (parent == null) {
                result.AddFatal(SyntaxError.StandaloneEif, tokens[0]);
                return null;
            }

            if (parent.Statements.Count == 0) {
                result.AddFatal(SyntaxError.StandaloneEif, tokens[0]);
                return null;
            }

            if (!(parent.Statements.Last() is IfStatement)) {
                result.AddFatal(SyntaxError.EifNotFollowingIfEif, tokens[0]);
                return null;
            }

            IfStatement previous = (IfStatement)parent.Statements.Last();
            IfStatement eifStatement = new IfStatement(new Literal("none"), null);
            eifStatement.Tokens.AddRange(tokens);
            if (tokens.Last().Value == "end") tokens.RemoveLast();
            if (tokens[0].Value == "eif") tokens.RemoveAt(0);
            TokenCollection controller = tokens.SplitCascade(Token.LineBreak)[0];

            if (controller.Count > 0) eifStatement.Condition = ParseExpression(controller, state, options, result);
            else result.AddFatal(SyntaxError.EifConditionMissing, eifStatement.Tokens[0]);

            var content = tokens.SplitCascade(Token.LineBreak);
            content.RemoveAt(0);
            tokens.Clear();
            tokens.AddRange(content.JoinTokens());
            previous.Otherwise = ParseStatementList(eifStatement, tokens, state, options, result);
            return null;
        }

        public BlockStatement ParseElseStatement(TokenCollection tokens, ParserState state,
            ParserOptions options, ParserResult result, BlockStatement parent = null)
        {
            if (parent == null) {
                result.AddFatal(SyntaxError.StandaloneElse, tokens[0]);
                return new BlockStatement();
            }

            if (parent.Statements.Count == 0) {
                result.AddFatal(SyntaxError.StandaloneElse, tokens[0]);
                return new BlockStatement();
            }

            if (!(parent.Statements.Last() is IfStatement)) {
                result.AddFatal(SyntaxError.ElseNotFollowingIfEif, tokens[0]);
                return new BlockStatement();
            }

            IfStatement previous = (IfStatement)parent.Statements.Last();
            while (previous.Otherwise != null) previous = (IfStatement)previous.Otherwise;
            IfStatement elseStatement = new IfStatement(new Literal("true"), null);
            elseStatement.Tokens.AddRange(tokens);
            if (tokens.Last().Value == "end") tokens.RemoveLast();
            if (tokens[0].Value == "else") tokens.RemoveAt(0);

            var content = tokens.SplitCascade(Token.LineBreak);
            content.RemoveAt(0);
            tokens.Clear();
            tokens.AddRange(content.JoinTokens());
            previous.Otherwise = ParseStatementList(elseStatement, tokens, state, options, result);
            return null;
        }

        // catch blocks handles the control when its try block has a unhandled error.
        // it receives a literal named parameter as the error captured.

        // 1.  try ...
        //     catch exception
        //         ...
        //     end

        public BlockStatement ParseCatchStatement(TokenCollection tokens, ParserState state,
            ParserOptions options, ParserResult result, BlockStatement parent = null)
        {
            if(parent == null) {
                result.AddFatal(SyntaxError.StandaloneCatch, tokens[0]);
                return new BlockStatement();
            }

            if(parent.Statements.Count == 0) {
                result.AddFatal(SyntaxError.StandaloneCatch, tokens[0]);
                return new BlockStatement();
            }
                
            if(!(parent.Statements.Last() is TryStatement)) {
                result.AddFatal(SyntaxError.CatchNotFollowingTry, tokens[0]);
                return new BlockStatement();
            }

            CatchStatement catchStatement = new CatchStatement((TryStatement)parent.Statements.Last(), new Literal("none"));
            catchStatement.Tokens.AddRange(tokens);
            if (tokens.Last().Value == "end") tokens.RemoveLast();
            if (tokens[0].Value == "catch") tokens.RemoveAt(0);
            TokenCollection controller = tokens.SplitCascade(Token.LineBreak)[0];

            if (controller.Count > 0) catchStatement.ExceptionName = new Literal(controller[0].Value);
            else result.AddFatal(SyntaxError.CatchUnnamedException, catchStatement.Tokens[0]);

            var content = tokens.SplitCascade(Token.LineBreak);
            content.RemoveAt(0);
            tokens.Clear();
            tokens.AddRange(content.JoinTokens());
            var tryStmt = (TryStatement)parent.Statements.Last();
            tryStmt.Catch = (CatchStatement)ParseStatementList(catchStatement, tokens, state, options, result);

            return null;
        }

        public BlockStatement ParseStatementList(BlockStatement block, TokenCollection tokens, 
            ParserState state, ParserOptions options, ParserResult result)
        {
            tokens.Add(Token.LineBreak);
            int cascadeDepth = state.Markers.Count;
            bool cascadeBlock = false;
            TokenCollection temperary = new TokenCollection();
            foreach (var token in tokens) {
                if (token.ContentEquals(Token.LineBreak) &&
                    state.Markers.Count == cascadeDepth) {
                    if (temperary.Count > 0) {
                        if (cascadeBlock && ("if eif try".Contains(temperary[0]) ||
                           (" iter while conditional data func config block if eif else try catch ".Contains(" " + temperary[0] + " ") && temperary.Last().Value == ("end")))) {
                            var parsingBlock = ParseBlockStatement(temperary, state, options, result, block);
                            block.Statements.AddIfNotNull(parsingBlock);
                            cascadeBlock = false;
                        } else {
                            block.Statements.Add(ParseExpression(temperary, state, options, result));
                        }
                    }

                    temperary = new TokenCollection();

                    continue;
                }

                bool inBlock = false;
                if (temperary.Count > 0) inBlock = ("if eif try".Contains(temperary[0]) ||
                             (" iter while conditional data func config block if eif else try catch ".Contains(" " + temperary[0] + " ")
                             && temperary.Last().Value == ("end")));

                // register stack record for cascading structures.

                switch (token.Value) {
                    case "iter": if (state.Markers.Count == cascadeDepth) cascadeBlock = true; state.Markers.Push(StackItem.Iterate); break;
                    case "while": if (state.Markers.Count == cascadeDepth) cascadeBlock = true; state.Markers.Push(StackItem.While); break;
                    case "conditional": if (state.Markers.Count == cascadeDepth) cascadeBlock = true;  state.Markers.Push(StackItem.Conditional); break;
                    case "data": if (state.Markers.Count == cascadeDepth) cascadeBlock = true;  state.Markers.Push(StackItem.Data); break;
                    case "func": if (state.Markers.Count == cascadeDepth) cascadeBlock = true;  state.Markers.Push(StackItem.Func); break;
                    case "config": if (state.Markers.Count == cascadeDepth) cascadeBlock = true;  state.Markers.Push(StackItem.Configure); break;
                    case "block": if (state.Markers.Count == cascadeDepth) cascadeBlock = true;  state.Markers.Push(StackItem.Block); break;
                    case "if": if (state.Markers.Count == cascadeDepth) cascadeBlock = true;  state.Markers.Push(StackItem.If); break;
                    case "eif": 
                        var previous = state.Markers.Peek();
                        if (inBlock) {
                            if (previous == StackItem.If || previous == StackItem.Eif) {
                                state.Markers.Pop();
                                if (state.Markers.Count == cascadeDepth) {
                                    var parsingBlock = ParseBlockStatement(temperary, state, options, result, block);
                                    block.Statements.AddIfNotNull(parsingBlock);
                                    temperary = new TokenCollection();
                                }
                            } else result.AddFatal(SyntaxError.ElseIfPlacement, token);
                        }
                        state.Markers.Push(StackItem.Eif); 
                        break;
                    case "else": 
                        previous = state.Markers.Peek();
                        if (inBlock) {
                            if (previous == StackItem.If || previous == StackItem.Eif) {
                                state.Markers.Pop();
                                if (state.Markers.Count == cascadeDepth) {
                                    var parsingBlock = ParseBlockStatement(temperary, state, options, result, block);
                                    block.Statements.AddIfNotNull(parsingBlock);
                                    temperary = new TokenCollection();
                                }
                            } else result.AddFatal(SyntaxError.ElsePlacement, token);
                        }
                        state.Markers.Push(StackItem.Else);  
                        break;
                    case "match": if (state.Markers.Count == cascadeDepth) cascadeBlock = true;  state.Markers.Push(StackItem.Match); break;
                    case "try": if (state.Markers.Count == cascadeDepth) cascadeBlock = true;  state.Markers.Push(StackItem.Try); break;
                    case "catch": 
                        previous = state.Markers.Peek();
                        if (inBlock) {
                            if (previous == StackItem.Try) {
                                state.Markers.Pop();
                                if (state.Markers.Count == cascadeDepth) {
                                    var parsingBlock = ParseBlockStatement(temperary, state, options, result, block);
                                    block.Statements.AddIfNotNull(parsingBlock);
                                    temperary = new TokenCollection();
                                }
                            } else result.AddFatal(SyntaxError.CatchPlacement, token);
                        }
                        state.Markers.Push(StackItem.Catch); 
                        break;
                    case "{": state.Markers.Push(StackItem.Bracket); break;
                    case "[": state.Markers.Push(StackItem.Bracket); break;
                    case "(": state.Markers.Push(StackItem.Bracket); break;

                    case "}":
                    case "]":
                    case ")":
                    case "end":
                        if (state.Markers.Count <= cascadeDepth) {
                            result.AddFatal(SyntaxError.EndOverflow, token);
                            break;
                        } 
                        state.Markers.Pop(); break;
                }

                temperary.Add(token);
            }

            return block;
        }

        public IExpression ParseExpression(TokenCollection tokens, 
            ParserState state, ParserOptions options, ParserResult result) 
        {
            List<IExpression> expressionUnits = new List<IExpression>();
            foreach (Token token in tokens) {
                expressionUnits.Add(new Literal(token.Value) { Tokens = new TokenCollection() { token }});
            }

            // parse operators in a determined priority level.
            // there are several different kinds of operator parsing patterns:
            // assignments are parsed with lowest priority right to left.
            // most of the common binary and unary operators are parsed left to right.
            // call expressions and member expressions are parsed left to right

            ParseEnclosedExpression(expressionUnits, state, options, result);

            while (ParseLtrs(expressionUnits, state, options, result)) { }

            foreach (var op in Operator.Operators) {
                while (ParseOperators(op.Value, expressionUnits, state, options, result)) { }
            }

            while (ParseRtls(expressionUnits, state, options, result)) { }

            // lazy identifer.

            // adding a 'lazy' identifer before an expression returns the expression form of it. it indicates
            // the expression is packaged into a 'expr' object and will not execute unless it is explicitly called
            // 'expr.exec()' 

            if (expressionUnits.Count > 0) {
                switch (expressionUnits[0].Tokens[0].Value.ToLower()) {
                    case "break": return new BreakStatement() { Tokens = tokens };
                    case "continue": return new ContinueStatement() { Tokens = tokens };
                    case "debugger": 
                        var debugger = new DebuggerStatement() { Tokens = tokens };
                        if(tokens.Count > 1)
                            switch (tokens[1].Value.ToLower()) {
                                case "on":
                                case "true":
                                    debugger.Toggle = true;
                                    break;
                                case "off":
                                case "false":
                                    debugger.Toggle = false;
                                    break;
                                default:
                                    debugger.Toggle = false;
                                    result.AddWarning(SyntaxError.UnrecognizedCommandment, tokens[1]);
                                    break;
                            }
                        if (tokens.Count > 2) result.AddWarning(SyntaxError.CommandmentParameterCountNotMatch, tokens[2]);
                        return debugger;
                    case "error":
                        var error = new ErrorStatement() { Tokens = tokens };
                        if (tokens.Count > 1) {
                            if (tokens[1].Type == TokenType.IntegerLiteral) {
                                error.ErrorId = Convert.ToInt32(tokens[1].Value);
                            } else if (tokens[1].Type == TokenType.StringLiteral) {
                                error.ErrorText = tokens[1].Value;
                            } else result.AddWarning(SyntaxError.UnrecognizedCommandment, tokens[1]);
                        } 
                        
                        if (tokens.Count > 2) result.AddWarning(SyntaxError.CommandmentParameterCountNotMatch, tokens[2]);
                        return error;
                    case "warning":
                        var warning = new WarningStatement() { Tokens = tokens };
                        if (tokens.Count > 1) {
                            if (tokens[1].Type == TokenType.IntegerLiteral) {
                                warning.ErrorId = Convert.ToInt32(tokens[1].Value);
                            } else if (tokens[1].Type == TokenType.StringLiteral) {
                                warning.ErrorText = tokens[1].Value;
                            } else result.AddWarning(SyntaxError.UnrecognizedCommandment, tokens[1]);
                        }

                        if (tokens.Count > 2) result.AddWarning(SyntaxError.CommandmentParameterCountNotMatch, tokens[2]);
                        return warning;
                    case "module":
                        if (tokens.Count > 1) {
                            var original = new TokenCollection();
                            original.AddRange(tokens);
                            tokens.RemoveAt(0);
                            return new ModuleStatement(tokens.JoinString("")) { Tokens = original } ;
                        } else result.AddWarning(SyntaxError.CommandmentParameterCountNotMatch, tokens[0]);
                        return new Literal("none");
                    case "use":
                        if (tokens.Count > 1) {
                            var original = new TokenCollection();
                            original.AddRange(tokens);
                            tokens.RemoveAt(0);
                            return new UseStatement(tokens.JoinString("")) { Tokens = original };
                        } else result.AddWarning(SyntaxError.CommandmentParameterCountNotMatch, tokens[0]);
                        return new Literal("none");
                    case "return":
                        if (tokens.Count == 1) return new ReturnStatement(null) { Tokens = tokens } ;
                        else {
                            var original = new TokenCollection();
                            original.AddRange(tokens);
                            tokens.RemoveAt(0);
                            return new ReturnStatement(ParseExpression(tokens, state, options, result)) { Tokens = original };
                        }
                    case "touch":
                        if (tokens.Count == 1) return new TouchStatement("");
                        else {
                            var original = new TokenCollection();
                            original.AddRange(tokens);
                            tokens.RemoveAt(0);
                            return new TouchStatement(tokens.JoinString("")) { Tokens = original };
                        }

                    // the identifer 'def', 'hidden' or 'expose' tells the parser to parse the following binary
                    // anonymous declaration as global ones, and should be considered as global functions or
                    // data types. otherwise, the functions and data-types defined will be treated only as private
                    // varaibles or lazy-load blocks.

                    // 1.  lambda = func (int32 i) => int32
                    //         ...
                    //     end

                    // 2.  expose function = func (int32 i) => int32
                    //         ...
                    //     end

                    // when loading sources from a collection of files, we only import the exposed functions
                    // and data-types into the execution context. any private variables and statements will
                    // not be executed. the other non-definition statements will only be executed if you run
                    // exactly that file.

                    // [file a.ss]
                    //     expose msgbox = func (string str)
                    //         alert("inside function `msgbox`: " + str)
                    //     end
                    //     internalFunction = func (string str)
                    //         alert("inside function `internalFunction`: " + str)
                    //     end
                    //     alert("invoke a.ss")  ' this will not executed unless you run a.ss
                    //
                    // [file b.ss] |> run this file
                    //     anotherFunc = func (string str)
                    //         alert("inside function `anotherFunc`: " + str)
                    //     end
                    //     anotherFunc("can invoke")
                    //     msgbox("can invoke")
                    //   ' internalFunction("not visible")

                    case "def":
                    case "hidden":
                        if(expressionUnits.Count == 2) {
                            if(expressionUnits[1] is BinaryExpression binary) {
                                if (binary.Left is Literal literal &&
                                    binary.Right is DeclarationBlock decl) {
                                    decl.Identifer = literal;
                                    decl.IsExposed = false;
                                    decl.IsReadonly = true;
                                    return decl;
                                } else {
                                    result.AddFatal(SyntaxError.DeclarationSyntaxError, tokens[0]);
                                    break;
                                }
                            } else {
                                result.AddFatal(SyntaxError.ExpectDeclaration, tokens[0]);
                                break;
                            }
                            
                        } else {
                            result.AddFatal(SyntaxError.DeclarationSyntaxError, tokens[0]);
                            break;
                        }

                    case "expose":
                        if (expressionUnits.Count == 2) {
                            if (expressionUnits[1] is BinaryExpression binary) {
                                if (binary.Left is Literal literal &&
                                    binary.Right is DeclarationBlock decl) {
                                    decl.Identifer = literal;
                                    decl.IsExposed = true;
                                    decl.IsReadonly = true;
                                    return decl;
                                } else {
                                    result.AddFatal(SyntaxError.DeclarationSyntaxError, tokens[0]);
                                    break;
                                }
                            } else {
                                result.AddFatal(SyntaxError.ExpectDeclaration, tokens[0]);
                                break;
                            }

                        } else {
                            result.AddFatal(SyntaxError.DeclarationSyntaxError, tokens[0]);
                            break;
                        }
                        
                    default:
                        break;
                }
                return expressionUnits[0];
            } else {
                result.AddFatal(SyntaxError.EmptyExpression, Span.Default);
                return new Literal("none");
            }
        }

        public FunctionParameter ParseFunctionParameter(TokenCollection tokens,
            ParserState state, ParserOptions options, ParserResult result)
        {
            List<IExpression> expressionUnits = new List<IExpression>();
            foreach (Token token in tokens) {
                expressionUnits.Add(new Literal(token.Value) { Tokens = new TokenCollection() { token } });
            }

            ParseEnclosedExpression(expressionUnits, state, options, result);

            while (ParseLtrs(expressionUnits, state, options, result)) { }

            foreach (var op in Operator.Operators) {
                while (ParseOperators(op.Value, expressionUnits, state, options, result)) { }
            }

            while (ParseRtls(expressionUnits, state, options, result)) { }

            if(expressionUnits.Count==0) {
                result.AddFatal(SyntaxError.EmptyExpression, tokens[0]);
                return new FunctionParameter();
            }

            if (expressionUnits.Count >= 2) {
                FunctionParameter param = new FunctionParameter();
                if (expressionUnits.Last() is BinaryExpression bin && bin.Operator.Symbol == "=") {
                    param.Default = bin.Right;
                    if (bin.Left is Literal literal)
                        param.Identifer = literal;
                    else {
                        result.AddFatal(SyntaxError.ExpectLiteralParameterName, bin.Left.Tokens[0]);
                        param.Identifer = new Literal(bin.Left.Tokens[0].Value);
                    }

                } else {
                    if (expressionUnits.Last() is Literal literal)
                        param.Identifer = literal;
                    else {
                        result.AddFatal(SyntaxError.ExpectLiteralParameterName, expressionUnits.Last().Tokens[0]);
                        param.Identifer = new Literal(expressionUnits.Last().Tokens[0].Value);
                    }
                }

                expressionUnits.RemoveAt(expressionUnits.Count - 1);
                param.Type = expressionUnits.Last();
                expressionUnits.RemoveAt(expressionUnits.Count - 1);

                foreach (var item in expressionUnits) {
                    if (item is Literal litem)
                        param.Modifers.Add(litem);
                    else {
                        result.AddFatal(SyntaxError.ExpectLiteralModifers, item.Tokens[0]);
                        continue;
                    }
                }

                return param;
            } else {
                result.AddFatal(SyntaxError.InvalidParameter, Span.Default);
                return new FunctionParameter();
            }
        }

        // this method parses all the enclosed expressions in a expression subunit. enclosed expressions
        // (blocks and brackets) are the top priority elements in a expression.

        public void ParseEnclosedExpression(List<IExpression> expressions,
            ParserState state, ParserOptions options, ParserResult result)
        {
            int baseline = 0;
            TokenCollection subunit = new TokenCollection();
            int removeStart = 0; int removeDuration = 0;
            for (int i = 0; i < expressions.Count; i++) {
                var item = expressions[i];
                if(item is Literal literal) {
                    switch (literal.Value) {
                        
                        // this block-beginning identifers are signs of entering a new block.
                        // while other identifers (eif, else, catch) are a state change within a block.

                        case "iter": 
                        case "while": 
                        case "conditional": 
                        case "data": 
                        case "func": 
                        case "config":
                        case "block":
                        case "if":
                        case "match": 
                        case "try":
                        case "{":
                        case "[":
                        case "(":
                            baseline++; 
                            if(baseline == 1) {
                                removeStart = i;
                                removeDuration = 0;
                            }
                            break;

                        case "eif":
                            if (baseline == 1) {
                                var previous = ParseBlockStatement(subunit, state, options, result);
                                expressions.RemoveRange(removeStart, removeDuration);
                                expressions.InsertIfNotNull(removeStart, previous);
                                subunit = new TokenCollection();
                                i = removeStart;
                            }
                            break;
                        case "else":
                            if (baseline == 1) {
                                var previous = ParseBlockStatement(subunit, state, options, result);
                                expressions.RemoveRange(removeStart, removeDuration);
                                expressions.InsertIfNotNull(removeStart, previous);
                                subunit = new TokenCollection();
                                i = removeStart;
                            }
                            break;
                        
                        case "catch":
                            if (baseline == 1) {
                                var previous = ParseBlockStatement(subunit, state, options, result);
                                expressions.RemoveRange(removeStart, removeDuration);
                                expressions.InsertIfNotNull(removeStart, previous);
                                subunit = new TokenCollection();
                                i = removeStart;
                            }
                            break;

                        case "}": 
                        case "]":
                        case ")":
                        case "end":
                            if (baseline == 1) {
                                subunit.Add(literal.Tokens[0]); baseline--; removeDuration++;
                                var block = ParseBlockStatement(subunit, state, options, result);
                                expressions.RemoveRange(removeStart, removeDuration);
                                expressions.InsertIfNotNull(removeStart, block);
                                subunit = new TokenCollection();
                                i = removeStart;
                                break;
                            }

                            baseline--;
                            break;
                    }
                }

                if (baseline > 0) {
                    subunit.AddRange(item.Tokens);
                    removeDuration++;
                }
            }
        }

        public BlockStatement ParseSequenceExpression(TokenCollection tokens, ParserState state,
            ParserOptions options, ParserResult result)
        {
            SequenceExpression returns = new TupleExpression();
            returns.Tokens.AddRange(tokens);
            bool ignoreNewlines = true;
            if(tokens.Count>=1) {
                if (tokens[0].Value == "(" ||
                    tokens[0].Value == "[" ||
                    tokens[0].Value == "{" ) {
                    var lastValue = tokens.Last().Value;
                    if (tokens[0].Value == "[") ignoreNewlines = false;
                    if (lastValue == ")" ||
                        lastValue == "]" ||
                        lastValue == "}") {
                        tokens.RemoveLast();
                        tokens.RemoveAt(0);
                    } else {
                        result.AddFatal(SyntaxError.UnpairedBrackets, tokens[0]);
                        return returns;
                    }
                }

                // parsing the enclosed expression without brackets.

                // by default, if the expression is wrapped in a pair of parenthesis, it is parsed into tuples
                // and will ignore all the newline characters, if it is brackets, it consider the newlines as 
                // new rows to be parsed into a matrix.

                // note that i) the matrix will ignore all rows with zero elements. ii) if the rows are of variable
                // lengths, this will throw a syntax error when parsing the matrix.

                if (ignoreNewlines) {
                    if (tokens.Count == 0) return new TupleExpression() { Bracket = BracketType.Parenthesis };
                    List<TokenCollection> exprs = new List<TokenCollection>();
                    TokenCollection exprBlocks = new TokenCollection();

                    int baseline = 0;
                    foreach (var item in tokens) {
                        switch (item.Value) {
                            case "iter":
                            case "while":
                            case "conditional":
                            case "data":
                            case "func":
                            case "config":
                            case "block":
                            case "if":
                            case "match":
                            case "try":
                            case "{":
                            case "[":
                            case "(": baseline++; break;

                            case "eif":
                            case "else":
                            case "catch": break;


                            case "}":
                            case "]":
                            case ")":
                            case "end": baseline--; break;
                        }

                        if (item.Value == "," && baseline == 0) {
                            exprs.Add(exprBlocks);
                            exprBlocks = new TokenCollection();
                        } else if (baseline == 0 && (item.Value == ";" || item.ContentEquals(Token.LineBreak))) { // ignore newlines
                        } else exprBlocks.Add(item);
                    }

                    if (exprBlocks.Count > 0) exprs.Add(exprBlocks);
                    TupleExpression tuple = new TupleExpression();
                    tuple.Bracket = BracketType.Parenthesis;
                    foreach (var expression in exprs) {
                        tuple.Statements.Add(ParseExpression(expression, state, options, result));
                    }

                    tuple.Tokens.AddRange(returns.Tokens);
                    return tuple;

                } else {
                    if (tokens.Count == 0) return new Matrix2DExpression() { Bracket = BracketType.Bracket };
                    List<TokenCollection> exprs = new List<TokenCollection>();
                    TokenCollection exprBlocks = new TokenCollection();
                    int rowCount = 1;
                    int columnCount = 0; int tempCol = 1;

                    int baseline = 0;
                    foreach (var item in tokens) {
                        switch (item.Value) {
                            case "iter":
                            case "while":
                            case "conditional":
                            case "data":
                            case "func":
                            case "config":
                            case "block":
                            case "if":
                            case "match":
                            case "try":
                            case "{":
                            case "[":
                            case "(": baseline++; break;

                            case "eif":
                            case "else":
                            case "catch": break;


                            case "}":
                            case "]":
                            case ")":
                            case "end": baseline--; break;
                        }

                        if (item.Value == "," && baseline == 0) {
                            tempCol++;
                            exprs.Add(exprBlocks);
                            exprBlocks = new TokenCollection();
                        } else if (baseline == 0 && ((item.Value == ";" || item.ContentEquals(Token.LineBreak)))) {

                            // the current line contains no element.

                            if (tempCol == 1 && exprBlocks.Count == 0) { tempCol = 1; } else {
                                if (exprBlocks.Count == 0) {

                                    // this means the last element in a row is empty, this is not allowed and 
                                    // will throw a syntax warning.

                                    result.AddWarning(SyntaxError.MatrixEmptyElement, returns.Tokens[0]);
                                    exprBlocks.Add((Token)"none");
                                }

                                exprs.Add(exprBlocks);
                                exprBlocks = new TokenCollection();
                                if (columnCount == 0) columnCount = tempCol;
                                else { if (columnCount != tempCol) {
                                        result.AddFatal(SyntaxError.MatrixNotUniform, returns.Tokens[0]);
                                        return new Matrix2DExpression() { Bracket = BracketType.Bracket };
                                    } }

                                rowCount++;
                                tempCol = 1;
                            }

                        } else exprBlocks.Add(item);
                    }

                    if (columnCount == 0 && exprBlocks.Count == 0) return new Matrix2DExpression() { Bracket = BracketType.Bracket };
                    if (columnCount == 0 ) columnCount = tempCol;
                    if (exprBlocks.Count > 0) exprs.Add(exprBlocks);

                    if(rowCount*columnCount != exprs.Count) {
                        result.AddFatal(SyntaxError.InternalMatrixAssignment, returns.Tokens[0]);
                        return new Matrix2DExpression() { Bracket = BracketType.Bracket };
                    }

                    Matrix2DExpression matrix = new Matrix2DExpression();
                    matrix.Bracket = BracketType.Bracket;
                    int linearCount = 0;
                    matrix.Width = columnCount;
                    matrix.Height = rowCount;
                    matrix.Expressions = new IExpression[rowCount, columnCount];
                    foreach (var expression in exprs) {
                        matrix.Statements.Add(ParseExpression(expression, state, options, result));
                        matrix.Expressions[linearCount / columnCount, linearCount % columnCount] = matrix.Statements[matrix.Statements.Count - 1];
                        linearCount++;
                    }

                    matrix.Tokens.AddRange(returns.Tokens);
                    return matrix;
                }

            } else {
                result.AddFatal(SyntaxError.EmptyTokenForBlock, Span.Default);
                return returns;
            }
        }

        public bool ParseOperators(Operator op, List<IExpression> expressions,
            ParserState state, ParserOptions options, ParserResult result)
        {
            for(int counter = 0; counter < expressions.Count; counter++) {
                var unit = expressions[counter];
                IExpression left = counter == 0 ? null : expressions[counter - 1];
                IExpression right = counter == expressions.Count - 1 ? null : expressions[counter + 1];
                if(unit is Literal literal) {
                    if (literal.Value == op.Symbol) {
                        switch (op.Type) {
                            case OperatorType.UnaryLeft:
                                if (right == null) continue;
                                if (right is Literal literalRight)
                                    if (new Token(literalRight.Value).IsValidSymbolBeginning()) continue;
                                UnaryExpression unaryLeft = new UnaryExpression(right) { Operator = op };
                                unaryLeft.Tokens.AddRange(literal.Tokens);
                                unaryLeft.Tokens.AddRange(right.Tokens);
                                expressions.RemoveRange(counter, 2);
                                expressions.Insert(counter, unaryLeft);
                                return true;
                            case OperatorType.UnaryRight:
                                if (left == null) continue;
                                if (left is Literal literalLeft)
                                    if (new Token(literalLeft.Value).IsValidSymbolBeginning()) continue;
                                UnaryExpression unaryRight = new UnaryExpression(left) { Operator = op };
                                unaryRight.Tokens.AddRange(left.Tokens);
                                unaryRight.Tokens.AddRange(literal.Tokens);
                                expressions.RemoveRange(counter - 1, 2);
                                expressions.Insert(counter - 1, unaryRight);
                                return true;
                            case OperatorType.Binary:
                                if (left == null || right == null) continue;
                                if (left is Literal lleft)
                                    if (new Token(lleft.Value).IsValidSymbolBeginning()) continue;
                                if (right is Literal lright)
                                    if (new Token(lright.Value).IsValidSymbolBeginning()) continue;
                                BinaryExpression bin = new BinaryExpression(left, right) { Operator = op };
                                bin.Tokens.AddRange(left.Tokens);
                                bin.Tokens.AddRange(literal.Tokens);
                                bin.Tokens.AddRange(right.Tokens);
                                expressions.RemoveRange(counter - 1, 3);
                                expressions.Insert(counter - 1, bin);
                                return true;
                            default:
                                break;
                        }
                    }
                }
            }

            return false;
        }

        public bool ParseLtrs(List<IExpression> expressions,
            ParserState state, ParserOptions options, ParserResult result)
        {
            // the left-to-right flow operators includes tuple(=> function calls), matrix2d(=> index calls)
            // and dot member operator. the scanner went through the expressions from left to right and 
            // match these 3 patterns one by one in a turn.

            // if a tuple has a sibling in its left, which is not a symbol, it is turned to function calls.
            // if a matrix2d has a sibling in its left, which is not a symbol, it is turned to index.
            // if a dot has 2 non-symbol siblings, it is turned to a member operation.

            for(int counter = 0; counter<expressions.Count; counter++) {
                var item = expressions[counter];
                IExpression left = counter == 0 ? null : expressions[counter - 1];
                IExpression right = counter == expressions.Count - 1 ? null : expressions[counter + 1];
                if (item is Literal literal) {
                    if(literal.Value == ".") {
                        if (left == null || right == null) continue;
                        if (left is Literal lleft)
                            if (new Token(lleft.Value).IsValidSymbolBeginning()) continue;
                        if (right is Literal lright)
                            if (new Token(lright.Value).IsValidSymbolBeginning()) continue;
                        BinaryExpression bin = new BinaryExpression(left, right) { Operator = Operator.MemberOperator };
                        bin.Tokens.AddRange(left.Tokens);
                        bin.Tokens.AddRange(literal.Tokens);
                        bin.Tokens.AddRange(right.Tokens);
                        expressions.RemoveRange(counter - 1, 3);
                        expressions.Insert(counter - 1, bin);
                        return true;
                    }
                } else if (item is TupleExpression tuple) {
                    if (left == null) continue;
                    if (left is Literal literalLeft)
                        if (new Token(literalLeft.Value).IsValidSymbolBeginning()) continue;
                    CallExpression unaryRight = new CallExpression(left);
                    unaryRight.Arguments = tuple.Statements;
                    unaryRight.Tokens.AddRange(left.Tokens);
                    unaryRight.Tokens.AddRange(item.Tokens);
                    expressions.RemoveRange(counter - 1, 2);
                    expressions.Insert(counter - 1, unaryRight);
                    return true;
                } else if(item is Matrix2DExpression matrix) {
                    if (left == null) continue;
                    if (left is Literal literalLeft)
                        if (new Token(literalLeft.Value).IsValidSymbolBeginning()) continue;
                    IndexExpression unaryRight = new IndexExpression(left);
                    unaryRight.Arguments = matrix.Statements;
                    unaryRight.Tokens.AddRange(left.Tokens);
                    unaryRight.Tokens.AddRange(item.Tokens);
                    expressions.RemoveRange(counter - 1, 2);
                    expressions.Insert(counter - 1, unaryRight);
                    return true;
                }
            }

            return false;
        }

        public bool ParseRtls(List<IExpression> expressions,
            ParserState state, ParserOptions options, ParserResult result)
        {
            // the right-to-left operators includes assignments operators "=".

            for(int counter = expressions.Count - 1; counter >= 0; counter--) {
                var item = expressions[counter];
                IExpression left = counter == 0 ? null : expressions[counter - 1];
                IExpression right = counter == expressions.Count - 1 ? null : expressions[counter + 1];
                if (item is Literal literal) {
                    if (literal.Value == "=") {
                        if (left == null || right == null) continue;
                        if (left is Literal lleft)
                            if (new Token(lleft.Value).IsValidSymbolBeginning()) continue;
                        if (right is Literal lright)
                            if (new Token(lright.Value).IsValidSymbolBeginning()) continue;
                        BinaryExpression bin = new BinaryExpression(left, right) { Operator = Operator.AssignmentOperator };
                        bin.Tokens.AddRange(left.Tokens);
                        bin.Tokens.AddRange(literal.Tokens);
                        bin.Tokens.AddRange(right.Tokens);
                        expressions.RemoveRange(counter - 1, 3);
                        expressions.Insert(counter - 1, bin);
                        return true;
                    }
                }
            }

            return false;
        }
    }
}

namespace Simula
{
    public static partial class Extension
    {
        public static void AddIfNotNull<T>(this IList<T> exprs, T item) where T : class
        {
            if (item == null) return;
            else exprs.Add(item);
        }

        public static void AddRangeIfNotNull<T>(this IList<T> exprs, IList<T> item) where T : class
        {
            if (item == null) return;
            foreach (var range in item) {
                exprs.AddIfNotNull(range);
            }
        }

        public static void AddRangeIfNotNull<T>(this IList<T> exprs, T[] item) where T : class
        {
            if (item == null) return;
            foreach (var range in item) {
                exprs.AddIfNotNull(range);
            }
        }

        public static void InsertIfNotNull<T>(this IList<T> exprs, int position, T item) where T : class
        {
            if (item == null) return;
            exprs.Insert(position, item);
        }
    }
}
