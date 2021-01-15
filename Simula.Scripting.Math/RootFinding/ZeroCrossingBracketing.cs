using System;
using System.Collections.Generic;

namespace Simula.Maths.RootFinding
{
    public static class ZeroCrossingBracketing
    {
        public static IEnumerable<Tuple<double, double>> FindIntervalsWithin(Func<double, double> f, double lowerBound, double upperBound, int subdivisions)
        {
            // TODO: Consider binary-style search instead of linear scan
            double fmin = f(lowerBound);
            double fmax = f(upperBound);

            if (Math.Sign(fmin) != Math.Sign(fmax))
            {
                yield return new Tuple<double, double>(lowerBound, upperBound);
                yield break;
            }

            double subdiv = (upperBound - lowerBound)/subdivisions;
            double smin = lowerBound;
            int sign = Math.Sign(fmin);

            for (int k = 0; k < subdivisions; k++)
            {
                double smax = smin + subdiv;
                double sfmax = f(smax);
                if (double.IsInfinity(sfmax))
                {
                    // expand interval to include pole
                    smin = smax;
                    continue;
                }

                if (Math.Sign(sfmax) != sign)
                {
                    yield return new Tuple<double, double>(smin, smax);
                    sign = Math.Sign(sfmax);
                }

                smin = smax;
            }
        }

        /// <summary>Detect a range containing at least one root.</summary>
        /// <param name="f">The function to detect roots from.</param>
        /// <param name="lowerBound">Lower value of the range.</param>
        /// <param name="upperBound">Upper value of the range</param>
        /// <param name="factor">The growing factor of research. Usually 1.6.</param>
        /// <param name="maxIterations">Maximum number of iterations. Usually 50.</param>
        /// <returns>True if the bracketing operation succeeded, false otherwise.</returns>
        /// <remarks>This iterative methods stops when two values with opposite signs are found.</remarks>
        public static bool Expand(Func<double, double> f, ref double lowerBound, ref double upperBound, double factor = 1.6, int maxIterations = 50)
        {
            double originalLowerBound = lowerBound;
            double originalUpperBound = upperBound;

            if (lowerBound >= upperBound)
            {
                throw new ArgumentOutOfRangeException(nameof(upperBound), "xmax must be greater than xmin.");
            }

            double fmin = f(lowerBound);
            double fmax = f(upperBound);

            for (int i = 0; i < maxIterations; i++)
            {
                if (Math.Sign(fmin) != Math.Sign(fmax))
                {
                    return true;
                }

                if (Math.Abs(fmin) < Math.Abs(fmax))
                {
                    lowerBound += factor*(lowerBound - upperBound);
                    fmin = f(lowerBound);
                }
                else
                {
                    upperBound += factor*(upperBound - lowerBound);
                    fmax = f(upperBound);
                }
            }

            lowerBound = originalLowerBound;
            upperBound = originalUpperBound;
            return false;
        }

        public static bool Reduce(Func<double, double> f, ref double lowerBound, ref double upperBound, int subdivisions = 1000)
        {
            double originalLowerBound = lowerBound;
            double originalUpperBound = upperBound;

            if (lowerBound >= upperBound)
            {
                throw new ArgumentOutOfRangeException(nameof(upperBound), "xmax must be greater than xmin.");
            }

            // TODO: Consider binary-style search instead of linear scan
            double fmin = f(lowerBound);
            double fmax = f(upperBound);

            if (Math.Sign(fmin) != Math.Sign(fmax))
            {
                return true;
            }

            double subdiv = (upperBound - lowerBound) / subdivisions;
            double smin = lowerBound;
            int sign = Math.Sign(fmin);

            for (int k = 0; k < subdivisions; k++)
            {
                double smax = smin + subdiv;
                double sfmax = f(smax);
                if (double.IsInfinity(sfmax))
                {
                    // expand interval to include pole
                    smin = smax;
                    continue;
                }

                if (Math.Sign(sfmax) != sign)
                {
                    lowerBound = smin;
                    upperBound = smax;
                    return true;
                }

                smin = smax;
            }

            lowerBound = originalLowerBound;
            upperBound = originalUpperBound;
            return false;
        }

        public static bool ExpandReduce(Func<double, double> f, ref double lowerBound, ref double upperBound, double expansionFactor = 1.6, int expansionMaxIterations = 50, int reduceSubdivisions = 100)
        {
            return Expand(f, ref lowerBound, ref upperBound, expansionFactor, expansionMaxIterations) || Reduce(f, ref lowerBound, ref upperBound, reduceSubdivisions);
        }
    }
}
