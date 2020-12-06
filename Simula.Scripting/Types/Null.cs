using System.Dynamic;
using System.Collections.Generic;

namespace Simula.Scripting.Types
{
    public class Null : DynamicObject
    {
        public static Null NULL = new Null();
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            switch (binder.Name) {
                case "type":
                    result = new String("null");
                    return true;
                default:
                    result = NULL;
                    return true;
            }
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            return true;
        }

        public override string ToString()
        {
            return "null";
        }
    }

    public class Var
    {
        internal string name;
        internal string type;
        internal Dictionary<string, dynamic> _fields = new Dictionary<string, dynamic>();
    }
}