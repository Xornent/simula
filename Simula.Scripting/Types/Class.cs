using System.ComponentModel.DataAnnotations;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Dynamic;
using Simula.Scripting.Contexts;
using Simula.Scripting.Syntax;
using System.Text;

namespace Simula.Scripting.Types
{
    public class Class : Var
    {
        public Class() : base() { }
        public Class(Type type, dynamic[]? native = null) : base()
        {
            this.ClrType = type;
            this.ClrArguments = native;
            this.IsCompiled = true;
        }

        public Class(string module, DefinitionBlock block) : base()
        {
            this.IsCompiled = false;
            this.Definition = block;
            this.ModuleName = module;
        }

        private Type? ClrType;
        private dynamic[]? ClrArguments;

        private string ModuleName;
        private DefinitionBlock Definition;
        private bool IsCompiled = false;

        // 'self' is a Class object.
        // 'args[0]' is reserved for dynamic context.
        // from 'args[1]' to the end of the argument array is the constructor argument.
        public static Function _create = new Function((self, args) => {

            // the 'IsCompiled' field indicates whether the class is loaded from a 
            // c-sharp assembly, or is declared in a script.

            if (self.IsCompiled) {

                // loaded from assembly, the inheritage is automatically compiled 
                // into the reflection type.

                var instance = Activator.CreateInstance(self.ClrType, System.Reflection.BindingFlags.Default, null, self.ClrArguments, null);
                if (!args[0].FunctionCache.ContainsKey(instance.type))
                    args[0].CacheFunction(instance.type, instance.GetType());

                List<dynamic> arguments = args.ToList();
                arguments.RemoveAt(0);

                // check _init function in the dynamic context function cache.
                Predicate<Function> predicate = (pred) => {
                    if (pred.name == "_init") return true;
                    else return false;
                };
                Function f = args[0].FunctionCache[instance.type].Find(predicate);
                if (f != null) f._call(instance, arguments.ToArray());
                return instance ?? Null.NULL;
            } else {
                Var expando = new Var();
                var dictionary = expando._fields;

                expando.type = self.ModuleName + "." + self.Definition.ClassName?.ToString();
                string key = expando.type;
                bool registeredFunction = args[0].FunctionCache.ContainsKey(key);

                List<Function> functions = new List<Function>();

                // add the inherited fields and functions.
                // for fields, we can initialize a instance of father class and read
                // all its _fields member.
                // for functions, we can read from the context function cache.

                foreach (EvaluationStatement item in self.Definition.ClassInheritages) {
                    var cls = item.Execute(args[0]).Result;
                    dynamic inst = _create._call(cls, new dynamic[]{ args[0] });

                    foreach (var field in inst._fields) {
                        if (dictionary.ContainsKey(field.Key))
                            dictionary[field.Key] = field.Value;
                        else dictionary.Add(field.Key, field.Value);
                    }

                    if (!registeredFunction)
                        foreach (var funcs in args[0].FunctionCache[inst.type]) {
                            functions.Add(funcs);
                        }
                }

                foreach (Statement stmt in self.Definition.Children) {
                    if (stmt is DefinitionBlock def) {
                        if (def.Type == DefinitionType.Constant) {
                            dictionary[def.ConstantName ?? "_annonymous_"] =
                                (def.ConstantValue?.Execute(args[0])?.Result ?? Types.Null.NULL);
                        }
                    }
                }

                if (!registeredFunction) {

                    // cache function for user-defined classes in simula

                    foreach (Statement stmt in self.Definition.Children) {
                        if (stmt is DefinitionBlock def) {
                            if (def.Type == DefinitionType.Function) {
                                List<Pair> funcParams = new List<Pair>();
                                foreach (var par in def.FunctionParameters) {
                                    funcParams.Add(new Pair(new Types.String(par.Name ?? ""), new Types.String("any")));
                                }

                                Function func = new Function((Func<dynamic, dynamic[], dynamic>)((self, args2) => {
                                    ScopeContext scope = new ScopeContext();
                                    var dict = (IDictionary<string, object>)scope.Store;
                                    scope.Permeable = true;

                                    // register local variables to the function call scope:
                                    // 1. arguments of the function
                                    // 2. 'this' variable
                                    // 3. all functions and fields declared in the same class.

                                    int count = 0;
                                    foreach (var par in def.FunctionParameters) {
                                        dict[par.Name ?? ""] = args2[count];
                                        count++;
                                    }

                                    foreach (Function par in args[0].FunctionCache[key]) {
                                        dict[par.name ?? ""] = par;
                                        count++;
                                    }

                                    dict["this"] = self;

                                    args[0].Scopes.Add(scope);

                                    // note that merely adding the _fields elements to the context can provide read functions,
                                    // but setting them will change the reference and set as a copy, the original value will
                                    // not change. by giving a reference to 'this' works.

                                    foreach (var item in self._fields) {
                                        dict[item.Key] = new Reference(args[0], "this." + item.Key);
                                    }

                                    BlockStatement block = new BlockStatement() { Children = def.Children };
                                    dynamic result = block.Execute(args[0]);

                                    args[0].Scopes.RemoveAt(args[0].Scopes.Count - 1);
                                    return result;
                                }), funcParams);

                                func.fullName = new List<string>() { ((self.ModuleName == "") ? "" : (self.ModuleName + ".")) + def.FunctionName?.ToString() ?? "_annonymous_" };
                                func.name = def.FunctionName?.ToString() ?? "_annonymous_";

                                // cache the function into dynamic context
                                functions.Add(func);
                            }
                        }
                    }

                    // the functions is added to the collection in a sequence that derived functions
                    // first, declared function follows. but we need declared functions are prior to 
                    // the functions inherited (overlap). so we should reverse the sequence given that
                    // we find functions with the least index first.

                    functions.Reverse();
                    args[0].FunctionCache.Add(key, functions);
                }

                List<dynamic> arguments = args.ToList();
                arguments.RemoveAt(0);

                // check _init function in the dynamic context function cache.
                Predicate<Function> predicate = (pred) => {
                    if (pred.name == "_init") return true;
                    else return false;
                };
                Function f = args[0].FunctionCache[key].Find(predicate);
                if (f != null) f._call(expando, arguments.ToArray());
                return expando;
            }
        }, new List<Pair>() { });

        internal new string type = "sys.class";

        public override string ToString()
        {
            if(IsCompiled) {
                return "<native> class: " + ClrType?.Name;
            }

            return "class: " + Definition.ClassName.ToString();
        }
    }
}
