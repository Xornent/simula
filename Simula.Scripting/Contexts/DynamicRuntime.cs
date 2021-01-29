using Simula.Scripting.Syntax;
using System.Collections.Generic;
using System.Dynamic;
using Simula.Scripting.Types;
using System;
using System.Reflection;
using System.IO;
using System.Linq;

using static Simula.Scripting.Resources;

namespace Simula.Scripting.Contexts
{
    public class DynamicRuntime
    {
        public DynamicRuntime()
        {
            dynamic sys = new ExpandoObject();
            sys.fullName = new List<string> { "sys" };
            sys.@double = this.typeFloat;
            sys.func = this.typeFunc;
            sys.@class = this.typeClass;
            sys.@string = this.typeString;
            sys.@bool = this.typeBool;
            sys.@matrix = this.typeMat;
            sys.@uint8 = this.typeByte;
            sys.@uint16 = this.typeChar;
            sys.@uint32 = this.typeUint32;
            sys.@uint64 = this.typeUint64;
            sys.@int8 = this.typeInt8;
            sys.@int16 = this.typeInt16;
            sys.@int32 = this.typeInt32;
            sys.@int64 = this.typeInt64;
            sys.addr = this.typeAddr;

            Store.sys = sys;
            Store.@null = Null.NULL;

            Store.@double = new Reference(this, "sys.double");
            Store.func = new Reference(this, "sys.func");
            Store.@class = new Reference(this, "sys.class");
            Store.@string = new Reference(this, "sys.string");
            Store.@bool = new Reference(this, "sys.bool");
            Store.matrix = new Reference(this, "sys.matrix");

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
                fullName = { "alert" },
                returntypes = new HashSet<string>() { "any" },
                desc = Loc(StringTableIndex.Doc_Sys_Alert)
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
                fullName = { "dir" },
                returntypes = new HashSet<string>() { "null" },
                desc = Loc(StringTableIndex.Doc_Sys_Dir)
            };

            Store.@ref = new Function((self, args) => {
                if (args[0].fullName.Count != 0) {
                    return new Reference(this, args[0].fullName[0]);
                } else {
                    this.PostRuntimeError(StringTableIndex.AnnonymousTypeNameReferenceFail);
                    return Null.NULL;
                }
            }, new List<Pair>() {
                new Pair(new Types.String("referencePath"), new Types.String("sys.string"))
            })
            {
                name = "ref",
                fullName = { "ref" },
                returntypes = new HashSet<string>() { "ref" },
                desc = Loc(StringTableIndex.Doc_Sys_Ref)
            };

            Store.@uint8 = new Function((self, args) => {
                try {
                    if (args[0] is INumericalMatrix inum) {
                        return inum.ToByteMatrix();
                    } else return new Types.Byte(Convert.ToByte(args[0].raw));
                } catch (InvalidCastException invalidCast) {
                    this.PostRuntimeError(StringTableIndex.IntegralCastOutOfRange);
                    return new Types.Byte(0);
                }
            }, new List<Pair>() {
                new Pair(new Types.String("numeric"), new Types.String("any"))
            })
            {
                name = "uint8",
                fullName = { "uint8" },
                returntypes = new HashSet<string>() { "sys.uint8" },
                desc = Loc(StringTableIndex.Doc_Sys_Uint8)
            };

            Store.addr = new Function((self, args) => {
                try {
                    return new Addr(args[0]);
                } catch (ArgumentException argEx) {
                    this.PostRuntimeError(StringTableIndex.NovolatileMemberGetAddrFail);
                    return new Addr();
                }
            }, new List<Pair>() {
                new Pair(new Types.String("obj"), new Types.String("any"))
            })
            {
                name = "addr",
                fullName = { "addr" },
                returntypes = new HashSet<string>() { "any" },
                desc = Loc(StringTableIndex.Doc_Sys_Addr)
            };

            CacheFunction("sys.double", typeof(Types.Double));
            CacheFunction("sys.func", typeof(Function));
            CacheFunction("sys.class", typeof(Class));
            CacheFunction("sys.string", typeof(Simula.Scripting.Types.String));
            CacheFunction("sys.bool", typeof(Simula.Scripting.Types.Boolean));
            CacheFunction("sys.array", typeof(Simula.Scripting.Types.Array));
            CacheFunction("sys.matrix", typeof(Simula.Scripting.Types.Matrix));

            CacheFunction("sys.uint16", typeof(Simula.Scripting.Types.Char));
            CacheFunction("sys.uint8", typeof(Simula.Scripting.Types.Byte));
            CacheFunction("sys.uint32", typeof(Simula.Scripting.Types.UInt32));
            CacheFunction("sys.uint64", typeof(Simula.Scripting.Types.UInt64));
            CacheFunction("sys.int8", typeof(Simula.Scripting.Types.Int8));
            CacheFunction("sys.int16", typeof(Simula.Scripting.Types.Int16));
            CacheFunction("sys.int32", typeof(Simula.Scripting.Types.Int32));
            CacheFunction("sys.int64", typeof(Simula.Scripting.Types.Int64));

            CacheFunction("sys.addr", typeof(Simula.Scripting.Types.Addr));

            // the runtime context begins searching gor add-on units, the default add-ons
            // should be in the same directory as the executable and with extensions ".sdl"

            DirectoryInfo dir = new DirectoryInfo(Environment.CurrentDirectory + @"\libraries\");
            foreach (var files in dir.GetFiles()) {
                if (files.Extension.EndsWith("dll")) {
                    Dom.Library lib = new Dom.Library(files.FullName);
                    lib.LoadDefinition(this);
                }
            }
        }

        public dynamic Store = new ExpandoObject();
        public Dictionary<string, List<Function>> FunctionCache = new Dictionary<string, List<Function>>();
        public List<ScopeContext> Scopes = new List<ScopeContext>();
        public static Dictionary<string, Operator> Registry = new Dictionary<string, Operator>() {
            {"_deref", new Operator((Token.Token)"*", OperatorType.UnaryLeft) },
            {"_lincrement", new Operator((Token.Token)"++", OperatorType.UnaryLeft) },
            {"_ldecrement", new Operator((Token.Token)"--", OperatorType.UnaryLeft) },
            {"_inverse", new Operator((Token.Token)"-", OperatorType.UnaryLeft) },
            {"_transpos", new Operator((Token.Token)"^", OperatorType.UnaryRight) },
            {"_not", new Operator((Token.Token)"!", OperatorType.UnaryLeft) },
            {"_rincrement", new Operator((Token.Token)"++", OperatorType.UnaryRight) },
            {"_rdecrement", new Operator((Token.Token)"--", OperatorType.UnaryRight) },
            {"_multiply", new Operator((Token.Token)"*")},
            {"_pow", new Operator((Token.Token)"^") },
            {"_divide", new Operator((Token.Token)"/")},
            {"_mod", new Operator((Token.Token)"%")},
            {"_add", new Operator((Token.Token)"+")},
            {"_substract", new Operator((Token.Token)"-")},

            {"_lt", new Operator((Token.Token)"<")},
            {"_lte", new Operator((Token.Token)"<=")},
            {"_gt", new Operator((Token.Token)">")},
            {"_gte", new Operator((Token.Token)">=")},
            {"_equals", new Operator((Token.Token)"==")},
            {"_notequals", new Operator((Token.Token)"!=")},
            {"_or", new Operator((Token.Token)"||")},
            {"_and", new Operator((Token.Token)"&&")},

            {"_assign", new Operator((Token.Token)"=")},
            {"_addassign", new Operator((Token.Token)"+=") },
            {"_substractassign", new Operator((Token.Token)"-=") },
            {"_powassign", new Operator((Token.Token)"^=") },
            {"_multiplyassign", new Operator((Token.Token)"*=") },
            {"_divideassign", new Operator((Token.Token)"/=") },
            {"_modassign", new Operator((Token.Token)"%=") }
        };

        public Null typeNull = Null.NULL;
        public Class typeClass = new Class(typeof(Class)) { fullName = { "sys.class" }, name = "class" };
        public Class typeFunc = new Class(typeof(Function)) { fullName = { "sys.func" }, name = "func" };
        public Class typeFloat = new Class(typeof(Types.Double)) { fullName = { "sys.double" }, name = "double" };
        public Class typeBool = new Class(typeof(Types.Boolean)) { fullName = { "sys.bool" }, name = "bool" };
        public Class typeString = new Class(typeof(Types.String)) { fullName = { "sys.string" }, name = "string" };
        public Class typeArr = new Class(typeof(Types.Array)) { fullName = { "sys.array" }, name = "array" };
        public Class typeMat = new Class(typeof(Types.Matrix)) { fullName = { "sys.matrix" }, name = "matrix" };

        public Class typeByte = new Class(typeof(Types.Byte)) { fullName = { "sys.uint8" }, name = "uint8" };
        public Class typeChar = new Class(typeof(Types.Char)) { fullName = { "sys.uint16" }, name = "uint16" };
        public Class typeUint32 = new Class(typeof(Types.UInt32)) { fullName = { "sys.uint32" }, name = "uint32" };
        public Class typeUint64 = new Class(typeof(Types.UInt64)) { fullName = { "sys.uint64" }, name = "uint64" };
        public Class typeInt8 = new Class(typeof(Types.Int8)) { fullName = { "sys.int8" }, name = "int8" };
        public Class typeInt16 = new Class(typeof(Types.Int16)) { fullName = { "sys.int16" }, name = "int16" };
        public Class typeInt32 = new Class(typeof(Types.Int32)) { fullName = { "sys.int32" }, name = "int32" };
        public Class typeInt64 = new Class(typeof(Types.Int64)) { fullName = { "sys.int64" }, name = "int64" };

        public Class typeAddr = new Class(typeof(Types.Addr)) { fullName = { "sys.addr" }, name = "addr" };

        public void CacheFunction(string alias, Type type)
        {
            if (FunctionCache.ContainsKey(alias)) FunctionCache.Remove(alias);
            List<Function> functions = new List<Function>();
            foreach (var field in type.GetFields()) {
                if (field.IsStatic) {

                    // the member functions can have a function export attribute.
                    // if it has, we should obtain the documentation etc... from it.

                    if (field.GetValue(null) is Function f) {
                        f.fullName.Insert(0, alias + "." + field.Name);
                        f.name = field.Name;
                        var attr = field.GetCustomAttribute<Dom.FunctionExportAttribute>();
                        if(attr != null) {
                            f.desc = attr.Description;
                            f.param = new List<List<Pair>> { attr.Pairs };
                            f.returntypes = attr.Returns.Contains("|") ? attr.Returns.Split("|").ToHashSet() : new HashSet<string>() { attr.Returns };
                        }

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

                        if (container != null) {
                            if (container.fullName.Count != 0) {
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

                                // the container is nameless. this is common when executing annonymous objects 
                                // of user-defined types. the following code produced a circumstance:

                                //     cls().value = 1
                                // where cls is an user-defined class.

                                if (container is ExpandoObject obj) {
                                    var dictionary = (IDictionary<string, object>)obj;
                                    dynamic original = dictionary[re.Token];
                                    dictionary[re.Token] = value;
                                } else {
                                    if (container._fields.ContainsKey(re.Token)) {
                                        dynamic original = container._fields[re.Token];
                                        container._fields[re.Token] = value;
                                    } else container._fields.Add(re.Token, value);
                                }
                            }

                        } else {
                            this.PostRuntimeError(StringTableIndex.NullContainer);
                            return value;
                        }

                    } else {

                        // if the scope has no items of its name, we should set the object
                        // directly to the parent scope. a scope contains a default fullName as "".

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

                        if (container != null) {
                            if (container.fullName.Count > 0) {
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
                                if (container is ExpandoObject obj) {
                                    var dictionary = (IDictionary<string, object>)obj;
                                    dynamic original = dictionary[re.Token];
                                    dictionary[re.Token] = value;
                                } else {
                                    if (container._fields.ContainsKey(re.Token)) {
                                        dynamic original = container._fields[re.Token];
                                        container._fields[re.Token] = value;
                                    } else container._fields.Add(re.Token, value);
                                }
                            }

                        } else {
                            this.PostRuntimeError(StringTableIndex.NullContainer);
                            return value;
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

                        if (container != null) {
                            if (container.fullName.Count > 0) {
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
                                if (container is ExpandoObject obj) {
                                    var dictionary = (IDictionary<string, object>)obj;
                                    dynamic original = dictionary[re.Token];
                                    dictionary[re.Token] = value;
                                } else {
                                    if (container._fields.ContainsKey(re.Token)) {
                                        dynamic original = container._fields[re.Token];
                                        container._fields[re.Token] = value;
                                    } else container._fields.Add(re.Token, value);
                                }
                            }

                        } else {
                            this.PostRuntimeError(StringTableIndex.NullContainer);
                            return value;
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

                        if (container != null) {
                            if (container.fullName.Count > 0) {
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
                                if (container is ExpandoObject obj) {
                                    var dictionary = (IDictionary<string, object>)obj;
                                    dynamic original = dictionary[re.Token];
                                    dictionary[re.Token] = value;
                                } else {
                                    if (container._fields.ContainsKey(re.Token)) {
                                        dynamic original = container._fields[re.Token];
                                        container._fields[re.Token] = value;
                                    } else container._fields.Add(re.Token, value);
                                }
                            }

                        } else {
                            this.PostRuntimeError(StringTableIndex.NullContainer);
                            return value;
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

        public void PostRuntimeError(string code = "ss0000", string message = "", Exception? inner = null)
        {
            System.Windows.MessageBox.Show(message);
        }

        public void PostRuntimeError(StringTableIndex index)
        {
            System.Windows.MessageBox.Show(Resources.StringTable[index]);
        }

        public static void PostExecutionError(StringTableIndex index)
        {
            System.Windows.MessageBox.Show(Resources.StringTable[index]);
        }
    }
}