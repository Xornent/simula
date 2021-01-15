namespace Simula.Maths.IntegralTransforms
{
    using System;

    /// <summary>
    /// Hartley Transform Convention
    /// </summary>
    [Flags]
    public enum HartleyOptions
    {
        // FLAGS:

        /// <summary>
        /// Only scale by 1/N in the inverse direction; No scaling in forward direction.
        /// </summary>
        AsymmetricScaling = 0x02,

        /// <summary>
        /// Don't scale at all (neither on forward nor on inverse transformation).
        /// </summary>
        NoScaling = 0x04,

        // USABILITY POINTERS:

        /// <summary>
        /// Universal; Symmetric scaling.
        /// </summary>
        Default = 0,
    }
}
