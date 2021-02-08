using System.Collections.Generic;
using Simula.Scripting.Build;
using Simula.Scripting.Contexts;
using Simula.Scripting.Token;
using Simula.Scripting.Types;

namespace Simula.Scripting.Syntax
{
    public class SelfOperation : OperatorStatement
    {
        public Token.Token Self { get; set; }

        bool IsLiteral = false;
        dynamic? literalCache;
        dynamic? containerTemp;
        bool notfound = false;
        bool isfield = false;
        bool isref = false;

        public override void Parse(TokenCollection collection)
        {
            this.Self = collection[0];
        }

        public override Execution Operate(DynamicRuntime ctx)
        {
            if (literalCache != null) return new Execution(ctx, literalCache);

            string raw = this.Self.ToString().Replace("\n","").Replace("\r","");
            if (raw.StartsWith("\"") && raw.EndsWith("\"") && (!raw.EndsWith("\\\""))) {
                if (raw.StartsWith("\"$")) {
                    Types.String noEscape = new String(raw.Remove(0, 2).Remove(Self.Value.Length - 3, 1));
                    literalCache = noEscape;
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

                literalCache = s;
                return new Execution(ctx, s);
            }

            if (raw.ToLower() == "true") { literalCache = new Boolean(true); return new Execution(ctx, literalCache); }
            if (raw.ToLower() == "false") { literalCache = new Boolean(false); return new Execution(ctx, literalCache); }

            double d;
            bool successDouble = double.TryParse(raw, out d);
            if (successDouble) {
                return new Execution(ctx, new Double(d));
            }

            if (containerTemp == null) {
                var result = ctx.GetMemberWithContainer(raw);
                containerTemp = result.Container;
                if (containerTemp is System.Dynamic.ExpandoObject) 
                    containerTemp = (IDictionary<string, object>)containerTemp;
                if (containerTemp is Var) isfield = true;
                if (result.Member == null) { notfound = true; return new Execution(); }
                if (result.Member is Reference r) { isref = true; return new Execution(ctx, r.GetDynamic()); }
                return new Execution(ctx, result.Member);
            } else {
                if (notfound) return new Execution();
                if (isfield) {
                    if (!isref) return new Execution(ctx, containerTemp._fields[raw]);
                    else return new Execution(ctx, containerTemp._fields[raw].GetDynamic());
                } else {
                    if (!isref) return new Execution(ctx, ((IDictionary<string, object>)containerTemp)[raw]);
                    else return new Execution(ctx, ((IDictionary<string, dynamic>)containerTemp)[raw].GetDynamic());
                }
            }
        }

        public override TypeInference InferType(CompletionContext ctx)
        {
            string raw = this.Self.ToString();
            if (raw.StartsWith("\"") && raw.EndsWith("\"") && (!raw.EndsWith("\\\""))) {
                return new TypeInference(new HashSet<string>() { "sys.string" }, null);
            }

            if (raw.ToLower() == "true") return new TypeInference(new HashSet<string>() { "sys.bool" }, null);
            if (raw.ToLower() == "false") return new TypeInference(new HashSet<string>() { "sys.bool" }, null);

            if (raw.EndsWith(".")) {
                System.Numerics.BigInteger tempInt;
                bool successInt = System.Numerics.BigInteger.TryParse(raw, out tempInt);
                if (successInt) {
                    return new TypeInference(new HashSet<string>() { "sys.int" }, null);
                }
            } else {
                double d;
                bool successDouble = double.TryParse(raw, out d);
                if (successDouble) {
                    return new TypeInference(new HashSet<string>() { "sys.double" }, null);
                }
            }

            var result = ctx.AccessibleRoots.Find((rec) => { return rec.Name == raw; });
            if (result == null) return new TypeInference(new HashSet<string>() { "null" }, null);
            return new TypeInference(result);
        }

        public override string Generate(GenerationContext ctx)
        {
            string raw = this.Self.ToString().Replace("\r","").Replace("\n","");
            if (raw.StartsWith("\"") && raw.EndsWith("\"") && (!raw.EndsWith("\\\""))) {
                if (raw.StartsWith("\"$")) {
                    Types.String noEscape = new String(raw.Remove(0, 2).Remove(Self.Value.Length - 3, 1));
                    IsLiteral = true;
                    raw = "@\"" + raw.Remove(0, 2);
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

                IsLiteral = true;
            }

            if (raw.ToLower() == "true") { IsLiteral = true; raw = raw.ToLower(); }
            if (raw.ToLower() == "false") { IsLiteral = true; raw = raw.ToLower(); }

            double d;
            bool successDouble = double.TryParse(raw, out d);
            if (successDouble) {
                IsLiteral = true;
            }

            if (IsLiteral) return "(" + raw + ")";
            else return raw;
        }
    }

    public class ObjectTransferOperation : OperatorStatement
    {
        public ObjectTransferOperation(dynamic obj)
        {
            this.Transfer = obj;
        }

        private dynamic Transfer;

        public override Execution Operate(DynamicRuntime ctx)
        {
            return new Execution(ctx, this.Transfer);
        }
    }
}