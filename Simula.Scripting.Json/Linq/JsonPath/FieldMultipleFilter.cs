using System.Collections.Generic;
using System.Globalization;
#if !HAVE_LINQ
using Simula.Scripting.Json.Utilities.LinqBridge;
#else
using System.Linq;
#endif
using Simula.Scripting.Json.Utilities;

namespace Simula.Scripting.Json.Linq.JsonPath
{
    internal class FieldMultipleFilter : PathFilter
    {
        internal List<string> Names;

        public FieldMultipleFilter(List<string> names)
        {
            Names = names;
        }

        public override IEnumerable<JToken> ExecuteFilter(JToken root, IEnumerable<JToken> current, bool errorWhenNoMatch)
        {
            foreach (JToken t in current) {
                if (t is JObject o) {
                    foreach (string name in Names) {
                        JToken? v = o[name];

                        if (v != null) {
                            yield return v;
                        }

                        if (errorWhenNoMatch) {
                            throw new JsonException("Property '{0}' does not exist on JObject.".FormatWith(CultureInfo.InvariantCulture, name));
                        }
                    }
                } else {
                    if (errorWhenNoMatch) {
                        throw new JsonException("Properties {0} not valid on {1}.".FormatWith(CultureInfo.InvariantCulture, string.Join(", ", Names.Select(n => "'" + n + "'")
#if !HAVE_STRING_JOIN_WITH_ENUMERABLE
                            .ToArray()
#endif
                            ), t.GetType().Name));
                    }
                }
            }
        }
    }
}