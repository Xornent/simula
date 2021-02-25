using System.Collections.Generic;

namespace Simula.Scripting.Json.Linq
{
    /// <summary>
    /// Represents a collection of <see cref="JToken"/> objects.
    /// </summary>
    /// <typeparam name="T">The type of token.</typeparam>
    public interface IJEnumerable<
#if HAVE_VARIANT_TYPE_PARAMETERS
        out
#endif
            T> : IEnumerable<T> where T : JToken
    {
        /// <summary>
        /// Gets the <see cref="IJEnumerable{T}"/> of <see cref="JToken"/> with the specified key.
        /// </summary>
        /// <value></value>
        IJEnumerable<JToken> this[object key] { get; }
    }
}