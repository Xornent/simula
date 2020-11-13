using Simula.Scripting.Debugging;
using Simula.Scripting.Reflection;
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

        public override ExecutionResult Operate(Compilation.RuntimeContext ctx) {
            var operate = GetResultWithoutInheritage(ctx);
            if(operate.Result is ClrInstance inst) {
                if(inst.Parent == null) {
                    inst.Parent = ClrClass.Create(inst.Reflection?.GetType() ?? typeof(Type.Var), ref ctx);
                    var cls = inst.Parent;
                    if(cls.Inheritage != null) {
                        inst.ParentalInstance = cls.CreateInstance(new List<Member>(), ref ctx);
                    }
                }
            }
            return operate;
        }

        public ExecutionResult GetResultWithoutInheritage(Compilation.RuntimeContext ctx) {

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
                    
                return new ExecutionResult(new ClrInstance(s, ref ctx) , ctx);
            }

            if(raw.ToLower() == "true") return new ExecutionResult(new ClrInstance(new Type.Boolean("true"), ref ctx), ctx);
            if(raw.ToLower() == "false") return new ExecutionResult(new ClrInstance(new Type.Boolean("false"), ref ctx), ctx);

            System.Numerics.BigInteger tempInt;
            bool successInt = System.Numerics.BigInteger.TryParse(raw, out tempInt);
            if (successInt) {
                Type.Integer i = tempInt;
                return new ExecutionResult(new ClrInstance(i, ref ctx), ctx);
            }

            float tempFloat;
            bool successFloat = float.TryParse(raw, out tempFloat);
            if (successFloat) {
                Type.Float f = tempFloat;
                return new ExecutionResult(new ClrInstance(f, ref ctx), ctx);
            }

            var exec = ctx.CallStack.Peek().GetMember(Self.Value.Replace("\n",""));
            if(exec.Result.Type == MemberType.Instance) {
                var selfFunction = ((Instance)exec.Result).GetMember("_self");
                if(selfFunction.Pointer != 0) {
                    if(selfFunction.Result.Type == MemberType.Function)
                        return ((Function)selfFunction.Result).Invoke(new List<Member>(), ref ctx);
                    else return exec;
                } else return exec;
            } else return exec;
        } 
    }
}
