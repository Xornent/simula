using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Dynamic;
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
            // find the 'module' directive to locate the module the following
            // definition elements should be added to.

            ExpandoObject store = ctx.Store;
            string moduleFullName = "";

            foreach (var item in this.Body.Children) {
                if(item is ModuleStatement mod) {
                    string[] hierachy = mod.FullName.Split(".");
                    store = ctx.Store;
                    moduleFullName = mod.FullName;
                    foreach(string str in hierachy) {
                        IDictionary<string, object> dict = (IDictionary<string, object>)store;
                        if (dict.ContainsKey(str)) store = (ExpandoObject)dict[str];
                        else {
                            dynamic obj = new ExpandoObject();
                            obj.fullName = new List<string>() { str };
                            dict[str] = obj;
                            store = obj;
                        }
                    }

                } else if (item is DefinitionBlock def) {
                    switch (def.Type) {
                        case DefinitionType.Constant:
                            dynamic result = def.ConstantValue?.Execute(ctx).Result ?? Types.Null.NULL;
                            result.fullName = new List<string>() { ((moduleFullName == "") ? "" : (moduleFullName + ".")) + def.ConstantName?.ToString() ?? "_annonymous_" };
                            var dict = (IDictionary<string, object>)store;
                            dict[def.ConstantName ?? "_annonymous_"] = result;
                            break;
                        case DefinitionType.Function:

                            // the following piece of code is to register user-defined functions
                            // this wraps up interpreter into a CLR annonymous function.
                            // the code has another variations at class.cs/_create

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

                            func.name = def.FunctionName?.ToString() ?? "_annonymous_";
                            func.fullName = new List<string>() { ((moduleFullName == "") ? "" : (moduleFullName + ".")) + def.FunctionName?.ToString() ?? "_annonymous_" };
                            dict = (IDictionary<string, object>)store;
                            dict[def.FunctionName ?? "_annonymous_"] = func;
                            break;
                        case DefinitionType.Class:
                            dynamic cls = new Class(moduleFullName, def);
                            cls.fullName = new List<string>() { ((moduleFullName == "") ? "" : (moduleFullName + ".")) + def.ClassName?.ToString() ?? "_annonymous_" };
                            dict = (IDictionary<string, object>)store;
                            dict[def.ClassName ?? "_annonymous_"] = cls;
                            break;
                        default:
                            break;
                    }
                } 
            }
        }
    }
}
