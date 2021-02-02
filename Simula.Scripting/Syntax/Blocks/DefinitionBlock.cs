using Simula.Scripting.Build;
using Simula.Scripting.Token;
using System.Collections.Generic;
using System.Linq;

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
        public bool IsReadonly = false;

        private string Cascader = "";

        public bool IsStatic = true;
        public Token.Token? FunctionName;
        public List<Parameter> FunctionParameters = new List<Parameter>();
        public EvaluationStatement? FunctionAlias;

        public Token.Token? ClassName;
        public List<Parameter> ClassParameters = new List<Parameter>();
        public List<EvaluationStatement?> ClassInheritages = new List<EvaluationStatement?>();
        public List<EvaluationStatement?> ClassDerivations = new List<EvaluationStatement?>();
        public EvaluationStatement? ClassAlias;

        public Token.Token? ConstantName;
        public EvaluationStatement? ConstantValue;
        public bool ExplicitGet = false;
        public bool ExplicitSet = false;
        public BlockStatement? GetBlock;
        public BlockStatement? SetBlock;

        public CommentBlock? Documentation;

        public new void Parse(TokenCollection collection)
        {
            RawToken.AddRange(collection);
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

            // class_name [: a, b, c ...] [derives a, b, c ...]  [= defined<...>]
            // func_name([function parameters]) [= defined(...)]
            // const_name = defined

            switch (Type) {
                case DefinitionType.Constant:
                    if (collection.Contains(new Token.Token("="))) {
                        var l = collection.Split(new Token.Token("="));
                        if (l.Count == 2) {
                            ConstantName = l[0][0];

                            EvaluationStatement eval = new EvaluationStatement(true);
                            eval.Parse(l[1]);
                            ConstantValue = eval;
                        } else collection[0].Error = new TokenizerException("SS0002");
                    } else collection[0].Error = new TokenizerException("SS0014");
                    break;
                case DefinitionType.Function:
                    ParseAbsoluteFunctionDefinition(collection);
                    break;
                case DefinitionType.Class:
                    ParseClassDefinition(collection);
                    break;
            }

            foreach (var item in this.Children) {
                RawToken.AddRange(item.RawToken);
            }
        }

        private void ParseClassDefinition(TokenCollection collection)
        {
            if (collection.Contains(new Token.Token("derives"))) {
                var l = collection.Split(new Token.Token("derives"));
                if (l.Count == 2) {
                    ParseClassDefinition(l[0]);

                    var derivationList = l[1].Split(new Token.Token(","));
                    foreach (var item in derivationList) {
                        EvaluationStatement eval = new EvaluationStatement(true);
                        eval.Parse(item);
                        this.ClassDerivations.Add(eval);
                    }

                    return;
                } else collection[0].Error = new TokenizerException("SS0015");
            }

            if (collection.Contains(new Token.Token(":"))) {
                var l = collection.Split(new Token.Token(":"));
                if (l.Count == 2) {
                    ParseAbsoluteClassName(l[0]);

                    var inheritage = l[1].Split(new Token.Token(","));
                    foreach (var item in inheritage) {
                        EvaluationStatement eval = new EvaluationStatement(true);
                        eval.Parse(item);
                        this.ClassInheritages.Add(eval);
                    }
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
                        EvaluationStatement e = new EvaluationStatement(true);
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

        public override string Generate(GenerationContext ctx)
        {
            switch (this.Type) {
                case DefinitionType.Constant:
                    string constant = ((Visibility == Visibility.Expose) ? "public " : "private ") + "dynamic " + ConstantName?.Value;
                    if (this.IsReadonly) constant += " { get { return _get_" + ConstantName?.ToString() + "(); } }";
                    else constant += "{ get { return _get_" + ConstantName?.ToString() + "(); } set { _set_" + ConstantName?.Value + "(value); } }";

                    // register the _get method for a hidden variable _name.
                    if(!ExplicitSet && !ExplicitGet) constant = ctx.Indention() + "private dynamic _" + ConstantName?.Value + " = undef;\n" + constant;
                    if (ExplicitGet)
                        constant = ctx.Indention() + "private dynamic _get_" + ConstantName?.Value + "()\n" + GetBlock?.Generate(ctx) + "\n" + constant;
                    else constant = ctx.Indention() + "private dynamic _get_" + ConstantName?.Value + "() { if(_" + ConstantName?.Value + " == undef) return " + 
                                    ConstantValue?.Generate(ctx) + "; return _" + ConstantName?.Value + "; }\n" + constant;
                    
                    if (ExplicitSet)
                        constant = ctx.Indention() + "private dynamic _set_" + ConstantName?.Value + "(dynamic value)\n" + SetBlock?.Generate(ctx) + "\n" + constant;
                    else constant = ctx.Indention() + "private dynamic _set_" + ConstantName?.Value + "(dynamic value) { _" + ConstantName?.Value + " = value; }\n" + constant;

                    if (string.IsNullOrEmpty(ctx.DefinerName)) ctx.Objects.Add("+[0]" + ConstantName?.Value);
                    else ctx.Objects.Add("+[0]" + ctx.DefinerName + "." + ConstantName?.Value);

                    return constant;

                case DefinitionType.Function:
                    string function = ((Visibility == Visibility.Expose) ? "public " : "private ") + (IsStatic ? "static " : "") + "dynamic " + FunctionName?.Value + "(dynamic args[]) {\n";

                    function += ctx.Indention() + "    pushscope();\n";
                    int i = 0;
                    foreach (Parameter item in FunctionParameters) {
                        function += ctx.Indention() + "    scopes[" + (ctx.Scopes.Count - 1) + "]." + item.Name?.Value + " = args[" + i + "];\n";
                        i++;
                    }

                    string name;
                    if (string.IsNullOrEmpty(ctx.DefinerName)) name = ("" + FunctionName?.Value);
                    else name = (ctx.DefinerName + "." + FunctionName?.Value);
                    ctx.Objects.Add("+[" + (ctx.Scopes.Count - 1) + "]" + FunctionName?.Value);

                    string parent = ctx.DefinerName;
                    ctx.DefinerName = name;
                    ctx.PushScope("Function " + FunctionName?.Value);
                    string funcBlock = new BlockStatement() { Children = this.Children }.Generate(ctx);
                    ctx.PopScope();
                    ctx.DefinerName = parent;

                    List<string> lines = funcBlock.Split('\n').ToList();
                    lines[0] = function;
                    lines[lines.Count - 1] = (ctx.Indention() + "    popscope();\n" + ctx.Indention() + "}");
                    return lines.JoinString("\n");

                case DefinitionType.Class:
                    string cls = ((Visibility == Visibility.Expose) ? "public " : "private ") + (IsStatic ? "static " : "") + "class " + ClassName?.Value;
                    List<string> inheritages = new List<string>();
                    if (this.ClassInheritages.Count == 0) { } else {
                        foreach (var item in ClassInheritages) {
                            inheritages.Add(item.Generate(ctx));
                        }
                    }

                    string inh = inheritages.JoinString(", ");
                    if (!string.IsNullOrWhiteSpace(inh)) cls += inh;
                    cls += " {\n";
                    ctx.IndentionLevel++;

                    if (string.IsNullOrEmpty(ctx.DefinerName)) name = ("" + ConstantName?.Value);
                    else name = (ctx.DefinerName + "." + ConstantName?.Value);
                    ctx.Objects.Add("+[0]" + name);
                    parent = ctx.DefinerName;
                    ctx.DefinerName = name;

                    foreach (var item in this.Children) {
                        if(item is DefinitionBlock def) {
                            def.IsStatic = false;
                            cls += def.Generate(ctx);
                        }
                    }

                    ctx.DefinerName = parent;

                    ctx.IndentionLevel--;
                    cls += ctx.Indention() + "}";

                    return cls;

                default:
                    break;
            }

            return ctx.Indention() + "// invalid definition";
        }
    }
}