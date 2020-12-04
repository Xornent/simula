using Simula.Scripting.Token;
using System.Collections.Generic;

namespace Simula.Scripting.Syntax
{
    public enum DefinitionType
    {
        Constant,
        Function,
        Class
    }

    public enum Visibility
    {
        Expose,
        Hidden
    }

    public class Parameter
    {
        public EvaluationStatement? Type { get; set; }
        public Token.Token? Name { get; set; }
    }

    public class DefinitionBlock : BlockStatement
    {
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

        public new void Parse(TokenCollection collection)
        {
            Token.Token[] store = new Token.Token[collection.Count];
            collection.CopyTo(store);

            if (collection[0] == "expose") {
                if (collection.Count == 1) {
                    collection[0].Error = new TokenizerException("SS0012");
                    return;
                }
                Visibility = Visibility.Expose;
                switch (collection[1]) {
                    case "def":
                        if (collection.Count == 2) {
                            collection[0].Error = new TokenizerException("SS0013");
                            return;
                        }
                        switch (collection[2]) {
                            case "func":
                                Type = DefinitionType.Function;
                                collection.RemoveRange(0, 3);
                                break;
                            case "class":
                                Type = DefinitionType.Class;
                                collection.RemoveRange(0, 3);
                                break;
                            case "var":
                                Type = DefinitionType.Constant;
                                collection.RemoveRange(0, 3);
                                break;
                            default:
                                collection[2].Error = new TokenizerException("SS0013");
                                return;
                        }
                        break;
                    case "func":
                        Type = DefinitionType.Function;
                        collection.RemoveRange(0, 2);
                        break;
                    case "class":
                        Type = DefinitionType.Class;
                        collection.RemoveRange(0, 2);
                        break;
                    case "var":
                        Type = DefinitionType.Constant;
                        collection.RemoveRange(0, 2);
                        break;
                    default:
                        collection[1].Error = new TokenizerException("SS0012");
                        return;
                }
            } else if (collection[0] == "hidden") {
                Visibility = Visibility.Hidden;
                if (collection.Count == 1) {
                    collection[0].Error = new TokenizerException("SS0012");
                    return;
                }
                switch (collection[1]) {
                    case "def":
                        if (collection.Count == 2) {
                            collection[0].Error = new TokenizerException("SS0013");
                            return;
                        }
                        switch (collection[2]) {
                            case "func":
                                Type = DefinitionType.Function;
                                collection.RemoveRange(0, 3);
                                break;
                            case "class":
                                Type = DefinitionType.Class;
                                collection.RemoveRange(0, 3);
                                break;
                            case "var":
                                Type = DefinitionType.Constant;
                                collection.RemoveRange(0, 3);
                                break;
                            default:
                                collection[2].Error = new TokenizerException("SS0013");
                                return;
                        }
                        break;
                    case "func":
                        Type = DefinitionType.Function;
                        collection.RemoveRange(0, 2);
                        break;
                    case "class":
                        Type = DefinitionType.Class;
                        collection.RemoveRange(0, 2);
                        break;
                    case "var":
                        Type = DefinitionType.Constant;
                        collection.RemoveRange(0, 2);
                        break;
                    default:
                        collection[1].Error = new TokenizerException("SS0012");
                        return;
                }
            } else if (collection[0] == "def") {
                Visibility = Visibility.Hidden;
                if (collection.Count == 1) {
                    collection[0].Error = new TokenizerException("SS0013");
                    return;
                }
                switch (collection[1]) {
                    case "func":
                        Type = DefinitionType.Function;
                        collection.RemoveRange(0, 2);
                        break;
                    case "class":
                        Type = DefinitionType.Class;
                        collection.RemoveRange(0, 2);
                        break;
                    case "var":
                        Type = DefinitionType.Constant;
                        collection.RemoveRange(0, 2);
                        break;
                    default:
                        collection[1].Error = new TokenizerException("SS0013");
                        return;
                }
            }

            if (collection.Count == 0) {
                store[store.Length - 1].Error = new TokenizerException("ss0021");
                return;
            }

            // 在此处的 collection 值应为序列

            // class_name<[class parameters ...]> [: inherit] [= defined<...>]
            // func_name([function parameters]) [= defined(...)]
            // const_name = defined

            switch (Type) {
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
                            FunctionAlias = eval;
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
                            ClassAlias = eval;
                        } else collection[0].Error = new TokenizerException("SS0002");
                    } else {
                        ParseClassDefinition(collection);
                    }
                    break;
            }
        }

        private void ParseClassDefinition(TokenCollection collection)
        {
            if (collection.Contains(new Token.Token(":"))) {
                var l = collection.Split(new Token.Token(":"));
                if (l.Count == 2) {
                    ParseAbsoluteClassName(l[0]);
                    EvaluationStatement eval = new EvaluationStatement();
                    eval.Parse(l[1]);
                    ClassInheritage = eval;
                } else collection[0].Error = new TokenizerException("SS0015");
            } else {
                ParseAbsoluteClassName(collection);
            }
        }

        private void ParseAbsoluteClassName(TokenCollection collection)
        {
            ClassName = collection[0];
        }

        private void ParseAbsoluteFunctionDefinition(TokenCollection collection)
        {
            FunctionName = collection[0];
            if (collection.Count < 3) {
                collection[0].Error = new TokenizerException("ss0023");
                return;
            }

            try {
                if (collection[1] == "(" && collection[2] == ")") { } else {
                    TokenCollection tk = new TokenCollection();
                    for (int i = 2; i < collection.Count; i++) {
                        if (collection[i] == ")") break;
                        else tk.Add(collection[i]);
                    }

                    var funcParam = tk.Split(new Token.Token(","));
                    FunctionParameters.Clear();
                    foreach (var item in funcParam) {
                        Parameter par = new Parameter();
                        par.Name = item.Last();
                        item.RemoveLast();
                        EvaluationStatement e = new EvaluationStatement();
                        e.Parse(item);
                        par.Type = e;
                        FunctionParameters.Add(par);
                    }
                }
            } catch {
                if (collection.Count < 3) {
                    collection[collection.Count - 1].Error = new TokenizerException("ss0024");
                    return;
                }
            }
        }
    }
}
