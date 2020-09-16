using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Completion {

    public class CompletionProvider {
        public static List<Data.KeywordData> AllowedKeywords(List<string> beforeLines, string line) {
            Stack<string> context = new Stack<string>();
            foreach (var ln in beforeLines) {
                string s = ln.Trim();
                if (s.StartsWith("while")) context.Push("while");
                if (s.StartsWith("expose")) {
                    if (s.Remove(0, 6).Trim().StartsWith("def")) s = s.Remove(0, 6).Trim();
                    if (s.Remove(0, 6).Trim().StartsWith("func")) context.Push("deffunc");
                    if (s.Remove(0, 6).Trim().StartsWith("class")) context.Push("defclass");
                }
                if (s.StartsWith("hidden")) {
                    if (s.Remove(0, 6).Trim().StartsWith("def")) s = s.Remove(0, 6).Trim();
                    if (s.Remove(0, 6).Trim().StartsWith("func")) context.Push("deffunc");
                    if (s.Remove(0, 6).Trim().StartsWith("class")) context.Push("defclass");
                }
                if (s.StartsWith("def")) {
                    if (s.Remove(0, 3).Trim().StartsWith("func")) context.Push("deffunc");
                    if (s.Remove(0, 3).Trim().StartsWith("class")) context.Push("defclass");
                }
                if (s.StartsWith("if")) context.Push("if");
                if (s.StartsWith("eif")) {
                    if (context.Count > 0)
                        if (context.Peek() == "if" || context.Peek() == "eif")
                            context.Pop();
                    context.Push("eif");
                }
                if (s.StartsWith("else")) {
                    if (context.Count > 0)
                        if (context.Peek() == "if" || context.Peek() == "eif")
                            context.Pop();
                    context.Push("else");
                }
                if (s.StartsWith("end")) {
                    if (context.Count > 0)
                        context.Pop();
                }
            }

            List<Data.KeywordData> keys = new List<Data.KeywordData>();
            if (line.Trim() == "") {
                keys.Add(Data.KeywordData.Registry[0]);
                keys.Add(Data.KeywordData.Registry[18]);
                keys.Add(Data.KeywordData.Registry[14]);

                if (context.Count == 0) {
                    keys.Add(Data.KeywordData.Registry[4]);
                    keys.Add(Data.KeywordData.Registry[5]);
                    keys.Add(Data.KeywordData.Registry[6]);
                } else {
                    keys.Add(Data.KeywordData.Registry[3]);
                }

                if (context.Contains("if") || context.Contains("eif")) {
                    keys.Add(Data.KeywordData.Registry[1]);
                    keys.Add(Data.KeywordData.Registry[2]);
                }

                if (context.Contains("defclass")) {
                    keys.Add(Data.KeywordData.Registry[7]);
                    keys.Add(Data.KeywordData.Registry[11]);
                    keys.Add(Data.KeywordData.Registry[12]);
                }

                if (context.Contains("deffunc")) {
                    keys.Add(Data.KeywordData.Registry[16]);
                }

                if (context.Contains("while")) {
                    keys.Add(Data.KeywordData.Registry[17]);
                }
            }

            if(line.Trim().EndsWith( "def")) {
                if (context.Contains("deffunc")) {

                }else if (context.Contains("defclass")) {
                    keys.Add(Data.KeywordData.Registry[9]); 
                    keys.Add(Data.KeywordData.Registry[10]);
                } else {
                    keys.Add(Data.KeywordData.Registry[8]);
                    keys.Add(Data.KeywordData.Registry[9]);
                    keys.Add(Data.KeywordData.Registry[10]);
                }
            }
            if (line.Trim().EndsWith("expose") || line.Trim().EndsWith("hidden")) {
                if (context.Contains("deffunc")) {

                } else if (context.Contains("defclass")) {
                    keys.Add(Data.KeywordData.Registry[7]);
                    keys.Add(Data.KeywordData.Registry[9]);
                    keys.Add(Data.KeywordData.Registry[10]);
                } else {
                    keys.Add(Data.KeywordData.Registry[7]);
                    keys.Add(Data.KeywordData.Registry[8]);
                    keys.Add(Data.KeywordData.Registry[9]);
                    keys.Add(Data.KeywordData.Registry[10]);
                }
            }

            return keys;
        }
    }
}
