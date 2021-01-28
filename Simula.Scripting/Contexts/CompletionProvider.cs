using Simula.Editor.CodeCompletion;
using System;
using System.Linq;
using System.Collections.Generic;
using Simula.Scripting.Syntax;
using System.Text;

namespace Simula.Scripting.Contexts
{
    public class CompletionProvider
    {
        public CompletionContext Context;
        public CompletionCaret Caret;

        public CompletionProvider(CompletionContext ctx, CompletionCaret caret)
        {
            this.Context = ctx;
            this.Caret = caret;
        }

        public List<ICompletionData> GetCompletionData()
        {
            List<ICompletionData> data = new List<ICompletionData>();
            List<Syntax.Statement> cascadeList = Caret.Cascade.ToList();

            if (cascadeList.Count > 0) {
                if (cascadeList[0] is CommentBlock)
                    return data;
                else if (cascadeList[0] is EvaluationStatement eval) {
                    if (eval.RawToken.Last().ToString() == ";") eval.RawToken.RemoveLast();
                    if (eval.RawToken.Last().ToString().StartsWith("\"") && 
                        !(eval.RawToken.Last().Value.Length>=2 && eval.RawToken.Last().Value.EndsWith("\"")))
                        return data;
                }
            }
                

            // keyword data

            if (cascadeList.Find((stmt) => { return (stmt is IfBlock || stmt is ElseIfBlock); }) != null)
                data.AddRange(new List<ICompletionData>() { Data.KeywordData.Registry[1], Data.KeywordData.Registry[2] });
            else data.AddRange(new List<ICompletionData>() { Data.KeywordData.Registry[0] });

            if (cascadeList.Find((stmt) => { return (stmt is WhileBlock || stmt is IterateBlock); }) != null)
                data.AddRange(new List<ICompletionData>() { Data.KeywordData.Registry[17], Data.KeywordData.Registry[22] });
            else { }

            if(cascadeList.Count > 1) {
                data.AddRange(new List<ICompletionData>() { Data.KeywordData.Registry[3] });
            } else {
                data.AddRange(new List<ICompletionData>() { 
                    Data.KeywordData.Registry[4],
                    Data.KeywordData.Registry[5],
                    Data.KeywordData.Registry[6]
                });
            }

            if (cascadeList.Count > 0) {
                var current = cascadeList[0];
                if (current.RawToken.Count > 0) {
                    if (current.RawToken[0] != Token.Token.LineBreak)
                        data.AddRange(new List<ICompletionData>() {
                            Data.KeywordData.Registry[7],
                            Data.KeywordData.Registry[11],
                            Data.KeywordData.Registry[12],
                            Data.KeywordData.Registry[13],
                            Data.KeywordData.Registry[14],
                            Data.KeywordData.Registry[15],
                            Data.KeywordData.Registry[16],
                            Data.KeywordData.Registry[18],
                            Data.KeywordData.Registry[19]
                        });
                    {
                        if (current.RawToken[0] == "expose" || current.RawToken[0] == "hidden" || current.RawToken[0] == "def") {
                            data.Clear();
                            if (current.RawToken.Contains(new Token.Token("func")) ||
                                current.RawToken.Contains(new Token.Token("class")) ||
                                current.RawToken.Contains(new Token.Token("var"))) {
                                if(current.RawToken.Contains(new Token.Token("(")) ||
                                    current.RawToken.Contains(new Token.Token("=")) ||
                                    current.RawToken.Contains(new Token.Token(":")) ||
                                    current.RawToken.Contains(new Token.Token("derives"))) {

                                } else {
                                    data.Clear();
                                    if (current.RawToken.Contains(new Token.Token("class")) && current.RawToken.Count >= 2)
                                        if (current.RawToken[current.RawToken.Count - 1] != "class")
                                            data.Add(Data.KeywordData.Registry[23]);
                                    return data; // empty
                                }
                            } else {
                                data.Clear();
                                data.AddRange(new List<ICompletionData>() {
                        Data.KeywordData.Registry[8],
                        Data.KeywordData.Registry[9],
                        Data.KeywordData.Registry[10]
                                });
                                return data;
                            }
                        } else if (current.RawToken[0] == "iter") {
                            data.Clear();
                            if (!current.RawToken.Contains(new Token.Token("in")))
                                data.AddRange(new List<ICompletionData>() {
                            Data.KeywordData.Registry[20]
                        });
                            else
                                data.AddRange(new List<ICompletionData>() {
                            Data.KeywordData.Registry[21]
                        });
                        }
                    }
                } else {
                    data.AddRange(new List<ICompletionData>() {
                    Data.KeywordData.Registry[7],
                    Data.KeywordData.Registry[11],
                    Data.KeywordData.Registry[12],
                    Data.KeywordData.Registry[13],
                    Data.KeywordData.Registry[14],
                    Data.KeywordData.Registry[15],
                    Data.KeywordData.Registry[16],
                    Data.KeywordData.Registry[18],
                    Data.KeywordData.Registry[19]
                });
                }
            }

            // primary accessible variables

            foreach (var item in Context.AccessibleRoots) {
                if (item.Cache == null)
                    data.Add(new Data.FunctionData(item.Name, item.Comments, item.Documentation));
                else data.Add(item.Cache);
            }

            // member selection

            if(cascadeList.Count>0) {
                if(cascadeList[0].RawToken.Count>0) {
                    if (cascadeList[0].RawToken.Contains(new Token.Token("."))) {
                        List<string> members = new List<string>();
                        var tokens = cascadeList[0].RawToken;
                        if (tokens.Last() != ".") tokens.RemoveLast();

                        label_loop:
                        if (tokens.Count > 0)
                            if (tokens.Last() == ".") {
                                tokens.RemoveLast();
                                members.Insert(0, tokens.Last());
                                tokens.RemoveLast();
                                goto label_loop;
                            }

                        var mem = Context.AccessibleRoots;
                        CompletionRecord? parent = null;
                        while (mem.Find((rec) => {
                            return (rec.Name == members.FirstOrDefault());
                        }) != null) {
                            parent = mem.Find((rec) => {
                                return (rec.Name == members.FirstOrDefault());
                            });
                            mem = parent.Children;
                            members.RemoveAt(0);
                            if (members.Count == 0) break;
                        }

                        data.Clear();
                        foreach (var item in mem) {
                            if (item is CompletionTypeRecord type) { } else {
                                if (item.Cache == null)
                                    data.Add(new Data.FunctionData(item.Name, item.Comments, item.Documentation));
                                else data.Add(item.Cache);
                            }
                        }

                        if (parent != null)
                            if (!parent.Type.Contains("any"))
                                foreach (var types in parent.Type) {
                                    if (types != "null" && Context.ClassRecords.ContainsKey(types)) {
                                        AddTypedMembers(types, data);
                                    }
                                }
                    }
                }
            }

            return data;
        }

        public void AddTypedMembers(string types, List<ICompletionData> data)
        {
            foreach (var classtype in Context.ClassRecords[types].Children) {
                if (classtype is CompletionTypeRecord typerec) {
                    AddTypedMembers(typerec.Reference, data);
                } else {
                    if (classtype.Cache == null)
                        data.Add(new Data.FunctionData(classtype.Name, classtype.Comments, classtype.Documentation));
                    else data.Add(classtype.Cache);
                }
            }
        }
    }
}
