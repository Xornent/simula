// <contribution>
//    Cephes Math Library, Stephen L. Moshier
//    ALGLIB 2.0.1, Sergey Bochkanov
// </contribution>

using System;

// ReSharper disable CheckNamespace
namespace Simula.Maths
// ReSharper restore CheckNamespace
{
    /// <summary>
    /// This partial implementation of the SpecialFunctions class contains all methods related to the harmonic function.
    /// </summary>
    public static partial class SpecialFunctions
    {
        /// <summary>
        /// Computes the <paramref name="t"/>'th Harmonic number.
        /// </summary>
        /// <param name="t">The Harmonic number which needs to be computed.</param>
        /// <returns>The t'th Harmonic number.</returns>
        public static double Harmonic(int t)
        {
            return Constants.EulerMascheroni + DiGamma(t + 1.0);
        }

        /// <summary>
        /// Compute the generalized harmonic number of order n of m. (1 + 1/2^m + 1/3^m + ... + 1/n^m)
        /// </summary>
        /// <param name="n">The order parameter.</param>
        /// <param name="m">The power parameter.</param>
        /// <returns>General Harmonic number.</returns>
        public static double GeneralHarmonic(int n, double m)
        {
            double sum = 0;
            for (int i = 0; i < n; i++)
            {
                sum += Math.Pow(i + 1, -m);
            }

            return sum;
        }
    }
}
