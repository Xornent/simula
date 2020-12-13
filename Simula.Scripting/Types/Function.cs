using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace Simula.Scripting.Types
{
    /// <summary>
    /// 对应一个内置类型 func 类. 提供对外接口:
    /// 
    /// addFunction        func (function addition)
    /// removeFunction     func (array :: parameter flag)
    /// select             func (array :: parameter flag)
    /// given              func (array :: pair arguments)
    /// isMultiple         bool
    /// 
    /// +                  operator :: binary (function addition)
    /// -                  operator :: binary (array :: parameter flag)
    /// call               ( ... )
    /// </summary>
    public class Function : Var
    {
        private List<Func<dynamic, dynamic[], dynamic>> raw = new List<Func<dynamic, dynamic[], dynamic>> { };
        private List<List<Pair>> param = new List<List<Pair>>();
        public Function() 
        {
            this._fields.Add("isMultiple", false);
        }

        public Function(Func<dynamic, dynamic[], dynamic> function,
                        List<Pair> pairs) : this()
        {
            this.raw.Add(function);
        }

        public Function(List<Func<dynamic, dynamic[], dynamic>> functions,
                        List<List<Pair>> pairs) : this()
        {
            raw.AddRange(functions);
            param.AddRange(pairs);
        }

        public Function(Function function)
        {
            this.raw = function.raw;
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
            return "func";
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
