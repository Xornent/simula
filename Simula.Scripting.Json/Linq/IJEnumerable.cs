
using System.Collections.Generic;

namespace Simula.Scripting.Json.Linq
{
    public interface IJEnumerable<
#if HAVE_VARIANT_TYPE_PARAMETERS
        out
#endif
            T> : IEnumerable<T> where T : JToken
    {
        IJEnumerable<JToken> this[object key] { get; }
    }
}