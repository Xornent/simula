using System;

namespace Simula.Maths.Integration
{
    /// <summary>
    /// Approximation algorithm for definite integrals by Simpson's rule.
    /// </summary>
    public static class SimpsonRule
    {
        /// <summary>
        /// Direct 3-point approximation of the definite integral in the provided interval by Simpson's rule.
        /// </summary>
        /// <param name="f">The analytic smooth function to integrate.</param>
        /// <param name="intervalBegin">Where the interval starts, inclusive and finite.</param>
        /// <param name="intervalEnd">Where the interval stops, inclusive and finite.</param>
        /// <returns>Approximation of the finite integral in the given interval.</returns>
        public static double IntegrateThreePoint(Func<double, double> f, double intervalBegin, double intervalEnd)
        {
            if (f == null)
            {
                throw new ArgumentNullException(nameof(f));
            }

            double midpoint = (intervalEnd + intervalBegin)/2;
            return (intervalEnd - intervalBegin)/6*(f(intervalBegin) + f(intervalEnd) + (4*f(midpoint)));
        }

        /// <summary>
        /// Composite N-point approximation of the definite integral in the provided interval by Simpson's rule.
        /// </summary>
        /// <param name="f">The analytic smooth function to integrate.</param>
        /// <param name="intervalBegin">Where the interval starts, inclusive and finite.</param>
        /// <param name="intervalEnd">Where the interval stops, inclusive and finite.</param>
        /// <param name="numberOfPartitions">Even number of composite subdivision partitions.</param>
        /// <returns>Approximation of the finite integral in the given interval.</returns>
        public static double IntegrateComposite(Func<double, double> f, double intervalBegin, double intervalEnd, int numberOfPartitions)
        {
            if (f == null)
            {
                throw new ArgumentNullException(nameof(f));
            }

            if (numberOfPartitions <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(numberOfPartitions), "Value must be positive (and not zero).");
            }

            if (numberOfPartitions.IsOdd())
            {
                throw new ArgumentException("Value must be even.", nameof(numberOfPartitions));
            }

            double step = (intervalEnd - intervalBegin)/numberOfPartitions;
            double factor = step/3;

            double offset = step;
            int m = 4;
            double sum = f(intervalBegin) + f(intervalEnd);
            for (int i = 0; i < numberOfPartitions - 1; i++)
            {
                // NOTE (cdrnet, 2009-01-07): Do not combine intervalBegin and offset (numerical stability)
                sum += m*f(intervalBegin + offset);
                m = 6 - m;
                offset += step;
            }

            return factor*sum;
        }
    }
}
