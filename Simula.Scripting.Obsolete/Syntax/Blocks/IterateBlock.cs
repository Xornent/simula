using Simula.Scripting.Build;
using Simula.Scripting.Contexts;
using Simula.Scripting.Token;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Simula.Scripting.Syntax
{
    public class IterateBlock : BlockStatement
    {
        public EvaluationStatement? Enumerator = null;
        public EvaluationStatement? Collection = null;
        public EvaluationStatement? Position = null;

        public new void Parse(TokenCollection collection)
        {
            this.RawToken.AddRange(collection);
            System.Dynamic.ExpandoObject expando = new System.Dynamic.ExpandoObject();
            expando.TryAdd("_init", (Func<dynamic[], dynamic>)(delegate (dynamic[] a) {
                return "s";
            }));
            if (collection.Contains(new Token.Token("at"))) {
                List<TokenCollection> subitems = collection.Split(new Token.Token("at"));
                if (subitems.Count > 2) {
                    collection[0].Error = new TokenizerException("SS0010");
                    return;
                }

                Position = new EvaluationStatement();
                Position.Parse(subitems[1]);

                if (subitems[0].Contains(new Token.Token("in"))) {
                    var l = subitems[0].Split(new Token.Token("in"));
                    if (l.Count > 2) {
                        collection[0].Error = new TokenizerException("SS0010");
                        return;
                    }

                    l[0].RemoveAt(0);
                    Enumerator = new EvaluationStatement();
                    Enumerator.Parse(l[0]);
                    Collection = new EvaluationStatement();
                    Collection.Parse(l[1]);
                } else collection[0].Error = new TokenizerException("SS0011");
            } else {
                List<TokenCollection> subitems = collection.Split(new Token.Token("in"));
                if (subitems.Count > 2) {
                    collection[0].Error = new TokenizerException("SS0010");
                    return;
                } else if (subitems.Count == 2) {
                    subitems[0].RemoveAt(0);
                    Enumerator = new EvaluationStatement();
                    Enumerator.Parse(subitems[0]);
                    Collection = new EvaluationStatement();
                    Collection.Parse(subitems[1]);
                } else if(subitems.Count == 1) {
                    subitems[0].RemoveAt(0);
                    Enumerator = new EvaluationStatement();
                    Enumerator.Parse(subitems[0]);
                }
            }

            foreach (var item in this.Children) {
                this.RawToken.AddRange(item.RawToken);
            }
        }

        public override Execution Execute(DynamicRuntime ctx)
        {
            var code = new BlockStatement() { Children = this.Children };
            if (Enumerator == null) return new Execution();
            if (Collection == null) {
                dynamic enumTimes = Enumerator.Execute(ctx).Result;
                long counter = Convert.ToInt64(enumTimes);
                for(long i = 0; i < counter; i++) 
                    code.Execute(ctx);

                return new Execution();
            }

            dynamic matrix = Collection.Execute(ctx).Result;
            if (Enumerator.RawEvaluateToken.Count == 0) return new Execution();
            string sets = Enumerator.RawEvaluateToken[0];

            if(Position!= null) {
                if (Position.RawEvaluateToken.Count != 0) {
                    string pos = Position.RawEvaluateToken[0];

                    if (matrix is Types.Matrix) {
                        for (int i = 0; i < matrix.total; i++) {
                            Types.NumericalMatrix<int> loc = matrix.GetLocation(i + 1);
                            ctx.SetMember(sets, matrix.data[i]);
                            ctx.SetMember(pos, loc);
                            code.Execute(ctx);
                        }
                    }
                }
            } else {
                if (matrix is Types.Matrix) {
                    for (int i = 0; i < matrix.total; i++) {
                        Types.NumericalMatrix<int> loc = matrix.GetLocation(i + 1);
                        ctx.SetMember(sets, matrix.data[i]);
                        code.Execute(ctx);
                    }
                }
            }

            return new Execution();
        }

        public override string Generate(GenerationContext ctx)
        {
            BlockStatement block = new BlockStatement();
            block.Children = this.Children;
            block.Nonmodifier = true;
            ctx.PushScope("While");

            string str = "";
            if (this.Collection == null) {
                str = "for(int i = 0; i < " + this.Enumerator?.Generate(ctx) + "; i++) {";
            } else {
                string tempName = "_" + Guid.NewGuid().ToString().Replace("-", "_").ToLower();
                str = "dynamic " + tempName + " = " + this.Collection?.Generate(ctx) + ";";
                str += "\n" + ctx.Indention() + "for(int i = 0; i < " + tempName + ".data.Length; i ++) {";
                str += "\n" + ctx.Indention() + "    dynamic " + this.Enumerator?.Generate(ctx) + " = " + tempName + ".data[i];";
                ctx.RegisterObject(this.Enumerator?.Generate(ctx) ?? "");

                if(this.Position != null) {
                    str += "\n" + ctx.Indention() + "    dynamic " + this.Position?.Generate(ctx) + " = " + tempName + ".getLocation(i);";
                    ctx.RegisterObject(this.Position?.Generate(ctx) ?? "");
                }
            }

            List<string> lines = new BlockStatement() { Children = this.Children }.Generate(ctx).Split('\n').ToList();
            lines.RemoveAt(0);
            str += lines.JoinString("\n");

            ctx.PopScope();
            return str;
        }
    }
}
