using System;
using System.Collections.Generic;
using System.Text;
using Simula.Scripting.Token;

namespace Simula.Scripting.Syntax {

    public class EvaluationStatement : Statement {

        public TokenCollection RawEvaluateToken = new TokenCollection();
        public List<OperatorStatement> EvaluateOperators = new List<OperatorStatement>();

        public dynamic Result(Compilation.RuntimeContext ctx) {
            if (EvaluateOperators.Count > 1) return Type.Global.Null;
            var operation = EvaluateOperators[0];

            return operation.Operate(ctx);
        }

        string EvalString = "";
        public new void Parse(TokenCollection collection) {
            EvalString = collection.ToString();

            this.RawEvaluateToken = collection;
            this.EvaluateOperators.Clear();
            foreach (var item in RawEvaluateToken) {
                EvaluateOperators.Add(new SelfOperation() { Self = item });
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

        public void Parse() {

            while (ParseBinocularOperator(".", EvaluateOperators)) { }

            // 关于特化类的说明

            // 在语言的最初设计中, 我们期望使用 <> 来指定特化类的参数, 但是发现此参数会与多个语言
            // 功能发生混淆, 于是, 我们使用 {} 来指定特化类参数.

            // 现在我们具体的说明冲突, 以期待以后解决的可能性:

            // 1. 特化类参数语句不能在函数调用之后解析, 因为特化类运算返回一个函数(初始化函数).
            // 2. 特化类不能在成员运算符之前解析, 因为引用关系

            // 因此,特化类参数的解析时间是固定的. 最合理的解析方法是 解析() > 解析<> > 替换<> > 替换()

            // 3. 考虑下面的例子,  

            //    if a < b || c > d

            //    我们完全有理由解析成 if a<b||c> d, 其中 b||c 传入 a<> 的特化参数中, 又因为 d 不应该
            //    出现在此处抛出语法错误. 唯一正确的写法是 if (a < b) || (c > d)

            // 综上所述, 我们要么直接指定 {} 作为特化类表示, 要么只能在许多判断语句中增加意义奇怪的括号.
            // 我们更愿意以 {} 作为解决方案. 不过, 我们将第二种方法保留. 使用时在本文件头添加

            //     #define optional

            // 即可.

#if optional
            while (ParseSealedOperator("(", ")", EvaluateOperators)) { }
            while (ParseSealedOperator("<", ">", EvaluateOperators)) { }
            while (ReplaceAngleBracketToClassType()) { }
            while (ReplaceSmallBracketToFunctionCall()) { }
#else
            while (ParseSealedOperator("{", "}", EvaluateOperators)) { }
            while (ReplaceAngleBracketToClassType()) { }

            while (ParseSealedOperator("(", ")", EvaluateOperators)) { }
            while (ReplaceSmallBracketToFunctionCall()) { }
#endif
            while (ParseSealedOperator("[", "]", EvaluateOperators)) { }
            while (ReplaceSmallBracketToIndex()) { }

            while (ParseBinocularOperator("**", EvaluateOperators)) { }
            while (ParseBinocularOperator("*", EvaluateOperators)) { }
            while (ParseBinocularOperator("/", EvaluateOperators)) { }
            while (ParseBinocularOperator("%", EvaluateOperators)) { }
            while (ParseBinocularOperator("+", EvaluateOperators)) { }
            while (ParseBinocularOperator("-", EvaluateOperators)) { }
            while (ParseBinocularOperator("<<", EvaluateOperators)) { }
            while (ParseBinocularOperator(">>", EvaluateOperators)) { }
            while (ParseBinocularOperator("<", EvaluateOperators)) { }
            while (ParseBinocularOperator(">", EvaluateOperators)) { }
            while (ParseBinocularOperator("<=", EvaluateOperators)) { }
            while (ParseBinocularOperator(">=", EvaluateOperators)) { }
            while (ParseBinocularOperator("==", EvaluateOperators)) { }
            while (ParseBinocularOperator("!=", EvaluateOperators)) { }
            while (ParseBinocularOperator("||", EvaluateOperators)) { }
            while (ParseBinocularOperator("&&", EvaluateOperators)) { }
            while (ParseBinocularOperator("|", EvaluateOperators)) { }
            while (ParseBinocularOperator("&", EvaluateOperators)) { }
            while (ParseBinocularOperator("^", EvaluateOperators)) { }
        }

        bool ReplaceSmallBracketToFunctionCall() {
            int length = EvaluateOperators.Count;
            for (int c = 0; c < length;c++) {
                var item = EvaluateOperators[c];
                if (item is SmallBracketOperation) {
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
                        EvaluateOperators.Add(func);
                        return true;
                    }
                }
            }
            return false;
        }

        bool ReplaceAngleBracketToClassType() {
            int length = EvaluateOperators.Count;
            for (int c = 0; c < length; c++) {
                var item = EvaluateOperators[c];
                if (item is AngleBracketOperation) {
                    if (c != 0) {
                        OperatorStatement op = EvaluateOperators[c - 1];
                        if (op is SelfOperation) {
                            var self = op as SelfOperation;
                            if (self.Self.IsValidSymbolBeginning()) { continue; }
                        }

                        ClassTypeOperation func = new ClassTypeOperation();
                        func.Left = EvaluateOperators[c - 1];
                        func.Right = EvaluateOperators[c];
                        EvaluateOperators.RemoveRange(c - 1, 2);
                        EvaluateOperators.Add(func);
                        return true;
                    }
                }
            }
            return false;
        }

        bool ReplaceSmallBracketToIndex() {
            int length = EvaluateOperators.Count;
            for (int c = 0; c < length; c++) {
                var item = EvaluateOperators[c];
                if (item is SmallBracketOperation) {
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
                        EvaluateOperators.Add(func);
                        return true;
                    }
                }
            }
            return false;
        }

        bool ParseBinocularOperator(string operation, List<OperatorStatement> token) {
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
                    } else if(self.Self == ")" ||
                        self.Self == "}" ||
                        self.Self == "]") {
                        cascader.Pop();
                        break;
                    }

                    if(cascader.Count==0) {
                        if (self.Self.IsValidSymbolBeginning()) {
                            if(self.Self == operation) {
                                if(c == 0 || token.Count < c + 2) {
                                    self.Self.Error = new TokenizerException("SS0017");
                                    continue;
                                }

                                if (token[c - 1] is SelfOperation) {
                                    var s = token[c - 1] as SelfOperation;
                                    if (s.Self.IsValidSymbolBeginning())
                                        self.Self.Error = new TokenizerException("SS0018");
                                }
                                if (token[c + 1] is SelfOperation) {
                                    var s = token[c + 1] as SelfOperation;
                                    if (s.Self.IsValidSymbolBeginning())
                                        self.Self.Error = new TokenizerException("SS0018");
                                }

                                switch (operation) {
                                    case ".":
                                        OperatorStatement op = new MemberOperation() { Left = token[c - 1], Right = token[c + 1] };
                                        token.RemoveRange(c - 1, 3);
                                        token.Insert(c - 1, op);
                                        return true;
                                    case "**":
                                        op = new PowOperation() { Left = token[c - 1], Right = token[c + 1] };
                                        token.RemoveRange(c - 1, 3);
                                        token.Insert(c - 1, op);
                                        return true;
                                    case "*":
                                        op = new MultiplyOperation() { Left = token[c - 1], Right = token[c + 1] };
                                        token.RemoveRange(c - 1, 3);
                                        token.Insert(c - 1, op);
                                        return true;
                                    case "/":
                                        op = new DivideOperation() { Left = token[c - 1], Right = token[c + 1] };
                                        token.RemoveRange(c - 1, 3);
                                        token.Insert(c - 1, op);
                                        return true;
                                    case "%":
                                        op = new ModOperation() { Left = token[c - 1], Right = token[c + 1] };
                                        token.RemoveRange(c - 1, 3);
                                        token.Insert(c - 1, op);
                                        return true;
                                    case "+":
                                        op = new AddOperation() { Left = token[c - 1], Right = token[c + 1] };
                                        token.RemoveRange(c - 1, 3);
                                        token.Insert(c - 1, op);
                                        return true;
                                    case "-":
                                        op = new MinusOperation() { Left = token[c - 1], Right = token[c + 1] };
                                        token.RemoveRange(c - 1, 3);
                                        token.Insert(c - 1, op);
                                        return true;
                                    case "<<":
                                        op = new LeftShiftOperation() { Left = token[c - 1], Right = token[c + 1] };
                                        token.RemoveRange(c - 1, 3);
                                        token.Insert(c - 1, op);
                                        return true;
                                    case ">>":
                                        op = new RightShiftOperation() { Left = token[c - 1], Right = token[c + 1] };
                                        token.RemoveRange(c - 1, 3);
                                        token.Insert(c - 1, op);
                                        return true;
                                    case "<":
                                        op = new LessThanOperation() { Left = token[c - 1], Right = token[c + 1] };
                                        token.RemoveRange(c - 1, 3);
                                        token.Insert(c - 1, op);
                                        return true;
                                    case ">":
                                        op = new MoreThanOperation() { Left = token[c - 1], Right = token[c + 1] };
                                        token.RemoveRange(c - 1, 3);
                                        token.Insert(c - 1, op);
                                        return true;
                                    case "<=":
                                        op = new NoMoreThanOperation() { Left = token[c - 1], Right = token[c + 1] };
                                        token.RemoveRange(c - 1, 3);
                                        token.Insert(c - 1, op);
                                        return true;
                                    case ">=":
                                        op = new NoLessThanOperation() { Left = token[c - 1], Right = token[c + 1] };
                                        token.RemoveRange(c - 1, 3);
                                        token.Insert(c - 1, op);
                                        return true;
                                    case "==":
                                        op = new EqualOperation() { Left = token[c - 1], Right = token[c + 1] };
                                        token.RemoveRange(c - 1, 3);
                                        token.Insert(c - 1, op);
                                        return true;
                                    case "!=":
                                        op = new NotEqualOperation() { Left = token[c - 1], Right = token[c + 1] };
                                        token.RemoveRange(c - 1, 3);
                                        token.Insert(c - 1, op);
                                        return true;
                                    case "||":
                                        op = new OrOperation() { Left = token[c - 1], Right = token[c + 1] };
                                        token.RemoveRange(c - 1, 3);
                                        token.Insert(c - 1, op);
                                        return true;
                                    case "&&":
                                        op = new AndOperation() { Left = token[c - 1], Right = token[c + 1] };
                                        token.RemoveRange(c - 1, 3);
                                        token.Insert(c - 1, op);
                                        return true;
                                    case "|":
                                        op = new BitOrOperation() { Left = token[c - 1], Right = token[c + 1] };
                                        token.RemoveRange(c - 1, 3);
                                        token.Insert(c - 1, op);
                                        return true;
                                    case "&":
                                        op = new BitAndOperation() { Left = token[c - 1], Right = token[c + 1] };
                                        token.RemoveRange(c - 1, 3);
                                        token.Insert(c - 1, op);
                                        return true;
                                    case "^":
                                        op = new BitNotOperation() { Left = token[c - 1], Right = token[c + 1] };
                                        token.RemoveRange(c - 1, 3);
                                        token.Insert(c - 1, op);
                                        return true;
                                    default:
                                        self.Self.Error = new TokenizerException("SS0001");
                                        break;
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }

        bool ParseSealedOperator(string left, string right, List<OperatorStatement> token) {
            int innerBrackets = 0;
            bool hasBracket = false;
            List<OperatorStatement> children = new List<OperatorStatement>();
            for(int i = 0; i<token.Count;i++) {
                if(token[i] is SelfOperation) {
                    var self = token[i] as SelfOperation;
                    if(self.Self == left) {
                        if(hasBracket == false) {
                            hasBracket = true;
                            innerBrackets = 0;
                            token.RemoveAt(i);
                            i--;
                            continue;
                        } else {
                            innerBrackets++;
                        }
                    }

                    if(self.Self == right) {
                        if(hasBracket == true) {
                            if (innerBrackets > 0) innerBrackets--;
                            else {
                                if (left == "(") {
                                    SmallBracketOperation sm = new SmallBracketOperation();
                                    sm.EvaluateOperators = children;
                                    sm.Parse();
                                    token.RemoveAt(i);
                                    token.Insert(i, sm);
#if optional
                                } else if (left == "<") {
#else
                                } else if (left == "{") {
#endif
                                    AngleBracketOperation sm = new AngleBracketOperation();
                                    sm.EvaluateOperators = children;
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
