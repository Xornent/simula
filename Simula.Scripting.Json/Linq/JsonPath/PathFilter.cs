using Simula.Scripting.Json.Utilities;
using System.Collections.Generic;
using System.Globalization;

namespace Simula.Scripting.Json.Linq.JsonPath
{
    internal abstract class PathFilter
    {
        public abstract IEnumerable<JToken> ExecuteFilter(JToken root, IEnumerable<JToken> current, bool errorWhenNoMatch);

        protected static JToken? GetTokenIndex(JToken t, bool errorWhenNoMatch, int index)
        {
            if (t is JArray a) {
                if (a.Count <= index) {
                    if (errorWhenNoMatch) {
                        throw new JsonException("Index {0} outside the bounds of JArray.".FormatWith(CultureInfo.InvariantCulture, index));
                    }

                    return null;
                }

                return a[index];
            } else if (t is JConstructor c) {
                if (c.Count <= index) {
                    if (errorWhenNoMatch) {
                        throw new JsonException("Index {0} outside the bounds of JConstructor.".FormatWith(CultureInfo.InvariantCulture, index));
                    }

                    return null;
                }

                return c[index];
            } else {
                if (errorWhenNoMatch) {
                    throw new JsonException("Index {0} not valid on {1}.".FormatWith(CultureInfo.InvariantCulture, index, t.GetType().Name));
                }

                return null;
            }
        }

        protected static JToken? GetNextScanValue(JToken originalParent, JToken? container, JToken? value)
        {
            if (container != null && container.HasValues) {
                value = container.First;
            } else {
                while (value != null && value != originalParent && value == value.Parent!.Last) {
                    value = value.Parent;
                }
                if (value == null || value == originalParent) {
                    return null;
                }
                value = value.Next;
            }

            return value;
        }
    }
}