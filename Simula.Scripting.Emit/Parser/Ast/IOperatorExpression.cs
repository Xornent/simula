using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Parser.Ast
{
    public interface IOperatorExpression : IExpression
    {
        Operator Operator { get; set; }
    }

    // the operator precedence in the enum Operator is set by the integral value of enum items.
    // the less the value is, the precedence is smaller, and it is interpreted later.
    
    public class Operator
    {
        public Operator() { }
        public Operator(string symbol, string method = "", OperatorType type = OperatorType.Binary)
        {
            this.Symbol = symbol;
            this.Type = type;
            this.Method = method;
        }

        public string Method { get; set; } = "";
        public string Symbol { get; set; } = "";
        public OperatorType Type { get; set; }

        public static Dictionary<string, Operator> Operators = new Dictionary<string, Operator>()
        {
            {"_deref", new Operator("*", "deref", OperatorType.UnaryLeft) },
            {"_lincrement", new Operator("++", "increment", OperatorType.UnaryLeft) },
            {"_ldecrement", new Operator("--", "decrement", OperatorType.UnaryLeft) },
            {"_inverse", new Operator("-", "inverse", OperatorType.UnaryLeft) },
            {"_transpos", new Operator("^", "transpos", OperatorType.UnaryRight) },
            {"_not", new Operator("!", "not", OperatorType.UnaryLeft) },
            {"_rincrement", new Operator("++", "increment", OperatorType.UnaryRight) },
            {"_rdecrement", new Operator("--", "decrement", OperatorType.UnaryRight) },
            {"_multiply", new Operator("*", "multiply")},
            {"_pow", new Operator("^", "pow") },
            {"_divide", new Operator("/", "divide")},
            {"_mod", new Operator("%", "mod")},
            {"_add", new Operator("+", "add")},
            {"_substract", new Operator("-", "substract")},

            // bitwise operators and class logic operators

            {"_exclude", new Operator("!", "exclude", OperatorType.Binary) },
            {"_bitand", new Operator("&", "bitand", OperatorType.Binary) },
            {"_bitor", new Operator("|", "bitor", OperatorType.Binary) },

            // type casting and object transfer operators

            // a compatibility operator operate on two datatypes, or a data and a datetype:
            // when it is used as 'type1 ~> type2', it returns a logical value indicating whether the
            // 'type1' is a superset of 'type2'. or 'object1 ~> type2', when 'object1' is an instance
            // of the 'type2' returns true, and otherwise false.

            // a cast operator cast the object into the type specified pointed by the arrow. for example
            // when coding 'object1 -> type2', the operator returns an instance object of 'type2' if 
            // the 'object1' is compatible to 'type2', otherwise it throws an error. (this step should
            // be completed in the static analysis period)

            {"_lcompatible", new Operator("~>", "compatible", OperatorType.Binary) },
            {"_rcompatible", new Operator("<~", "compatible", OperatorType.Binary) },
            {"_lcast", new Operator("->", "cast", OperatorType.Binary) },
            {"_rcast", new Operator("<-", "cast", OperatorType.Binary) },

            {"_lt", new Operator("<", "lt")},
            {"_lte", new Operator("<=", "lte")},
            {"_gt", new Operator(">", "gt")},
            {"_gte", new Operator(">=", "gte")},
            {"_equals", new Operator("==", "equals")},
            {"_notequals", new Operator("!=", "notequals")},
            {"_or", new Operator("||", "or")},
            {"_and", new Operator("&&", "and")},
            {"_compose", new Operator("|>", "compose")}
        };

        public static Operator MemberOperator = new Operator(".", "member", OperatorType.Binary);
        public static Operator AssignmentOperator = new Operator("=", "assign", OperatorType.Binary);
    }

    public enum OperatorType
    {
        UnaryLeft,
        UnaryRight,
        Binary
    }
}
