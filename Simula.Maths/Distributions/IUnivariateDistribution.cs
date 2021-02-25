namespace Simula.Maths.Distributions
{
    /// <summary>
    /// Univariate Probability Distribution.
    /// </summary>
    /// <seealso cref="IContinuousDistribution"/>
    /// <seealso cref="IDiscreteDistribution"/>
    public interface IUnivariateDistribution : IDistribution
    {
        /// <summary>
        /// Gets the mean of the distribution.
        /// </summary>
        double Mean { get; }

        /// <summary>
        /// Gets the variance of the distribution.
        /// </summary>
        double Variance { get; }

        /// <summary>
        /// Gets the standard deviation of the distribution.
        /// </summary>
        double StdDev { get; }

        /// <summary>
        /// Gets the entropy of the distribution.
        /// </summary>
        double Entropy { get; }

        /// <summary>
        /// Gets the skewness of the distribution.
        /// </summary>
        double Skewness { get; }

        /// <summary>
        /// Gets the median of the distribution.
        /// </summary>
        double Median { get; }

        /// <summary>
        /// Computes the cumulative distribution (CDF) of the distribution at x, i.e. P(X ≤ x).
        /// </summary>
        /// <param name="x">The location at which to compute the cumulative distribution function.</param>
        /// <returns>the cumulative distribution at location <paramref name="x"/>.</returns>
        double CumulativeDistribution(double x);
    }
}
