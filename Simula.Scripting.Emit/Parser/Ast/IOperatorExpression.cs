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
            {"_lincrement", new Operator("++", "lincrement", OperatorType.UnaryLeft) },
            {"_ldecrement", new Operator("--", "ldecrement", OperatorType.UnaryLeft) },
            {"_inverse", new Operator("-", "inverse", OperatorType.UnaryLeft) },
            {"_transpos", new Operator("^", "transpos", OperatorType.UnaryRight) },
            {"_not", new Operator("!", "not", OperatorType.UnaryLeft) },
            {"_rincrement", new Operator("++", "rincrement", OperatorType.UnaryRight) },
            {"_rdecrement", new Operator("--", "rdecrement", OperatorType.UnaryRight) },
            {"_multiply", new Operator("*", "multiply")},
            {"_pow", new Operator("^", "pow") },
            {"_divide", new Operator("/", "divide")},
            {"_mod", new Operator("%", "mod")},
            {"_add", new Operator("+", "add")},
            {"_substract", new Operator("-", "substract")},

            {"_lt", new Operator("<", "lt")},
            {"_lte", new Operator("<=", "lte")},
            {"_gt", new Operator(">", "gt")},
            {"_gte", new Operator(">=", "gte")},
            {"_equals", new Operator("==", "equals")},
            {"_notequals", new Operator("!=", "notequals")},
            {"_or", new Operator("||", "or")},
            {"_and", new Operator("&&", "and")},
            {"_compose", new Operator("<*>", "compose")}
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
