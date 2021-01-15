using System.Collections.Generic;
using Simula.Scripting.Contexts;
using Simula.Scripting.Token;
using Simula.Scripting.Types;

namespace Simula.Scripting.Syntax
{
    public class SelfOperation : OperatorStatement
    {
        public Token.Token Self { get; set; }
        public override void Parse(TokenCollection collection)
        {
            this.Self = collection[0];
        }

        public override Execution Operate(DynamicRuntime ctx)
        {
            string raw = this.Self.ToString();
            if (raw.StartsWith("\"") && raw.EndsWith("\"") && (!raw.EndsWith("\\\""))) {
                if (raw.StartsWith("\"$")) {
                    Types.String noEscape = new String(raw.Remove(0, 2).Remove(Self.Value.Length - 3, 1));
                    return new Execution(ctx, noEscape);
                }

                Types.String s = new String(raw
                    .Remove(0, 1)
                    .Remove(Self.Value.Length - 2, 1)
                    .Replace("\\\"", "\"")
                    .Replace("\\$", "$")
                    .Replace("\\0", "\0")
                    .Replace("\\a", "\a")
                    .Replace("\\b", "\b")
                    .Replace("\\f", "\f")
                    .Replace("\\n", "\n")
                    .Replace("\\r", "\r")
                    .Replace("\\t", "\t")
                    .Replace("\\v", "\v")
                    .Replace("\\\\", "\\"));

                return new Execution(ctx, s);
            }

            if (raw.ToLower() == "true") return new Execution(ctx, new Boolean(true));
            if (raw.ToLower() == "false") return new Execution(ctx, new Boolean(false));

            if (raw.EndsWith(".")) {
                System.Numerics.BigInteger tempInt;
                bool successInt = System.Numerics.BigInteger.TryParse(raw, out tempInt);
                if (successInt) {
                    Types.Integer i = tempInt;
                    return new Execution(ctx, i);
                }
            } else {
                double d;
                bool successDouble = double.TryParse(raw, out d);
                if (successDouble) {
                    Types.Float f = new Float(d);
                    return new Execution(ctx, f);
                }
            }

            var result = ctx.GetMember(raw);
            if (result == null) return new Execution();
            if (result is Reference r) return new Execution(ctx, r.GetDynamic());
            return new Execution(ctx, result);
        }
    }
}