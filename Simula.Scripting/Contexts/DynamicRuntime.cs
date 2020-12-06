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
            sys.@int = Class.typeInt;
            sys.@float = Class.typeFloat;
            sys.func = Class.typeFunc;
            sys.@class = Class.typeClass;
            sys.selector = Class.typeSelector;
            sys.@string = Class.typeString;
            sys.@bool = Class.typeBool;
            sys.@array = Class.typeArr;
            Store.sys = sys;

            Store.@int = Class.typeInt;
            Store.@float = Class.typeFloat;
            Store.func = Class.typeFunc;
            Store.@class = Class.typeClass;
            Store.selector = Class.typeSelector;
            Store.@string = Class.typeString;
            Store.@bool = Class.typeBool;
            Store.@array = Class.typeArr;

            Store.alert = new Function((self, args) => {
                if(args[0] == null) System.Windows.MessageBox.Show("INVALID : NULL");
                else System.Windows.MessageBox.Show((args[0]).ToString());
                return args[0];
            })
            { name = "alert" };

            CacheFunction("int", typeof(Integer));
            CacheFunction("float", typeof(Float));
            CacheFunction("func", typeof(Function));
            CacheFunction("class", typeof(Class));
            CacheFunction("selector", typeof(Selector));
            CacheFunction("string", typeof(Simula.Scripting.Types.String));
            CacheFunction("bool", typeof(Simula.Scripting.Types.Boolean));
            CacheFunction("array", typeof(Simula.Scripting.Types.Array));
        }

        public dynamic Store = new ExpandoObject();
        public static Dictionary<string, List<Function>> FunctionCache = new Dictionary<string, List<Function>>();
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

        public static void CacheFunction(string alias, Type type)
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