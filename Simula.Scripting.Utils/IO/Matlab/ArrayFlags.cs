using System;

namespace Simula.Maths.IO.Matlab
{
    /// <summary>
    /// MATLAB Array Flags
    /// </summary>
    [Flags]
    internal enum ArrayFlags
    {
        /// <summary>
        /// Complex flag
        /// </summary>
        Complex = 8,

        /// <summary>
        /// Global flag
        /// </summary>
        Global = 4,

        /// <summary>
        /// Logical flag
        /// </summary>
        Logical = 2
    }
}
