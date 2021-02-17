using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Simula.Scripting.Parser.Ast;

namespace Simula.Scripting.Parser
{
    public class Parser
    {
        public Program? Parse(TokenCollection collection) 
        {
            Program program = new Program();
            List<Ast.IExpression> statements = new List<Ast.IExpression>();
            TokenCollection tokens = new TokenCollection();
            foreach (var item in collection) {
                if (item.Type != TokenType.Comment) tokens.Add(item);
            }

            ParserResult result = Parse(statements, tokens, new ParserState(), ParserOptions.Default);
            program.Body = statements;
            program.Result = result;
            if(!result.Successful) return program;
            else return program;
        }

        public ParserResult Parse(List<IExpression> statements, TokenCollection tokens, ParserState state, ParserOptions options) 
        {
            ParserResult result = new ParserResult();
            BlockStatement? block = ParseBlockStatement(tokens, state, options, result);
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

        public BlockStatement? ParseBlockStatement(TokenCollection tokens, ParserState state, 
            ParserOptions options, ParserResult result, BlockStatement? parent = null)
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

            BlockStatement? block;
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

        public BlockStatement? ParseTryStatement(TokenCollection tokens, ParserState state,
            ParserOptions options, ParserResult result, BlockStatement? parent = null)
        {
            TryStatement tryStatement = new TryStatement(null);
            tryStatement.Tokens.AddRange(tokens);
            if (tokens.Last().Value == "end") tokens.RemoveLast();
            if (tokens[0].Value == "try") tokens.RemoveAt(0);

            return ParseStatementList(tryStatement, tokens, state, options, result);
        }

        public BlockStatement? ParseIterateStatement(TokenCollection tokens, ParserState state,
            ParserOptions options, ParserResult result, BlockStatement? parent = null)
        {
            IterateStatement tryStatement = new IterateStatement(new Literal(""), new Literal(""));
            tryStatement.Tokens.AddRange(tokens);
            if (tokens[0].Value == "iter") tokens.RemoveAt(0);

            return ParseStatementList(tryStatement, tokens, state, options, result);
        }

        // while is a sentinel loop, repeat doing the block statement until its evaluation turned false.
        
        // 1.  while condition != false
        //         ...
        //     end

        public BlockStatement? ParseWhileStatement(TokenCollection tokens, ParserState state,
            ParserOptions options, ParserResult result, BlockStatement? parent = null)
        {
            WhileStatement whileStatement = new WhileStatement(new Literal("none"));
            whileStatement.Tokens.AddRange(tokens);
            if (tokens.Last().Value == "end") tokens.RemoveLast();
            if (tokens[0].Value == "while") tokens.RemoveAt(0);
            TokenCollection controller = tokens.Split(Token.LineBreak)[0];

            if (controller.Count > 0) whileStatement.Evaluation = ParseExpression(controller, state, options, result);
            else result.AddFatal(SyntaxError.WhileConditionMissing, whileStatement.Tokens[0]);

            var content = tokens.Split(Token.LineBreak);
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

        public BlockStatement? ParseConditionalStatement(TokenCollection tokens, ParserState state,
            ParserOptions options, ParserResult result, BlockStatement? parent = null)
        {
            ConditionalStatement conditional = new ConditionalStatement(new Literal("none"));
            conditional.Tokens.AddRange(tokens);
            if (tokens.Last().Value == "end") tokens.RemoveLast();
            if (tokens[0].Value == "conditional") tokens.RemoveAt(0);
            TokenCollection controller = tokens.Split(Token.LineBreak)[0];

            if (controller.Count > 0) conditional.ConditionalType = ParseExpression(controller, state, options, result);
            else result.AddFatal(SyntaxError.UndefinedConditionalTarget, conditional.Tokens[0]);

            var content = tokens.Split(Token.LineBreak);
            content.RemoveAt(0);
            tokens.Clear();
            tokens.AddRange(content.JoinTokens());
            return ParseStatementList(conditional, tokens, state, options, result);
        }

        public BlockStatement? ParseDataStatement(TokenCollection tokens, ParserState state,
            ParserOptions options, ParserResult result, BlockStatement? parent = null)
        {
            DataDeclaration tryStatement = new DataDeclaration(new Literal(""));
            tryStatement.Tokens.AddRange(tokens);
            if (tokens[0].Value == "data") tokens.RemoveAt(0);

            return ParseStatementList(tryStatement, tokens, state, options, result);
        }

        public BlockStatement? ParseFunctionStatement(TokenCollection tokens, ParserState state,
            ParserOptions options, ParserResult result, BlockStatement? parent = null)
        {
            FunctionDeclaration tryStatement = new FunctionDeclaration(new Literal(""));
            tryStatement.Tokens.AddRange(tokens);
            if (tokens[0].Value == "func") tokens.RemoveAt(0);
            if (tokens.Last().Value == "end") tokens.RemoveLast();

            return ParseStatementList(tryStatement, tokens, state, options, result);
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

        public BlockStatement? ParseConfigureStatement(TokenCollection tokens, ParserState state,
            ParserOptions options, ParserResult result, BlockStatement? parent = null)
        {
            ConfigureStatement configure = new ConfigureStatement(new Literal("none"));
            configure.Tokens.AddRange(tokens);
            if (tokens.Last().Value == "end") tokens.RemoveLast();
            if (tokens[0].Value == "config") tokens.RemoveAt(0);
            TokenCollection controller = tokens.Split(Token.LineBreak)[0];

            if (controller.Count > 0) configure.ConfigurationObject = ParseExpression(controller, state, options, result);
            else result.AddFatal(SyntaxError.UndefinedConfigureTarget, configure.Tokens[0]);

            var content = tokens.Split(Token.LineBreak);
            content.RemoveAt(0);
            tokens.Clear();
            tokens.AddRange(content.JoinTokens());
            return ParseStatementList(configure, tokens, state, options, result);
        }

        public BlockStatement? ParseMatchStatement(TokenCollection tokens, ParserState state,
            ParserOptions options, ParserResult result, BlockStatement? parent = null)
        {
            MatchStatement tryStatement = new MatchStatement(new Literal(""), new Literal(""));
            tryStatement.Tokens.AddRange(tokens);
            if (tokens[0].Value == "match") tokens.RemoveAt(0);

            return ParseStatementList(tryStatement, tokens, state, options, result);
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

        public BlockStatement? ParseIfStatement(TokenCollection tokens, ParserState state,
            ParserOptions options, ParserResult result, BlockStatement? parent = null)
        {
            IfStatement ifStatement = new IfStatement(new Literal("none"), null);
            ifStatement.Tokens.AddRange(tokens);
            if (tokens.Last().Value == "end") tokens.RemoveLast();
            if (tokens[0].Value == "if") tokens.RemoveAt(0);
            TokenCollection controller = tokens.Split(Token.LineBreak)[0];

            if (controller.Count > 0) ifStatement.Condition = ParseExpression(controller, state, options, result);
            else result.AddFatal(SyntaxError.IfConditionMissing, ifStatement.Tokens[0]);

            var content = tokens.Split(Token.LineBreak);
            content.RemoveAt(0);
            tokens.Clear();
            tokens.AddRange(content.JoinTokens());
            return ParseStatementList(ifStatement, tokens, state, options, result);
        }

        public BlockStatement? ParseEIfStatement(TokenCollection tokens, ParserState state,
            ParserOptions options, ParserResult result, BlockStatement? parent = null)
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
            TokenCollection controller = tokens.Split(Token.LineBreak)[0];

            if (controller.Count > 0) eifStatement.Condition = ParseExpression(controller, state, options, result);
            else result.AddFatal(SyntaxError.EifConditionMissing, eifStatement.Tokens[0]);

            var content = tokens.Split(Token.LineBreak);
            content.RemoveAt(0);
            tokens.Clear();
            tokens.AddRange(content.JoinTokens());
            previous.Otherwise = ParseStatementList(eifStatement, tokens, state, options, result);
            return null;
        }

        public BlockStatement? ParseElseStatement(TokenCollection tokens, ParserState state,
            ParserOptions options, ParserResult result, BlockStatement? parent = null)
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

            var content = tokens.Split(Token.LineBreak);
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

        public BlockStatement? ParseCatchStatement(TokenCollection tokens, ParserState state,
            ParserOptions options, ParserResult result, BlockStatement? parent = null)
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
            TokenCollection controller = tokens.Split(Token.LineBreak)[0];

            if (controller.Count > 0) catchStatement.ExceptionName = new Literal(controller[0].Value);
            else result.AddFatal(SyntaxError.CatchUnnamedException, catchStatement.Tokens[0]);

            var content = tokens.Split(Token.LineBreak);
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
                    default:
                        break;
                }
                return expressionUnits[0];
            } else {
                result.AddFatal(SyntaxError.EmptyExpression, Span.Default);
                return new Literal("none");
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
                    if (tokens.Count == 0) return new Matrix2DExpression() { Bracket = BracketType.Brakcet };
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
                                        return new Matrix2DExpression() { Bracket = BracketType.Brakcet };
                                    } }

                                rowCount++;
                                tempCol = 1;
                            }

                        } else exprBlocks.Add(item);
                    }

                    if (columnCount == 0 && exprBlocks.Count == 0) return new Matrix2DExpression() { Bracket = BracketType.Brakcet };
                    if (columnCount == 0 ) columnCount = tempCol;
                    if (exprBlocks.Count > 0) exprs.Add(exprBlocks);

                    if(rowCount*columnCount != exprs.Count) {
                        result.AddFatal(SyntaxError.InternalMatrixAssignment, returns.Tokens[0]);
                        return new Matrix2DExpression() { Bracket = BracketType.Brakcet };
                    }

                    Matrix2DExpression matrix = new Matrix2DExpression();
                    matrix.Bracket = BracketType.Brakcet;
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
                IExpression? left = counter == 0 ? null : expressions[counter - 1];
                IExpression? right = counter == expressions.Count - 1 ? null : expressions[counter + 1];
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
                IExpression? left = counter == 0 ? null : expressions[counter - 1];
                IExpression? right = counter == expressions.Count - 1 ? null : expressions[counter + 1];
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
                IExpression? left = counter == 0 ? null : expressions[counter - 1];
                IExpression? right = counter == expressions.Count - 1 ? null : expressions[counter + 1];
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
        public static void AddIfNotNull<T>(this IList<T> exprs, T? item) where T : class
        {
            if (item == null) return;
            else exprs.Add(item);
        }

        public static void AddRangeIfNotNull<T>(this IList<T> exprs, IList<T?>? item) where T : class
        {
            if (item == null) return;
            foreach (var range in item) {
                exprs.AddIfNotNull(range);
            }
        }

        public static void AddRangeIfNotNull<T>(this IList<T> exprs, T?[]? item) where T : class
        {
            if (item == null) return;
            foreach (var range in item) {
                exprs.AddIfNotNull(range);
            }
        }

        public static void InsertIfNotNull<T>(this IList<T> exprs, int position, T? item) where T : class
        {
            if (item == null) return;
            exprs.Insert(position, item);
        }
    }
}
