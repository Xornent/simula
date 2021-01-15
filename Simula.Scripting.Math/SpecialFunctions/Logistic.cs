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
    /// This partial implementation of the SpecialFunctions class contains all methods related to the logistic function.
    /// </summary>
    public static partial class SpecialFunctions
    {
        /// <summary>
        /// Computes the logistic function. see: http://en.wikipedia.org/wiki/Logistic
        /// </summary>
        /// <param name="p">The parameter for which to compute the logistic function.</param>
        /// <returns>The logistic function of <paramref name="p"/>.</returns>
        public static double Logistic(double p)
        {
            return 1.0/(Math.Exp(-p) + 1.0);
        }

        /// <summary>
        /// Computes the logit function, the inverse of the sigmoid logistic function. see: http://en.wikipedia.org/wiki/Logit
        /// </summary>
        /// <param name="p">The parameter for which to compute the logit function. This number should be
        /// between 0 and 1.</param>
        /// <returns>The logarithm of <paramref name="p"/> divided by 1.0 - <paramref name="p"/>.</returns>
        public static double Logit(double p)
        {
            if (p < 0.0 || p > 1.0)
            {
                throw new ArgumentOutOfRangeException(nameof(p), "The argument must be between 0 and 1.");
            }

            return Math.Log(p/(1.0 - p));
        }
    }
}
