using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Simula.Scripting.Contexts;
using Simula.Scripting.Syntax;
using Simula.Scripting.Token;
using Simula.Scripting.Types;

namespace Simula.Scripting.Dom
{
    public class Source
    {
        public Source() { }
        public Source(string fileName, FileMode mode = FileMode.OpenOrCreate)
        {
            FileStream file = new FileStream(fileName, mode);
            StreamReader reader = new StreamReader(file);
            this.Content = reader.ReadToEnd().Replace("\t", "    ");
            this.Location = fileName;
        }

        public static Source FromSourceCode(string code)
        {
            return new Source() { Content = code };
        }

        public string Location { get; set; }
        private string content = "";
        public string Content { 
            get {
                return content;
            }

            set {
                TokenDocument doc = new TokenDocument();
                doc.Tokenize(value);
                this.Body = new BlockStatement();
                this.Body.Parse(doc.Tokens);
                this.content = value;
            }
        }

        public BlockStatement Body { get; private set; }

        public void LoadDefinition(DynamicRuntime ctx)
        {
            Execution result = new Execution();
            foreach (var item in this.Body.Children) {
                if (item is DefinitionBlock def) {
                    switch (def.Type) {
                        case DefinitionType.Constant:
                            break;
                        case DefinitionType.Function:
                            List<Pair> funcParams = new List<Pair>();
                            foreach (var par in def.FunctionParameters) {
                                funcParams.Add(new Pair(new Types.String(par.Name ?? ""), new Types.String("any")));
                            }

                            Function func = new Function((Func<dynamic, dynamic[], dynamic>)((self, args) => {
                                ScopeContext scope = new ScopeContext();
                                var dict = (IDictionary<string, object>)scope.Store;
                                scope.Permeable = true;

                                int count = 0;
                                foreach (var par in def.FunctionParameters) {
                                    dict[par.Name ?? ""] = args[count];
                                    count++;
                                }

                                ctx.Scopes.Add(scope);

                                BlockStatement block = new BlockStatement() { Children = def.Children };
                                dynamic result = block.Execute(ctx);

                                ctx.Scopes.RemoveAt(ctx.Scopes.Count - 1);
                                return result;
                            }), funcParams);

                            var dict = (IDictionary<string, object>)ctx.Store;
                            dict[def.FunctionName] = func;
                            break;
                        case DefinitionType.Class:
                            break;
                        default:
                            break;
                    }
                } 
            }
        }
    }
}
