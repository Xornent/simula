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
        private Func<dynamic, dynamic[], dynamic>[] raw = new Func<dynamic, dynamic[], dynamic>[] { };
        public Function() 
        {
            this._fields.Add("isMultiple", false);
        }

        public Function(Func<dynamic, dynamic[], dynamic> function) : this()
        {
            this.raw = new Func<dynamic, dynamic[], dynamic>[] { function };
        }

        public Function(Func<dynamic, dynamic[], dynamic>[] functions) : this()
        {
            var list = raw.ToList();
            list.AddRange(functions);
            if (list.Count > 1)
                this._fields["isMultiple"] = new Boolean(true);
            raw = list.ToArray();
        }

        public Function(Function function)
        {
            this.raw = function.raw;
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
        internal new string type = "func";

        public override string ToString()
        {
            return "func";
        }
    }

    public class Parameter : Var
    {
        internal new string type = "param";
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

        internal new string type = "pair";
    }
}
