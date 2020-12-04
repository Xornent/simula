﻿using Simula.Scripting.Contexts;
using Simula.Scripting.Token;
using System.Collections.Generic;

namespace Simula.Scripting.Syntax
{
    public class EvaluationStatement : Statement
    {
        public TokenCollection RawEvaluateToken = new TokenCollection();
        public List<OperatorStatement> EvaluateOperators = new List<OperatorStatement>();

        public override Execution Execute(DynamicRuntime ctx)
        {
            if (EvaluateOperators.Count > 1) return new Execution();
            var operation = EvaluateOperators[0];

            return operation.Operate(ctx);
        }

        private string EvalString = "";
        public override void Parse(TokenCollection collection)
        {
            EvalString = collection.ToString();

            RawEvaluateToken = collection;
            EvaluateOperators.Clear();
            foreach (var item in RawEvaluateToken) {
                EvaluateOperators.Add(new SelfOperation() { Self = item, RawEvaluateToken = new TokenCollection() { item } });
            }

            // 运算符次序

            // 1. 类型成员运算符(.)
            // 2. 子类型运算符({...}), 子函数运算符((...))
            // 3. 括号表达式运算符(()), 索引表达式运算符([])
            // 4. 指数运算符(**)
            // 5. 乘运算符(*), 除运算符(/)
            // 6. 取余运算符(%)
            // 7. 加运算符(+), 减运算符(-)
            // 8. 位左移运算符(<<), 位右移运算符(>>)
            // 9. 比较运算符(<, >, <=, >=, ==, !=)
            // 10. 布尔逻辑运算符 (||, &&)
            // 11. 位逻辑运算符 (|, &, ^)

            Parse();
        }

        public void Parse()
        {
            while (ParseBinocularOperator(".", EvaluateOperators)) { }

            while (ParseSealedOperator("{", "}", EvaluateOperators)) { }

            while (ParseSealedOperator("(", ")", EvaluateOperators)) { }
            while (ParseSealedOperator("[", "]", EvaluateOperators)) { }

            while (ParseBinocularOperator(".", EvaluateOperators) ||
                   ReplaceSmallBracketToFunctionCall() ||
                   ParseBinocularOperator(".", EvaluateOperators) ||
                   ReplaceSquareBracketToIndex() ||
                   ParseBinocularOperator(".", EvaluateOperators)) { }

            while (ParseBinocularOperatorForced(".", EvaluateOperators)) { }

            foreach (var op in DynamicRuntime.Registry) {
                while(ParseBinocularOperatorForced(op.Value.Symbol, EvaluateOperators));
            }
        }

        private bool ReplaceSmallBracketToFunctionCall()
        {
            int length = EvaluateOperators.Count;
            for (int c = 0; c < length; c++) {
                var item = EvaluateOperators[c];
                if (item is ParenthesisOperation) {
                    if (c != 0) {
                        OperatorStatement op = EvaluateOperators[c - 1];
                        if (op is SelfOperation) {
                            var self = (op as SelfOperation);
                            if (self.Self.IsValidSymbolBeginning()) { continue; }
                        }

                        FunctionCallOperation func = new FunctionCallOperation();
                        func.Left = EvaluateOperators[c - 1];
                        func.Right = EvaluateOperators[c];
                        EvaluateOperators.RemoveRange(c - 1, 2);
                        EvaluateOperators.Insert(c - 1, func);
                        return true;
                    }
                }
            }
            return false;
        }

        private bool ReplaceSquareBracketToIndex()
        {
            int length = EvaluateOperators.Count;
            for (int c = 0; c < length; c++) {
                var item = EvaluateOperators[c];
                if (item is BracketOperation) {
                    if (c != 0) {
                        OperatorStatement op = EvaluateOperators[c - 1];
                        if (op is SelfOperation) {
                            var self = op as SelfOperation;
                            if (self.Self.IsValidSymbolBeginning()) { continue; }
                        }

                        IndexOperation func = new IndexOperation();
                        func.Left = EvaluateOperators[c - 1];
                        func.Right = EvaluateOperators[c];
                        EvaluateOperators.RemoveRange(c - 1, 2);
                        EvaluateOperators.Insert(c - 1, func);
                        return true;
                    }
                }
            }
            return false;
        }

        private bool ParseBinocularOperator(string operation, List<OperatorStatement> token)
        {
            Stack<Token.Token> cascader = new Stack<Token.Token>();
            int c = -1;
            foreach (var item in token) {
                c++;
                if (item is SelfOperation) {
                    var self = item as SelfOperation;
                    if (self.Self == "(" ||
                        self.Self == "{" ||
                        self.Self == "[") {
                        cascader.Push(self.Self);
                        break;
                    } else if (self.Self == ")" ||
                        self.Self == "}" ||
                        self.Self == "]") {
                        cascader.Pop();
                        break;
                    }

                    if (cascader.Count == 0) {
                        if (self.Self.IsValidSymbolBeginning()) {
                            if (self.Self == operation) {
                                if (c == 0 || token.Count < c + 2) {
                                    self.Self.Error = new TokenizerException("SS0017");
                                    continue;
                                }

                                if (token[c - 1] is SelfOperation) {
                                    var s = token[c - 1] as SelfOperation;
                                    if (s.Self.IsValidSymbolBeginning()) continue;
                                }

                                if (token[c + 1] is SelfOperation) {
                                    var s = token[c + 1] as SelfOperation;
                                    if (s.Self.IsValidSymbolBeginning()) continue;
                                }

                                if (token[c - 1] is ParenthesisOperation) continue;
                                if (token[c - 1] is BracketOperation) continue;
                                if (token[c - 1] is BraceOperation) continue;

                                OperatorStatement? op = null;
                                switch (operation) {
                                    case "final":
                                        op.RawEvaluateToken.AddRange(token[c - 1].RawEvaluateToken);
                                        op.RawEvaluateToken.AddRange(token[c].RawEvaluateToken);
                                        op.RawEvaluateToken.AddRange(token[c + 1].RawEvaluateToken);
                                        token.RemoveRange(c - 1, 3);
                                        token.Insert(c - 1, op ?? new OperatorStatement());
                                        return true;
                                    default:
                                        op = new BinaryOperation();
                                        op.Left = token[c-1];
                                        op.Right = token[c+1];
                                        op.Operator = new Operator(operation);
                                        break;
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }

        private bool ParseBinocularOperatorForced(string operation, List<OperatorStatement> token)
        {
            Stack<Token.Token> cascader = new Stack<Token.Token>();
            int c = -1;
            foreach (var item in token) {
                c++;
                if (item is SelfOperation) {
                    var self = item as SelfOperation;
                    if (self.Self == "(" ||
                        self.Self == "{" ||
                        self.Self == "[") {
                        cascader.Push(self.Self);
                        break;
                    } else if (self.Self == ")" ||
                        self.Self == "}" ||
                        self.Self == "]") {
                        cascader.Pop();
                        break;
                    }

                    if (cascader.Count == 0) {
                        if (self.Self.IsValidSymbolBeginning()) {
                            if (self.Self == operation) {
                                if (c == 0 || token.Count < c + 2) {
                                    self.Self.Error = new TokenizerException("SS0017");
                                    continue;
                                }

                                if (token[c - 1] is SelfOperation) {
                                    var s = token[c - 1] as SelfOperation;
                                    if (s.Self.IsValidSymbolBeginning()) continue;
                                }
                                if (token[c + 1] is SelfOperation) {
                                    var s = token[c + 1] as SelfOperation;
                                    if (s.Self.IsValidSymbolBeginning()) continue;
                                }

                                OperatorStatement? op = null;
                                switch (operation) {
                                    case "final":
                                        op.RawEvaluateToken.AddRange(token[c - 1].RawEvaluateToken);
                                        op.RawEvaluateToken.AddRange(token[c].RawEvaluateToken);
                                        op.RawEvaluateToken.AddRange(token[c + 1].RawEvaluateToken);
                                        token.RemoveRange(c - 1, 3);
                                        token.Insert(c - 1, op ?? new OperatorStatement());
                                        return true;
                                    default:
                                        op = new BinaryOperation();
                                        op.Left = token[c-1];
                                        op.Right = token[c+1];
                                        op.Operator = new Operator(operation);
                                        goto case "final";
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }

        private bool ParseSealedOperator(string left, string right, List<OperatorStatement> token)
        {
            int innerBrackets = 0;
            bool hasBracket = false;
            List<OperatorStatement> children = new List<OperatorStatement>();
            for (int i = 0; i < token.Count; i++) {
                if (token[i] is SelfOperation) {
                    var self = token[i] as SelfOperation;
                    if (self.Self == left) {
                        if (hasBracket == false) {
                            hasBracket = true;
                            innerBrackets = 0;
                            token.RemoveAt(i);
                            i--;
                            continue;
                        } else {
                            innerBrackets++;
                        }
                    }

                    if (self.Self == right) {
                        if (hasBracket == true) {
                            if (innerBrackets > 0) innerBrackets--;
                            else {
                                if (left == "(") {
                                    ParenthesisOperation sm = new ParenthesisOperation();
                                    sm.EvaluateOperators = children;
                                    foreach (var item in children) {
                                        sm.RawEvaluateToken.AddRange(item.RawEvaluateToken);
                                    }
                                    sm.Parse();
                                    token.RemoveAt(i);
                                    token.Insert(i, sm);
#if optional
                                } else if (left == "<") {
#else
                                } else if (left == "[") {
                                    BracketOperation sm = new BracketOperation();
                                    sm.EvaluateOperators = children;
                                    foreach (var item in children) {
                                        sm.RawEvaluateToken.AddRange(item.RawEvaluateToken);
                                    }
                                    sm.Parse();
                                    token.RemoveAt(i);
                                    token.Insert(i, sm);
                                } else if (left == "{") {
                                    BraceOperation sm = new BraceOperation();
                                    sm.EvaluateOperators = children;
                                    foreach (var item in children) {
                                        sm.RawEvaluateToken.AddRange(item.RawEvaluateToken);
                                    }
                                    sm.Parse();
                                    token.RemoveAt(i);
                                    token.Insert(i, sm);
                                }
                                return true;
                            }
                        }
                    }
                }

                if (hasBracket) {
                    children.Add(token[i]);
                    token.RemoveAt(i);
                    i--;
                }
            }
            return false;
        }
    }
}
#endif