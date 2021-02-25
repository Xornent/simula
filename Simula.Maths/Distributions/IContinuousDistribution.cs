namespace Simula.Maths.Distributions
{
    using System.Collections.Generic;

    /// <summary>
    /// Continuous Univariate Probability Distribution.
    /// </summary>
    /// <seealso cref="IDiscreteDistribution"/>
    public interface IContinuousDistribution : IUnivariateDistribution
    {
        /// <summary>
        /// Gets the mode of the distribution.
        /// </summary>
        double Mode { get; }

        /// <summary>
        /// Gets the smallest element in the domain of the distribution which can be represented by a double.
        /// </summary>
        double Minimum { get; }

        /// <summary>
        /// Gets the largest element in the domain of the distribution which can be represented by a double.
        /// </summary>
        double Maximum { get; }

        /// <summary>
        /// Computes the probability density of the distribution (PDF) at x, i.e. ∂P(X ≤ x)/∂x.
        /// </summary>
        /// <param name="x">The location at which to compute the density.</param>
        /// <returns>the density at <paramref name="x"/>.</returns>
        double Density(double x);

        /// <summary>
        /// Computes the log probability density of the distribution (lnPDF) at x, i.e. ln(∂P(X ≤ x)/∂x).
        /// </summary>
        /// <param name="x">The location at which to compute the log density.</param>
        /// <returns>the log density at <paramref name="x"/>.</returns>
        double DensityLn(double x);

        /// <summary>
        /// Draws a random sample from the distribution.
        /// </summary>
        /// <returns>a sample from the distribution.</returns>
        double Sample();

        /// <summary>
        /// Fills an array with samples generated from the distribution.
        /// </summary>
        void Samples(double[] values);

        /// <summary>
        /// Draws a sequence of random samples from the distribution.
        /// </summary>
        /// <returns>an infinite sequence of samples from the distribution.</returns>
        IEnumerable<double> Samples();
    }
}
