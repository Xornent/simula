using System;
using System.Collections.Generic;
using System.Text;
using Simula.Scripting.Token;

namespace Simula.Scripting.Syntax {

    public enum DefinitionType {
        Constant,
        Function,
        Class
    }

    public enum Visibility {
        Expose,
        Hidden
    }

    public class Parameter {
        public EvaluationStatement? Type { get; set; }
        public Token.Token? Name { get; set; }
    }

    public class DefinitionBlock : BlockStatement {
        public DefinitionType Type;
        public Visibility Visibility = Visibility.Hidden;

        public Token.Token? FunctionName;
        public List<Parameter> FunctionParameters = new List<Parameter>();
        public EvaluationStatement? FunctionAlias;

        public Token.Token? ClassName;
        public List<Parameter> ClassParameters = new List<Parameter>();
        public EvaluationStatement? ClassInheritage;
        public EvaluationStatement? ClassAlias;

        public Token.Token? ConstantName;
        public EvaluationStatement? ConstantValue;

        public new void Parse(TokenCollection collection) {
                if (collection[0] == "expose") {
                    this.Visibility = Visibility.Expose;
                    switch ((string)collection[1]) {
                        case "def":
                            switch ((string)collection[2]) {
                                case "func":
                                    this.Type = DefinitionType.Function;
                                    collection.RemoveRange(0, 3);
                                    break;
                                case "class":
                                    this.Type = DefinitionType.Class;
                                    collection.RemoveRange(0, 3);
                                    break;
                                case "var":
                                    this.Type = DefinitionType.Constant;
                                    collection.RemoveRange(0, 3);
                                    break;
                                default:
                                    collection[2].Error = new TokenizerException("SS0013");
                                    return;
                            }
                            break;
                        case "func":
                            this.Type = DefinitionType.Function;
                            collection.RemoveRange(0, 2);
                            break;
                        case "class":
                            this.Type = DefinitionType.Class;
                            collection.RemoveRange(0, 2);
                            break;
                        case "var":
                            this.Type = DefinitionType.Constant;
                            collection.RemoveRange(0, 2);
                            break;
                        default:
                            collection[1].Error = new TokenizerException("SS0012");
                            return;
                    }
                } else if (collection[0] == "hidden") {
                    this.Visibility = Visibility.Hidden;
                    switch ((string)collection[1]) {
                        case "def":
                            switch ((string)collection[2]) {
                                case "func":
                                    this.Type = DefinitionType.Function;
                                    collection.RemoveRange(0, 3);
                                    break;
                                case "class":
                                    this.Type = DefinitionType.Class;
                                    collection.RemoveRange(0, 3);
                                    break;
                                case "var":
                                    this.Type = DefinitionType.Constant;
                                    collection.RemoveRange(0, 3);
                                    break;
                                default:
                                    collection[2].Error = new TokenizerException("SS0013");
                                    return;
                            }
                            break;
                        case "func":
                            this.Type = DefinitionType.Function;
                            collection.RemoveRange(0, 2);
                            break;
                        case "class":
                            this.Type = DefinitionType.Class;
                            collection.RemoveRange(0, 2);
                            break;
                        case "var":
                            this.Type = DefinitionType.Constant;
                            collection.RemoveRange(0, 2);
                            break;
                        default:
                            collection[1].Error = new TokenizerException("SS0012");
                            return;
                    }
                } else if (collection[0] == "def") {
                    this.Visibility = Visibility.Hidden;
                    switch ((string)collection[1]) {
                        case "func":
                            this.Type = DefinitionType.Function;
                            collection.RemoveRange(0, 2);
                            break;
                        case "class":
                            this.Type = DefinitionType.Class;
                            collection.RemoveRange(0, 2);
                            break;
                        case "var":
                            this.Type = DefinitionType.Constant;
                            collection.RemoveRange(0, 2);
                            break;
                        default:
                            collection[1].Error = new TokenizerException("SS0013");
                            return;
                    }
                }

                // 在此处的 collection 值应为序列

                // class_name<[class parameters ...]> [: inherit] [= defined<...>]
                // func_name([function parameters]) [= defined(...)]
                // const_name = defined

                switch (this.Type) {
                    case DefinitionType.Constant:
                        if (collection.Contains(new Token.Token("="))) {
                            var l = collection.Split(new Token.Token("="));
                            if (l.Count == 2) {
                                ConstantName = l[0][0];

                                EvaluationStatement eval = new EvaluationStatement();
                                eval.Parse(l[1]);
                                ConstantValue = eval;
                            } else collection[0].Error = new TokenizerException("SS0002");
                        } else collection[0].Error = new TokenizerException("SS0014");
                        break;
                    case DefinitionType.Function:
                        if (collection.Contains(new Token.Token("="))) {
                            var l = collection.Split(new Token.Token("="));
                            if (l.Count == 2) {
                                ParseAbsoluteFunctionDefinition(l[0]);
                                EvaluationStatement eval = new EvaluationStatement();
                                eval.Parse(l[1]);
                                this.FunctionAlias = eval;
                            } else collection[0].Error = new TokenizerException("SS0002");
                        } else {
                            ParseAbsoluteFunctionDefinition(collection);
                        }
                        break;
                    case DefinitionType.Class:
                        if (collection.Contains(new Token.Token("="))) {
                            var l = collection.Split(new Token.Token("="));
                            if (l.Count == 2) {
                                ParseClassDefinition(l[0]);
                                EvaluationStatement eval = new EvaluationStatement();
                                eval.Parse(l[1]);
                                this.ClassAlias = eval;
                            } else collection[0].Error = new TokenizerException("SS0002");
                        } else {
                            ParseClassDefinition(collection);
                        }
                        break;
                }
        }

        private void ParseClassDefinition(TokenCollection collection) {
            if (collection.Contains(new Token.Token(":"))) {
                var l = collection.Split(new Token.Token(":"));
                if (l.Count == 2) {
                    ParseAbsoluteClassName(l[0]);
                    EvaluationStatement eval = new EvaluationStatement();
                    eval.Parse(l[1]);
                    this.ClassInheritage = eval;
                } 
                else collection[0].Error = new TokenizerException("SS0015");
            } else {
                ParseAbsoluteClassName(collection);
            }
        }

        private void ParseAbsoluteClassName(TokenCollection collection) {
            this.ClassName = collection[0];
            if (collection[1] == "{}") { } else {
                TokenCollection tk = new TokenCollection();
                for (int i = 2; i < collection.Count; i++) {
                    if (collection[i] == "}") break;
                    else tk.Add(collection[i]);
                }

                var funcParam = tk.Split(new Token.Token(","));
                this.ClassParameters.Clear();
                foreach (var item in funcParam) {
                    Parameter par = new Parameter();
                    par.Name = item.Last();
                    item.RemoveLast();
                    EvaluationStatement e = new EvaluationStatement();
                    e.Parse(item);
                    par.Type = e;
                    this.ClassParameters.Add(par);
                }
            }
        }

        private void ParseAbsoluteFunctionDefinition(TokenCollection collection) {
            this.FunctionName = collection[0];
            if (collection[1] == "(" && collection[2] == ")") { } else {
                TokenCollection tk = new TokenCollection();
                for (int i = 2; i < collection.Count; i++) {
                    if (collection[i] == ")") break;
                    else tk.Add(collection[i]);
                }

                var funcParam = tk.Split(new Token.Token(","));
                this.FunctionParameters.Clear();
                foreach (var item in funcParam) {
                    Parameter par = new Parameter();
                    par.Name = item.Last();
                    item.RemoveLast();
                    EvaluationStatement e = new EvaluationStatement();
                    e.Parse(item);
                    par.Type = e;
                    this.FunctionParameters.Add(par);
                }
            }
        }
    }
}
