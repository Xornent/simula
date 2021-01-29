using System;
using Complex = System.Numerics.Complex;

namespace Simula.Maths
{
    /// <summary>
    /// Useful extension methods for Arrays.
    /// </summary>
    internal static class ArrayExtensions
    {
        /// <summary>
        /// Copies the values from on array to another.
        /// </summary>
        /// <param name="source">The source array.</param>
        /// <param name="dest">The destination array.</param>
        public static void Copy(this double[] source, double[] dest)
        {
            Buffer.BlockCopy(source, 0, dest, 0, source.Length * Constants.SizeOfDouble);
        }

        /// <summary>
        /// Copies the values from on array to another.
        /// </summary>
        /// <param name="source">The source array.</param>
        /// <param name="dest">The destination array.</param>
        public static void Copy(this float[] source, float[] dest)
        {
            Buffer.BlockCopy(source, 0, dest, 0, source.Length * Constants.SizeOfFloat);
        }

        /// <summary>
        /// Copies the values from on array to another.
        /// </summary>
        /// <param name="source">The source array.</param>
        /// <param name="dest">The destination array.</param>
        public static void Copy(this Complex[] source, Complex[] dest)
        {
            Array.Copy(source, 0, dest, 0, source.Length);
        }

        /// <summary>
        /// Copies the values from on array to another.
        /// </summary>
        /// <param name="source">The source array.</param>
        /// <param name="dest">The destination array.</param>
        public static void Copy(this Complex32[] source, Complex32[] dest)
        {
            Array.Copy(source, 0, dest, 0, source.Length);
        }
    }
}
