using Simula.Scripting.Build;
using Simula.Scripting.Token;
using System;
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

            // the 'collection' here is a sequence:

            // class_name [: a, b, c ...] [derives a, b, c ...]
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
                    this.IsStatic = false;
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
                    string constant = ctx.Indention() + ((Visibility == Visibility.Expose) ? "public " : "private ") + "dynamic " + ConstantName?.Value;
                    if (this.IsReadonly) constant += " { get { return _get_" + ConstantName?.ToString() + "(); } }";
                    else constant += "{ get { return _get_" + ConstantName?.ToString() + "(); } set { _set_" + ConstantName?.Value + "(value); } }";

                    // register the _get method for a hidden variable _name.
                    if(!ExplicitSet && !ExplicitGet) constant = ctx.Indention() + "private dynamic _" + ConstantName?.Value + " = undef;\n" + constant;
                    if (ExplicitGet)
                        constant = ctx.Indention() + "private dynamic _get_" + ConstantName?.Value + "()\n" + GetBlock?.Generate(ctx) + "\n" + constant;
                    else constant = ctx.Indention() + "private dynamic _get_" + ConstantName?.Value + "() { if(undef.Equals(_" + ConstantName?.Value + ")) return " + 
                                    ConstantValue?.Generate(ctx) + "; return _" + ConstantName?.Value + "; }\n" + constant;
                    
                    if (ExplicitSet)
                        constant = "private dynamic _set_" + ConstantName?.Value + "(dynamic value)\n" + SetBlock?.Generate(ctx) + "\n" + constant;
                    else constant = "private dynamic _set_" + ConstantName?.Value + "(dynamic value) { _" + ConstantName?.Value + " = value; return 0; }\n" + constant;

                    return constant;

                case DefinitionType.Function:
                    string function = ((Visibility == Visibility.Expose) ? "public " : "private ") + (IsStatic ? "static " : "") + "dynamic " + FunctionName?.Value;

                    ctx.PushScope("Function " + FunctionName?.Value);

                    // the parameters of function is set to dynamic[] args. to perform its correct function,
                    // we should alias all used arguments and give them a name.

                    // once declaring an object in a scope, we should also call ctx.RegisterObject to register
                    // that name in the context, this tells the generator the name has already been declared.
                    // and when assigning it, we should not declare it again.

                    int i = 0;
                    List<string> parameterList = new List<string>();
                    foreach (Parameter item in FunctionParameters) {
                        parameterList.Add("dynamic " + item.Name?.Value);
                        ctx.RegisterObject(item.Name?.Value + "");
                        i++;
                    }

                    function += "(" + parameterList.JoinString(", ") + ") {";

                    // in non-static member functions, 'this' is reserved

                    if(!IsStatic) {
                        ctx.RegisterObject("this");
                    }

                    // all other members declared in the same class (if the function is defined in a class)
                    // should be registered. a temp map inside the ctx is used to record the members of a class
                    // and it is initialized every time in a class definition block: see below.

                    if (ctx.classMembers.Count > 0 && !(IsStatic))
                        foreach (var item in ctx.classMembers[ctx.classMembers.Count - 1]) {
                            ctx.RegisterObject(item);
                        }

                    string name;
                    if (string.IsNullOrEmpty(ctx.DefinerName)) name = ("" + FunctionName?.Value);
                    else name = (ctx.DefinerName + "." + FunctionName?.Value);
                    
                    string parent = ctx.DefinerName;
                    ctx.DefinerName = name;
                    string funcBlock = new BlockStatement() { Children = this.Children }.Generate(ctx);
                    ctx.PopScope();
                    ctx.DefinerName = parent;

                    List<string> lines = funcBlock.Split('\n').ToList();
                    lines[0] = function;
                    lines[lines.Count - 1] = ctx.Indention() + "    return 0;\n" + ctx.Indention() + "}";
                    return lines.JoinString("\n");

                case DefinitionType.Class:

                    // note that a class in c-sharp has a unique name itself, while the true method of initializing
                    // an instance of class is by calling a function with the name of the class.

                    string alias = "_" + Guid.NewGuid().ToString().Replace("-", "_").ToLower();
                    string creation = "// constructors";
                    
                    string cls = ((Visibility == Visibility.Expose) ? "public " : "private ") + (IsStatic ? "static " : "") + "class " + alias;
                    List<string> inheritages = new List<string>();
                    if (this.ClassInheritages.Count == 0) { } else {
                        foreach (var item in ClassInheritages) {
                            inheritages.Add(item.Generate(ctx));
                        }
                    }

                    string inh = inheritages.JoinString(", ");
                    if (!string.IsNullOrWhiteSpace(inh)) cls += " : " + inh;
                    cls += " {\n";
                   
                    if (string.IsNullOrEmpty(ctx.DefinerName)) name = ("" + ConstantName?.Value);
                    else name = (ctx.DefinerName + "." + ConstantName?.Value);
                    ctx.RegisterObject(ClassName?.Value + "");
                    parent = ctx.DefinerName;
                    ctx.DefinerName = name;

                    // set the member map in a class. : see above description.

                    HashSet<string> members = new HashSet<string>();
                    bool hasDefinedConstructor = false;
                    foreach (var item in this.Children) {
                        if (item is DefinitionBlock def) {
                            switch (def.Type) {
                                case DefinitionType.Constant:
                                    members.Add(def.ConstantName?.Value ?? "");
                                    break;
                                case DefinitionType.Function:
                                    if(def.FunctionName == "_init") {
                                        hasDefinedConstructor = true;
                                        List<string> ctorParam = new List<string>();
                                        List<string> ctorNames = new List<string>();
                                        foreach (Parameter funcParam in def.FunctionParameters) {
                                            ctorParam.Add("dynamic " + funcParam.Name?.Value);
                                            ctorNames.Add(funcParam.Name?.Value ?? "undef");
                                        }

                                        creation += 
                                     "\n" + ctx.Indention() + ((Visibility == Visibility.Expose) ? "public " : "private ") + "static " + alias + " " + ClassName?.Value + "(" + ctorParam.JoinString(", ") + ") {\n" +
                                            ctx.Indention() + "    dynamic creation = new " + alias + "();\n" +
                                            ctx.Indention() + "    creation._init(" + ctorNames.JoinString(", ") + ");\n" +
                                            ctx.Indention() + "    return creation;\n" +
                                            ctx.Indention() + "}";
                                    }

                                    members.Add(def.FunctionName?.Value ?? "");
                                    break;
                                case DefinitionType.Class:
                                    members.Add(def.ClassName?.Value ?? "");
                                    break;
                                default:
                                    break;
                            }
                        }
                    }

                    // if no constructor (initialization function _init) is defined explicitly, the converter provides a _init
                    // with no parameter. making the class visible.

                    if(!hasDefinedConstructor) 
                        creation = ((Visibility == Visibility.Expose) ? "public " : "private ") + "static " + alias + " " + ClassName?.Value + "() {\n" +
                            ctx.Indention() + "    return new " + alias + "();\n" +
                            ctx.Indention() + "}";
                    ctx.classMembers.Add(members);

                    ctx.IndentionLevel++;
                    cls += ctx.Indention() + "// class " + ClassName?.Value;
                    foreach (var item in this.Children) {
                        if(item is DefinitionBlock def) {
                            def.IsStatic = IsStatic;
                            cls += "\n" + ctx.Indention() + def.Generate(ctx);
                        }
                    }

                    ctx.classMembers.RemoveAt(ctx.classMembers.Count - 1);

                    ctx.DefinerName = parent;

                    ctx.IndentionLevel--;
                    cls += "\n" + ctx.Indention() + "}";

                    return creation + "\n" + ctx.Indention() + cls;

                default:
                    break;
            }

            return ctx.Indention() + "// invalid definition";
        }
    }
}