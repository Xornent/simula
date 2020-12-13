using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Simula.Scripting.Contexts;
using Simula.Scripting.Syntax;
using Simula.Scripting.Token;

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
