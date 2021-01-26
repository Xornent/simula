using System.Dynamic;
using System.Collections.Generic;
using Simula.Scripting.Contexts;
using Simula.Scripting.Token;
using Simula.Scripting.Syntax;
using System.Linq;

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
                case "fullName":
                    result = "null";
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
        public Var()
        {
            // this.fullName.Add(System.Guid.NewGuid().ToString());
        }

        internal List<string> fullName = new List<string>() { };
        internal string name = "";
        public string type = "";
        public string desc = "";
        public Dictionary<string, dynamic> _fields = new Dictionary<string, dynamic>();
    }

    public class Reference
    {
        public dynamic FullName = "";
        public dynamic Container;
        public string Token;
        private DynamicRuntime Runtime;

        public Reference(DynamicRuntime runtime, string fullName)
        {
            this.Runtime = runtime;
            if (fullName.Contains(".")) {
                TokenDocument doc = new TokenDocument();
                doc.Tokenize(fullName);

                if (doc.Tokens.Last() == "<newline>")
                    doc.Tokens.RemoveLast();

                this.Token = doc.Tokens.Last();
                doc.Tokens.RemoveRange(doc.Tokens.Count - 2, 2);
                EvaluationStatement eval = new EvaluationStatement();
                eval.Parse(doc.Tokens);

                string containerPosition = fullName.Remove(fullName.Length - Token.Length - 1, Token.Length + 1);
                this.Container = eval.Execute(runtime).Result;
            } else {
                var result = runtime.GetMemberWithContainer(fullName);
                this.Container = result.Container;
                this.Token = fullName;
            }

            this.FullName = fullName;
        }

        public dynamic GetDynamic()
        {
            dynamic container = this.Container;
            while(container is Reference refer) {
                container = refer.GetDynamic();
            }

            dynamic result = Null.NULL;
            if (container is ExpandoObject expando) {
                IDictionary<string, object> dict = (IDictionary<string, object>)expando;
                if (dict.ContainsKey(Token))
                    result = dict[Token];
            } else {
                if (container._fields.ContainsKey(Token)) {
                    result = container._fields[Token];
                }

                string type = container.type;
                if (!Runtime.FunctionCache.ContainsKey(type)) {
                    Runtime.CacheFunction(type, container.GetType());
                }

                var function = Runtime.FunctionCache[type].Find((func) => {
                    if (func.name == Token) return true;
                    else return false;
                });
                if (function != null) result = function;
            }

            while (result is Reference re) {
                result = result.GetDynamic();
            }

            return result;
        }

        public override string ToString()
        {
            return "<reference> " + this.Container.fullName[0] + "." + this.Token;
        }
    }

    public static class Serializer
    {
        public static int GetSize(dynamic obj)
        {
            if (obj is Boolean) return 1;
            else if (obj is Float) return 8;
            else if (obj is Byte) return 1;
            else if (obj is Char) return 2;
            else if (obj is Matrix mtx) return mtx.elementSize * mtx.total;
            else if (obj is Var v) {
                int size = 0;
                foreach (var item in v._fields) {
                    int elementSize = GetSize(item);
                    if (elementSize == -1) return -1;
                    else size += elementSize;
                }
                return size;
            } else

                // arrays (mixed-type sequence), classes, functions, selectors,
                // and strings are not serializable, because the size of them are
                // not fixed. and any objects containing fields of these types
                // are also not serializable.

                return -1;
        }
    }
}