namespace Simula.Scripting.Json
{
    /// <summary>
    /// Specifies how date formatted strings, e.g. <c>"\/Date(1198908717056)\/"</c> and <c>"2012-03-21T05:40Z"</c>, are parsed when reading JSON text.
    /// </summary>
    public enum DateParseHandling
    {
        /// <summary>
        /// Date formatted strings are not parsed to a date type and are read as strings.
        /// </summary>
        None = 0,

        /// <summary>
        /// Date formatted strings, e.g. <c>"\/Date(1198908717056)\/"</c> and <c>"2012-03-21T05:40Z"</c>, are parsed to <see cref="DateTime"/>.
        /// </summary>
        DateTime = 1,
#if HAVE_DATE_TIME_OFFSET
        /// <summary>
        /// Date formatted strings, e.g. <c>"\/Date(1198908717056)\/"</c> and <c>"2012-03-21T05:40Z"</c>, are parsed to <see cref="DateTimeOffset"/>.
        /// </summary>
        DateTimeOffset = 2
#endif
    }
}