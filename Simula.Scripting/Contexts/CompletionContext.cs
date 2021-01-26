using Simula.Editor.CodeCompletion;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;

namespace Simula.Scripting.Contexts
{
    // CompletionContext, compared to dynamic runtime is a less strict context
    // to obtain information of the current location in a document.

    public class CompletionContext
    {
        public List<CompletionRecord> Records = new List<CompletionRecord>();
        public List<CompletionRecord> AccessibleRoots = new List<CompletionRecord>();
        public Dictionary<string, CompletionRecord> ClassRecords = new Dictionary<string, CompletionRecord>();
        public Syntax.BlockStatement SourceBlock = new Syntax.BlockStatement();

        public CompletionContext(DynamicRuntime runtime)
        {
            // register classes and definitions outside the source file
            // this includes system library functions and functions defined in
            // non-entry sources that are loaded into the runtime ahead of time.

            foreach (var item in (IDictionary<string, object>)runtime.Store) {
                if (item.Key != "global") {
                    var rec = new CompletionRecord(runtime, item.Key, item.Value, ClassRecords);
                    Records.Add(rec);
                    AccessibleRoots.Add(rec);
                }
            }
        }

        public void SetSource(string src)
        {
            // now we pick out the definition statements inside the source file
            // and create a sink info for them.

            Token.TokenDocument doc = new Token.TokenDocument();
            doc.Tokenize(src);
            Syntax.BlockStatement block = new Syntax.BlockStatement();
            block.Parse(doc.Tokens);
            SourceBlock = block;

            AccessibleRoots.RemoveAll((rec) => {
                return rec.IsPublic == false;
            });

            foreach (var item in ClassRecords) {
                if (!item.Value.IsPublic) ClassRecords.Remove(item.Key);
            }

            RegisterDefinition(block, AccessibleRoots);
            InferDefinitionType(block, AccessibleRoots);
            RegisterAssignments(block);
        }

        public CompletionCaret GetCaret(int line, int column)
        {
            return new CompletionCaret(line, column, this.SourceBlock);
        }

        public void RegisterDefinition(Syntax.BlockStatement block, List<CompletionRecord> container)
        {
            foreach (var item in block.Children) {
                if(item is Syntax.DefinitionBlock def) {
                    CompletionRecord rec = new CompletionRecord();
                    rec.IsPublic = false;
                    switch (def.Type) {
                        case Syntax.DefinitionType.Constant:
                            if (def.ConstantName != null) {
                                rec.FullName = def.ConstantName;
                                rec.Name = def.ConstantName;
                            }
                            break;
                        case Syntax.DefinitionType.Function:
                            if (def.FunctionName != null) {
                                rec.Name = def.FunctionName;

                                rec.FullName = def.FunctionName;
                                rec.Type.Add("sys.func");
                            }
                            break;
                        case Syntax.DefinitionType.Class:
                            if (def.ClassName != null) {
                                rec.Name = def.ClassName;
                                rec.FullName = def.ClassName;
                                rec.IsPublic = false;
                                RegisterDefinition(new Syntax.BlockStatement() { Children = def.Children }, rec.Children);
                                if (!this.ClassRecords.ContainsKey(def.ClassName)) 
                                    this.ClassRecords.Add(def.ClassName, rec);
                                foreach (var inheritages in def.ClassInheritages) 
                                    rec.Children.Add(new CompletionTypeRecord( inheritages.RawEvaluateToken.ToString() ));
                                rec.Type.Add("sys.class");
                            }
                            break;
                        default:
                            break;
                    }
                    container.Add(rec);
                }
            }
        }

        public void InferDefinitionType(Syntax.BlockStatement block, List<CompletionRecord> container)
        {
            foreach (var item in block.Children) {
                if (item is Syntax.DefinitionBlock def) {

                    // here, we parse the documentation comment for the definition block.

                    string doc = "";
                    if(def.Documentation != null) {
                        foreach (string line in def.Documentation.Lines) {
                            string eval = line.Remove(0, 1).Replace("\r", "").Trim();
                            if (string.IsNullOrWhiteSpace(eval)) doc += "\n\n";
                            else doc += eval + " ";
                        }
                    }

                    switch (def.Type) {
                        case Syntax.DefinitionType.Constant:
                            if (def.ConstantName != null) {
                                CompletionRecord rec = container.Find((r) => { return def.ConstantName == r.Name; });
                                if (rec != null) {
                                    rec.Type.AddRange(def.ConstantValue?.InferType(this).Types ?? new HashSet<string>() { "null" });
                                    rec.Cache = new Data.LocalData(def.ConstantName,
                                       "(local) [" + rec.Type.JoinString(", ") + "] " + def.ConstantName, doc);
                                }
                            }
                            break;
                        case Syntax.DefinitionType.Function:
                            if (def.FunctionName != null) {
                                CompletionRecord rec = container.Find((r) => { return def.FunctionName == r.Name; });
                                if (rec != null) {

                                    // infer function return types by evaluating the types of every return
                                    // statement in the function definition.

                                    var type = InferReturnType(def, this);
                                    rec.ReturnTypes = type;

                                    List<string> param = new List<string>();
                                    foreach (var par in def.FunctionParameters) {
                                        param.Add(par.Type.RawEvaluateToken.ToString() + " " + par.Name);
                                    }

                                    rec.Comments = "def func " + def.FunctionName + " (" + param.JoinString(", ") + ")";
                                    rec.Cache = new Data.FunctionData(def.FunctionName,
                                        "def func " + def.FunctionName + " (" + param.JoinString(", ") + ")\n\nreturn [" + type.JoinString(", ") + "]", doc);
                                }
                            }
                            break;
                        case Syntax.DefinitionType.Class:
                            if (def.ClassName != null) {
                                CompletionRecord rec = container.Find((r) => { return def.ClassName == r.Name; });
                                if (rec != null) {
                                    rec.Type.Add("sys.class");
                                    InferDefinitionType(new Syntax.BlockStatement() { Children = def.Children }, rec.Children);

                                    // here, the display object of class should show its constructor method
                                    // by default it is _init function. and if it doesn't explicitly define
                                    // a _init, shows the system ctor with no parameter.

                                    var ctor = rec.Children.Find((rec) => { return rec.Name == "_init"; });
                                    if(ctor == null)
                                        rec.Cache = new Data.ClassData(rec.Name, "class " + rec.Name + " ()" , doc);
                                    else
                                        rec.Cache = new Data.ClassData(rec.Name, "class " + rec.Name + ctor.Comments.Replace("def func _init",""), doc);
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        public void RegisterAssignments(Syntax.BlockStatement block)
        {
            foreach (var item in block.Children) {
                if (item is Syntax.EvaluationStatement eval)
                    eval.InferType(this);
                else if (item is Syntax.BlockStatement b)
                    RegisterAssignments(b);
            }
        }

        public HashSet<string> InferReturnType(Syntax.BlockStatement def, CompletionContext ctx)
        {
            HashSet<string> returns = new HashSet<string>();
            foreach (var item in def.Children) {
                if(item is Syntax.BlockStatement block) {
                    var list = InferReturnType(block, ctx);
                    returns.AddRange(list);
                } else if(item is Syntax.ReturnStatement ret) {
                    returns.AddRange(ret.Evaluation?.InferType(ctx).Types ?? new HashSet<string> { "null" });
                }
            }
            return returns;
        }
    }

    public class CompletionRecord
    {
        public List<CompletionRecord> Children = new List<CompletionRecord>();
        public string Name = "";
        public bool IsPublic = true;
        public string Comments = "";
        public string Documentation = "";

        public HashSet<string> Type = new HashSet<string>();
        public string FullName = "";
        public HashSet<string> ReturnTypes = new HashSet<string>();

        public ICompletionData? Cache = null;

        public CompletionRecord() { }
        public CompletionRecord(DynamicRuntime runtime, string name, dynamic container, Dictionary<string, CompletionRecord> dict, string comment = "",string doc = "", string containerName = "")
        {
            this.Name = name;
            this.FullName = containerName == "" ? name : containerName + "." + name;
            this.Comments = comment;
            this.Documentation = doc;

            if (container is ExpandoObject exp) {
                var d = (IDictionary<string, object>)exp;
                this.Comments = "module " + name;
                this.FullName = containerName == "" ? name : containerName + "." + name;
                this.Type.Add("module");
                foreach (var item in d) {
                    if(item.Key != "global")
                        this.Children.Add(new CompletionRecord(runtime, item.Key, item.Value, dict, item.Value.ToString(), item.Value is Types.Var v ? v.desc : "", this.FullName));
                }

            } else if(container is Types.Var variable) {
                if (!dict.ContainsKey(container.type)) {
                    CompletionRecord rec = new CompletionRecord();
                    dict.Add(container.type, rec);
                    rec.FullName = container.type;
                    foreach (var item in runtime.FunctionCache[container.type]) {
                        rec.Children.Add(new CompletionRecord(runtime, item.name, item, dict, item.ToString(), item.desc, container.type));
                    }

                    foreach (var item in container._fields) {
                        rec.Children.Add(new CompletionRecord(runtime, item.Key, item.Value, dict, "(local) ["+item.Value.type+"] " + item.Key, item.Value.desc, container.type));
                    }
                } else {
                    this.Children.Add(new CompletionTypeRecord(container.type) { 
                        FullName = container.type
                    });
                }
                this.Type.Add(container.type);
            } else if(container is Types.Reference refer) {
                this.Comments = "ref(" + refer.FullName + ")";
                this.FullName = this.Comments;
                this.Type.Add("ref");
            }

            if (container is Types.Function func) {
                this.Comments = func.ToString();
                this.Documentation = func.desc;
                this.ReturnTypes = func.returntypes.Count == 0 ? new HashSet<string>() { "any" } : func.returntypes;
                Cache = new Data.FunctionData(this.Name, this.Comments + "\n\nreturn [" + ReturnTypes.JoinString(", ") + "]", func.desc);
            } else if (container is Types.Class) {

                // in the code above, we register classes necessary to initialize members,
                // however, if some classes doesn't initialize any members in system library,
                // it will be ignored. we detect whether the class is register here again.

                if (!dict.ContainsKey(this.FullName)) {
                    CompletionRecord rec = new CompletionRecord();
                    dict.Add(this.FullName, rec);
                    rec.FullName = this.FullName;
                    foreach (var item in runtime.FunctionCache[this.FullName]) {
                        rec.Children.Add(new CompletionRecord(runtime, item.name, item, dict, item.ToString(), item.desc, this.FullName));
                    }
                }

                var ctor = dict[this.FullName].Children.Find((rec) => { return rec.Name == "_init"; });
                if (ctor == null)
                    Cache = new Data.ClassData(this.Name, "<native> class " + this.FullName, doc);
                else
                    Cache = new Data.ClassData(this.Name, "<native> class " + this.FullName + ctor.Comments.Replace("def func _init", ""), doc);
            } else if (container is ExpandoObject)
                Cache = new Data.ContainerData(this.Name, this.Comments, doc);
        }
    }

    public class CompletionTypeRecord : CompletionRecord
    {
        public string Reference;

        public CompletionTypeRecord(string type)
        {
            this.Reference = type;
        }
    }
}
