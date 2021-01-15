﻿using System;
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
        internal List<Func<dynamic, dynamic[], dynamic>> raw = new List<Func<dynamic, dynamic[], dynamic>> { };
        internal List<List<Pair>> param = new List<List<Pair>>();
        public Function() : base()
        {
            this._fields.Add("isMultiple", false);
        }

        public Function(Func<dynamic, dynamic[], dynamic> function,
                        List<Pair> pairs) : this()
        {
            this.raw.Add(function);
            param.Add(pairs);
        }

        public Function(List<Func<dynamic, dynamic[], dynamic>> functions,
                        List<List<Pair>> pairs) : this()
        {
            raw.AddRange(functions);
            param.AddRange(pairs);
        }

        public Function(Function function) : base()
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
            string expr = "";
            for(int id = 0; id<this.raw.Count; id++) {
                var pair = param[id];
                List<string> paramList = new List<string>();
                foreach (var item in pair) {
                    paramList.Add(item.ToString());
                }

                expr += "func " + this.name + " (" + paramList.JoinString(", ") + ")\n";
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
