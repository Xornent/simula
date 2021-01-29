using System;

namespace Simula.Maths.Providers.Common.Mkl
{
    /// <summary>
    /// Consistency vs. performance trade-off between runs on different machines.
    /// </summary>
    public enum MklConsistency : int
    {
        /// <summary>Consistent on the same CPU only (maximum performance)</summary>
        Auto = 2,
        /// <summary>Consistent on Intel and compatible CPUs with SSE2 support (maximum compatibility)</summary>
        Compatible = 3,
        /// <summary>Consistent on Intel CPUs supporting SSE2 or later</summary>
        SSE2 = 4,
        /// <summary>Consistent on Intel CPUs supporting SSE4.2 or later</summary>
        SSE4_2 = 8,
        /// <summary>Consistent on Intel CPUs supporting AVX or later</summary>
        AVX = 9,
        /// <summary>Consistent on Intel CPUs supporting AVX2 or later</summary>
        AVX2 = 10
    }

    [CLSCompliant(false)]
    public enum MklAccuracy : uint
    {
        Low = 0x1,
        High = 0x2
    }

    [CLSCompliant(false)]
    public enum MklPrecision : uint
    {
        Single = 0x10,
        Double = 0x20
    }
}
