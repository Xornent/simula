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
            if(raw.StartsWith("\"") && raw.EndsWith("\"") && (!raw.EndsWith("\\\""))) {
                Types.String s = new String(raw
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
                    .Replace("\\v", "\v"));
                    
                return new Execution(ctx, s);
            }

            if(raw.ToLower() == "true") return new Execution(ctx, new Boolean(true));
            if(raw.ToLower() == "false") return new Execution(ctx, new Boolean(false));

            System.Numerics.BigInteger tempInt;
            bool successInt = System.Numerics.BigInteger.TryParse(raw, out tempInt);
            if (successInt) {
                Types.Integer i = tempInt;
                return new Execution(ctx, i);
            }

            IDictionary<string, object> dict = (IDictionary<string, object>) ctx.Store;
            if (dict.ContainsKey(raw)) {
                if (dict[raw] is Reference r) return new Execution(ctx, r.GetDynamic());
                return new Execution(ctx, dict[raw]);
            } else return new Execution();
        }
    }
}