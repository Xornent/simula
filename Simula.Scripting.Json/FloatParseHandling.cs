namespace Simula.Scripting.Json
{
    /// <summary>
    /// Specifies how floating point numbers, e.g. 1.0 and 9.9, are parsed when reading JSON text.
    /// </summary>
    public enum FloatParseHandling
    {
        /// <summary>
        /// Floating point numbers are parsed to <see cref="Double"/>.
        /// </summary>
        Double = 0,

        /// <summary>
        /// Floating point numbers are parsed to <see cref="Decimal"/>.
        /// </summary>
        Decimal = 1
    }
}