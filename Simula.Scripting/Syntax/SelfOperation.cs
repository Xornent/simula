using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Syntax {
    public class SelfOperation : OperatorStatement {
        public Token.Token Self { get; set; } = new Token.Token("");

        // 本值运算是从一个令牌直接转化成一个解释的值的运算, 相当于解释器内部直接规定的
        // 一组隐式转换流程, 观察以下的几个令牌:

        // 17                   : 在这里被解析成 Type.Integer
        // 25.12                : 在这里被解析成 Type.Float
        // "iso"                : 在这里被解析成 Type.String
        // "komp\n\""           : 一个有转义字符的 Type.String
        // true                 : 定义在 <global> 中的字段 true
        // system               : 一个 module

        public override dynamic Operate(Compilation.RuntimeContext ctx) {

            // 解析成字符串:

            // 判断是不是字符串

            string raw = this.Self;
            if(raw.StartsWith("\"") && raw.EndsWith("\"") && (!raw.EndsWith("\\\""))) {
                Type.String s = raw
                    .Remove(0,1)
                    .Remove(Self.Value.Length - 2, 1)
                    .Replace("\\\"", "\"")
                    .Replace("\\\\", "\\")
                    .Replace("\\0", "\0")
                    .Replace("\\a", "\a")
                    .Replace("\\b", "\b")
                    .Replace("\\f", "\f")
                    .Replace("\\n", "\n")
                    .Replace("\\r", "\r")
                    .Replace("\\t", "\t")
                    .Replace("\\v", "\v");
                    
                return s;
            }

            System.Numerics.BigInteger tempInt;
            bool successInt = System.Numerics.BigInteger.TryParse(raw, out tempInt);
            if (successInt) {
                Type.Integer i = tempInt;
                return i;
            }

            float tempFloat;
            bool successFloat = float.TryParse(raw, out tempFloat);
            if (successFloat) {
                Type.Float f = tempFloat;
                return f;
            }

            return ctx.GetMember(Self.Value.Replace("\n",""));
        }
    }
}
