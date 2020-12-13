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
            sys.fullName = "sys";
            sys.@int = this.typeInt;
            sys.@float = this.typeFloat;
            sys.func = this.typeFunc;
            sys.@class = this.typeClass;
            sys.selector = this.typeSelector;
            sys.@string = this.typeString;
            sys.@bool = this.typeBool;
            sys.@array = this.typeArr;
            Store.sys = sys;

            Store.global = Store;
            Store.fullName = "";

            Store.alert = new Function((self, args) => {
                if (args[0] == null) System.Windows.MessageBox.Show("INVALID : NULL");
                else System.Windows.MessageBox.Show((args[0]).ToString());
                return args[0];
            }, new List<Pair>() { 
                new Pair(new Types.String("data"), Null.NULL)
            })
            {
                name = "alert",
                fullName = "alert"
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

                    report += "function cached :\n";
                    foreach (var item in (List<Function>)(FunctionCache[args[0].type])) {
                        report += "  [" + item.name + "] {" + item.ToString() + "}\n";
                    }
                }

                System.Windows.MessageBox.Show(report);
                return Types.Null.NULL;
            }, new List<Pair>() {
                new Pair(new Types.String("data"), Null.NULL)
            })
            {
                name = "dir",
                fullName = "dir"
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

        public Class typeNull = new Class(typeof(Null)) { fullName = "null", name = "null" };
        public Class typeClass = new Class(typeof(Class)) { fullName = "sys.class", name = "class" };
        public Class typeFunc = new Class(typeof(Function)) { fullName = "sys.func", name = "func" };
        public Class typeSelector = new Class(typeof(Selector)) { fullName = "sys.selector", name = "selector" };
        public Class typeInt = new Class(typeof(Integer)) { fullName = "sys.int", name = "int" };
        public Class typeFloat = new Class(typeof(Float)) { fullName = "sys.float", name = "float" };
        public Class typeBool = new Class(typeof(Types.Boolean)) { fullName = "sys.bool", name = "bool" };
        public Class typeString = new Class(typeof(Types.String)) { fullName = "sys.string", name = "string" };
        public Class typeArr = new Class(typeof(Types.Array)) { fullName = "sys.array", name = "array" };

        public void CacheFunction(string alias, Type type)
        {
            if (FunctionCache.ContainsKey(alias)) FunctionCache.Remove(alias);
            List<Function> functions = new List<Function>();
            foreach(var field in type.GetFields()) {
                if(field.IsStatic) {
                    if (field.GetValue(null) is Function f) {
                        f.name = field.Name;
                        functions.Add(f);
                    }
                }
            }

            FunctionCache.Add(alias, functions);
        }
    }
}