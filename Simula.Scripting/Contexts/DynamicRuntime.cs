using Simula.Scripting.Syntax;
using System.Collections.Generic;
using System.Dynamic;
using Simula.Scripting.Types;
using System;
using System.Reflection;

namespace Simula.Scripting.Contexts
{
    public class DynamicRuntime
    {
        public DynamicRuntime()
        {
            dynamic sys = new ExpandoObject();
            sys.fullName = new List<string> { "sys" };
            sys.@int = this.typeInt;
            sys.@float = this.typeFloat;
            sys.func = this.typeFunc;
            sys.@class = this.typeClass;
            sys.selector = this.typeSelector;
            sys.@string = this.typeString;
            sys.@bool = this.typeBool;
            sys.@array = this.typeArr;
            Store.sys = sys;
            Store.@null = Null.NULL;

            Store.@int = new Reference(this, "sys.int");
            Store.@float = new Reference(this, "sys.float");
            Store.func = new Reference(this, "sys.func");
            Store.@class = new Reference(this, "sys.class");
            Store.selector = new Reference(this, "sys.selector");
            Store.@string = new Reference(this, "sys.string");
            Store.@bool = new Reference(this, "sys.bool");
            Store.@array = new Reference(this, "sys.array");

            Store.global = Store;
            Store.fullName = new List<string> { "" };

            Store.alert = new Function((self, args) => {
                if (args[0] == null) System.Windows.MessageBox.Show("INVALID : NULL");
                else System.Windows.MessageBox.Show((args[0]).ToString());
                return args[0];
            }, new List<Pair>() {
                new Pair(new Types.String("data"), new Types.String("any"))
            })
            {
                name = "alert",
                fullName = { "alert" }
            };

            Store.dir = new Function((self, args) => {
                string report = "directory : [" + args[0].fullName + "] " + args[0].ToString() + "\n\n";
                if (args[0] is ExpandoObject exp) {
                    foreach (var item in (IDictionary<string, object>)exp) {
                        report += "  [" + item.Key + "] {" + item.Value.ToString() + "}\n";
                    }
                } else {
                    foreach (var item in (Dictionary<string, object>)(args[0]._fields)) {
                        report += "  [" + item.Key + "] {" + item.Value.ToString() + "}\n";
                    }

                    report += "\nfunction cached :\n";
                    foreach (var item in (List<Function>)(FunctionCache[args[0].type])) {
                        report += "  [" + item.name + "] {" + item.ToString() + "}\n";
                    }
                }

                System.Windows.MessageBox.Show(report);
                return Types.Null.NULL;
            }, new List<Pair>() {
                new Pair(new Types.String("data"), new Types.String("any"))
            })
            {
                name = "dir",
                fullName = { "dir" }
            };

            Store.@ref = new Function((self, args) => {
                return new Reference(this, args[0].fullName[0]);
            }, new List<Pair>() {
                new Pair(new Types.String("referencePath"), new Types.String("sys.string"))
            })
            {
                name = "ref",
                fullName = { "ref" }
            };

            CacheFunction("sys.int", typeof(Integer));
            CacheFunction("sys.float", typeof(Float));
            CacheFunction("sys.func", typeof(Function));
            CacheFunction("sys.class", typeof(Class));
            CacheFunction("sys.selector", typeof(Selector));
            CacheFunction("sys.string", typeof(Simula.Scripting.Types.String));
            CacheFunction("sys.bool", typeof(Simula.Scripting.Types.Boolean));
            CacheFunction("sys.array", typeof(Simula.Scripting.Types.Array));
        }

        public dynamic Store = new ExpandoObject();
        public Dictionary<string, List<Function>> FunctionCache = new Dictionary<string, List<Function>>();
        public List<ScopeContext> Scopes = new List<ScopeContext>();
        public static Dictionary<string, Operator> Registry = new Dictionary<string, Operator>() {
            {"_multiply", new Operator("*")},
            {"_divide", new Operator("/")},
            {"_mod", new Operator("%")},
            {"_add", new Operator("+")},
            {"_substract", new Operator("-")},
            {"_lt", new Operator("<")},
            {"_lte", new Operator("<=")},
            {"_gt", new Operator(">")},
            {"_gte", new Operator(">=")},
            {"_equals", new Operator("==")},
            {"_notequals", new Operator("!=")},
            {"_or", new Operator("||")},
            {"_and", new Operator("&&")},
            {"_assign", new Operator("=")},
            {"_addassign", new Operator("+=") }
        };

        public Null typeNull = Null.NULL;
        public Class typeClass = new Class(typeof(Class)) { fullName = { "sys.class" }, name = "class" };
        public Class typeFunc = new Class(typeof(Function)) { fullName = { "sys.func" }, name = "func" };
        public Class typeSelector = new Class(typeof(Selector)) { fullName = { "sys.selector" }, name = "selector" };
        public Class typeInt = new Class(typeof(Integer)) { fullName = { "sys.int" }, name = "int" };
        public Class typeFloat = new Class(typeof(Float)) { fullName = { "sys.float" }, name = "float" };
        public Class typeBool = new Class(typeof(Types.Boolean)) { fullName = { "sys.bool" }, name = "bool" };
        public Class typeString = new Class(typeof(Types.String)) { fullName = { "sys.string" }, name = "string" };
        public Class typeArr = new Class(typeof(Types.Array)) { fullName = { "sys.array" }, name = "array" };

        public void CacheFunction(string alias, Type type)
        {
            if (FunctionCache.ContainsKey(alias)) FunctionCache.Remove(alias);
            List<Function> functions = new List<Function>();
            foreach (var field in type.GetFields()) {
                if (field.IsStatic) {
                    if (field.GetValue(null) is Function f) {
                        f.fullName.Insert(0, alias + "." + field.Name);
                        f.name = field.Name;
                        functions.Add(f);
                    }
                }
            }

            FunctionCache.Add(alias, functions);
        }

        public dynamic GetMember(string name)
        {
            int counter = this.Scopes.Count;
            if(Scopes.Count > 0) {
                bool turn = true;
                counter--;
                while(turn && counter >= 0) {
                    var dict = (IDictionary<string, object>)(Scopes[counter].Store);
                    counter--;
                    if (dict.ContainsKey(name)) return dict[name];
                    if (Scopes[counter + 1].Permeable) turn = true;
                    else return Null.NULL;
                }
            }

            var library = (IDictionary<string, object>)Store;
            if (library.ContainsKey(name)) return library[name];
            else return Null.NULL;
        }

        public (dynamic Container, dynamic Member) GetMemberWithContainer(string name)
        {
            int counter = this.Scopes.Count;
            if (Scopes.Count > 0) {
                bool turn = true;
                counter--;
                while (turn && counter >= 0) {
                    var dict = (IDictionary<string, object>)(Scopes[counter].Store);
                    counter--;
                    if (dict.ContainsKey(name)) return ((Scopes[counter + 1].Store), dict[name]);
                    if (Scopes[counter + 1].Permeable) turn = true;
                    else return (this.Store, Null.NULL);
                }
            }

            var library = (IDictionary<string, object>)Store;
            if (library.ContainsKey(name)) return (this.Store, library[name]);
            else return (this.Store, Null.NULL);
        }

        public dynamic SetMember(string name, dynamic value)
        {
            if (Scopes.Count > 0) {
                var current = Scopes[Scopes.Count - 1];
                var dict = (IDictionary<string, object>)(current.Store);
                dict[name] = value;
            } else {
                var dict = (IDictionary<string, object>)Store;
                dict[name] = value;
            }

            return value;
        }

        public dynamic SetMemberReferenceCheck(string name, dynamic value)
        {
            if (Scopes.Count > 0) {
                var current = Scopes[Scopes.Count - 1];
                var dict = (IDictionary<string, object>)(current.Store);

                if (dict.ContainsKey(name)) {
                    if (dict[name] is Reference re) {
                        var container = re.Container;
                        while (container is Reference containerRef)
                            container = containerRef.Container;

                        string positioner = ((container.fullName[0] == "") ? "" : (container.fullName[0] + ".")) + re.Token;
                        value.fullName.Insert(0, positioner);

                        if (container is ExpandoObject obj) {
                            var dictionary = (IDictionary<string, object>)obj;
                            dynamic original = dictionary[re.Token];
                            original.fullName.Remove(positioner);
                            dictionary[re.Token] = value;
                        } else {
                            if (container._fields.ContainsKey(re.Token)) {
                                dynamic original = container._fields[re.Token];
                                original.fullName.Remove(positioner);
                                container._fields[re.Token] = value;
                            } else container._fields.Add(re.Token, value);
                        }
                    } else {
                        dynamic original = dict[name];
                        string positioner = ((current.Store.fullName[0] == "") ? "" : (current.Store.fullName[0] + ".")) + name;
                        original.fullName.Remove(positioner);
                        value.fullName.Insert(0, positioner);
                        dict[name] = value;
                    }
                } else {
                    string positioner = ((current.Store.fullName[0] == "") ? "" : (current.Store.fullName[0] + ".")) + name;
                    if (!(value is Reference))
                        value.fullName.Insert(0, positioner);
                    dict[name] = value; 
                }

            } else {
                var dict = (IDictionary<string, object>)Store;

                if (dict.ContainsKey(name)) {
                    if (dict[name] is Reference re) {
                        var container = re.Container;
                        while (container is Reference containerRef)
                            container = containerRef.Container;

                        string positioner = ((container.fullName[0] == "") ? "" : (container.fullName[0] + ".")) + re.Token;
                        value.fullName.Insert(0, positioner);

                        if (container is ExpandoObject obj) {
                            var dictionary = (IDictionary<string, object>)obj;
                            dynamic original = dictionary[re.Token];
                            original.fullName.Remove(positioner);
                            dictionary[re.Token] = value;
                        } else {
                            if (container._fields.ContainsKey(re.Token)) {
                                dynamic original = container._fields[re.Token];
                                original.fullName.Remove(positioner);
                                container._fields[re.Token] = value;
                            } else container._fields.Add(re.Token, value);
                        }
                    } else {
                        dynamic original = dict[name];
                        string positioner = ((Store.fullName[0] == "") ? "" : (Store.fullName[0] + ".")) + name;
                        original.fullName.Remove(positioner);
                        value.fullName.Insert(0, positioner);
                        dict[name] = value;
                    }
                } else {
                    string positioner = ((Store.fullName[0] == "") ? "" : (Store.fullName[0] + ".")) + name;
                    if (!(value is Reference))
                        value.fullName.Insert(0, positioner);
                    dict[name] = value;
                }

            }

            return value;
        }

        public dynamic SetMemberReferenceCheck(dynamic expando, string name, dynamic value, string space)
        {
            if (expando is ExpandoObject) {
                var dict = (IDictionary<string, object>)expando;
                if (dict.ContainsKey(name)) {
                    if (dict[name] is Reference re) {
                        var container = re.Container;
                        while (container is Reference containerRef)
                            container = containerRef.Container;

                        string positioner = ((container.fullName[0] == "") ? "" : (container.fullName[0] + ".")) + re.Token;
                        value.fullName.Insert(0, positioner);

                        if (container is ExpandoObject obj) {
                            var dictionary = (IDictionary<string, object>)obj;
                            dynamic original = dictionary[re.Token];
                            original.fullName.Remove(positioner);
                            dictionary[re.Token] = value;
                        } else {
                            if (container._fields.ContainsKey(re.Token)) {
                                dynamic original = container._fields[re.Token];
                                original.fullName.Remove(positioner);
                                container._fields[re.Token] = value;
                            } else container._fields.Add(re.Token, value);
                        }
                    } else {
                        dynamic original = dict[name];
                        string positioner = ((expando.fullName[0] == "") ? "" : (expando.fullName[0] + ".")) + name;
                        original.fullName.Remove(positioner);
                        value.fullName.Insert(0, positioner);
                        dict[name] = value;
                    }
                } else {
                    string positioner = ((expando.fullName[0] == "") ? "" : (expando.fullName[0] + ".")) + name;
                    if (!(value is Reference))
                        value.fullName.Insert(0, positioner);
                    dict[name] = value;
                }

            } else if(expando is Dictionary<string, dynamic> dict) {
                if (dict.ContainsKey(name)) {
                    if (dict[name] is Reference re) {
                        var container = re.Container;
                        while (container is Reference containerRef)
                            container = containerRef.Container;

                        string positioner = ((container.fullName[0] == "") ? "" : (container.fullName[0] + ".")) + re.Token;
                        value.fullName.Insert(0, positioner);

                        if (container is ExpandoObject obj) {
                            var dictionary = (IDictionary<string, object>)obj;
                            dynamic original = dictionary[re.Token];
                            original.fullName.Remove(positioner);
                            dictionary[re.Token] = value;
                        } else {
                            if (container._fields.ContainsKey(re.Token)) {
                                dynamic original = container._fields[re.Token];
                                original.fullName.Remove(positioner);
                                container._fields[re.Token] = value;
                            } else container._fields.Add(re.Token, value);
                        }
                    } else {
                        dynamic original = dict[name];
                        string positioner = ((space == "") ? "" : (space + ".")) + name;
                        original.fullName.Remove(positioner);
                        value.fullName.Insert(0, positioner);
                        dict[name] = value;
                    }
                } else {
                    string positioner = ((space == "") ? "" : (space + ".")) + name;
                    if (!(value is Reference))
                        value.fullName.Insert(0, positioner);
                    dict[name] = value;
                }
            }

            return value;
        }
    }
}