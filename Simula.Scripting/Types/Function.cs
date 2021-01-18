using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace Simula.Scripting.Types
{
    public class Function : Var
    {
        internal List<Func<dynamic, dynamic[], dynamic>> raw = new List<Func<dynamic, dynamic[], dynamic>> { };
        internal List<List<Pair>> param = new List<List<Pair>>();
        public HashSet<string> returntypes = new HashSet<string>();
        public Function(string returns = "") : base()
        {
            this._fields.Add("isMultiple", new Boolean(false));
            this.returntypes = returns.Contains("|") ? returns.Split("|").ToHashSet() : new HashSet<string>(){ returns };
        }

        public Function(Func<dynamic, dynamic[], dynamic> function,
                        List<Pair> pairs, string returns = "") : this()
        {
            this.raw.Add(function);
            param.Add(pairs);
            this.returntypes = returns.Contains("|") ? returns.Split("|").ToHashSet() : new HashSet<string>() { returns };
        }

        public Function(List<Func<dynamic, dynamic[], dynamic>> functions,
                        List<List<Pair>> pairs, string returns = "") : this()
        {
            raw.AddRange(functions);
            param.AddRange(pairs);
            this.returntypes = returns.Contains("|") ? returns.Split("|").ToHashSet() : new HashSet<string>() { returns };
        }

        public Function(Function function, string returns = "") : base()
        {
            this.raw = function.raw;
            this.returntypes = returns.Contains("|") ? returns.Split("|").ToHashSet() : new HashSet<string>() { returns };
        }

        public void AddFunction(Func<dynamic, dynamic[], dynamic> function, List<Pair> pairs)
        {
            this.raw.Add(function);
            this.param.Add(pairs);
            if (raw.Count > 1)
                this._fields["isMultiple"] = new Boolean(true);
        }

        public static Function addFunction;
        public static Function removeFunction;
        public static Function select;
        public static Function given;

        public dynamic _call(object? sender, dynamic[] parameter) 
        {
            return raw[0](sender ?? Null.NULL, parameter);
        }

        public static Function _substract;
        public static Function _add;

        internal new string name;
        internal new string type = "sys.func";

        public override string ToString()
        {
            string expr = "";
            for(int id = 0; id<this.raw.Count; id++) {
                var pair = param[id];
                List<string> paramList = new List<string>();
                foreach (var item in pair) {
                    paramList.Add(item.ToString());
                }

                expr += "def func " + this.name + " (" + paramList.JoinString(", ") + ")\n";
            }
            expr = expr.Trim('\n');
            return expr == "" ? "func" : expr;
        }
    }

    public class Pair : Var
    {
        public Pair() { }
        public Pair(String memberKey, dynamic member)
        {
            this.key = memberKey;
            this.value = member;
        }

        public String key;
        public dynamic value;

        public override string ToString()
        {
            return value.ToString() + " " + key.ToString();
        }

        internal new string type = "sys.pair";
    }

    [System.AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    sealed class FunctionParameterAttribute : Attribute
    {
        readonly List<Pair> param = new List<Pair>();
        public FunctionParameterAttribute(List<Pair> paramInfo)
        {
            this.param = paramInfo;
        }

        public List<Pair> Parameter {
            get { return param; }
        }
    }
}
