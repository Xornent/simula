using System;
using System.Collections.Generic;
using Simula.Maths.Distributions;
using Simula.Maths.Threading;

namespace Simula.Maths.Statistics
{
    /// <summary>
    /// Kernel density estimation (KDE).
    /// </summary>
    public static class KernelDensity
    {
        /// <summary>
        /// Estimate the probability density function of a random variable.
        /// </summary>
        /// <remarks>
        /// The routine assumes that the provided kernel is well defined, i.e. a real non-negative function that integrates to 1.
        /// </remarks>
        public static double Estimate(double x, double bandwidth, IList<double> samples, Func<double, double> kernel)
        {
            if (bandwidth <= 0)
            {
                throw new ArgumentException("The bandwidth must be a positive number!");
            }

            var n = samples.Count;
            var estimate = CommonParallel.Aggregate(0, n,
                               i => kernel((x - samples[i]) / bandwidth),
                               (a, b) => a + b,
                               0d) / (n * bandwidth);

            return estimate;
        }

        /// <summary>
        /// Estimate the probability density function of a random variable with a Gaussian kernel.
        /// </summary>
        public static double EstimateGaussian(double x, double bandwidth, IList<double> samples)
        {
            return Estimate(x, bandwidth, samples, GaussianKernel);
        }

        /// <summary>
        /// Estimate the probability density function of a random variable with an Epanechnikov kernel.
        /// The Epanechnikov kernel is optimal in a mean square error sense.
        /// </summary>
        public static double EstimateEpanechnikov(double x, double bandwidth, IList<double> samples)
        {
            return Estimate(x, bandwidth, samples, EpanechnikovKernel);
        }

        /// <summary>
        /// Estimate the probability density function of a random variable with a uniform kernel.
        /// </summary>
        public static double EstimateUniform(double x, double bandwidth, IList<double> samples)
        {
            return Estimate(x, bandwidth, samples, UniformKernel);
        }

        /// <summary>
        /// Estimate the probability density function of a random variable with a triangular kernel.
        /// </summary>
        public static double EstimateTriangular(double x, double bandwidth, IList<double> samples)
        {
            return Estimate(x, bandwidth, samples, TriangularKernel);
        }

        /// <summary>
        /// A Gaussian kernel (PDF of Normal distribution with mean 0 and variance 1).
        /// This kernel is the default.
        /// </summary>
        public static double GaussianKernel(double x)
        {
            return Normal.PDF(0.0, 1.0, x);
        }

        /// <summary>
        /// Epanechnikov Kernel:
        /// x =&gt; Math.Abs(x) &lt;= 1.0 ? 3.0/4.0(1.0-x^2) : 0.0
        /// </summary>
        public static double EpanechnikovKernel(double x)
        {
            return Math.Abs(x) <= 1.0 ? 0.75 * (1 - x * x) : 0.0;
        }

        /// <summary>
        /// Uniform Kernel:
        /// x =&gt; Math.Abs(x) &lt;= 1.0 ? 1.0/2.0 : 0.0
        /// </summary>
        public static double UniformKernel(double x)
        {
            return ContinuousUniform.PDF(-1.0, 1.0, x);
        }

        /// <summary>
        /// Triangular Kernel:
        /// x =&gt; Math.Abs(x) &lt;= 1.0 ? (1.0-Math.Abs(x)) : 0.0
        /// </summary>
        public static double TriangularKernel(double x)
        {
            return Triangular.PDF(-1.0, 1.0, 0.0, x);
        }
    }
}
