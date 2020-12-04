using Simula.Scripting.Syntax;
using System.Collections.Generic;
using System.Dynamic;
using Simula.Scripting.Types;

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
        }

        public dynamic Store = new ExpandoObject();
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
            {"_assign", new Operator("=")}
        };
    }
}