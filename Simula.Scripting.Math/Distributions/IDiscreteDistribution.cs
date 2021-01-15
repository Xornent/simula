﻿namespace Simula.Maths.Distributions
{
    using System.Collections.Generic;

    /// <summary>
    /// Discrete Univariate Probability Distribution.
    /// </summary>
    /// <seealso cref="IContinuousDistribution"/>
    public interface IDiscreteDistribution : IUnivariateDistribution
    {
        /// <summary>
        /// Gets the mode of the distribution.
        /// </summary>
        int Mode { get; }

        /// <summary>
        /// Gets the smallest element in the domain of the distribution which can be represented by an integer.
        /// </summary>
        int Minimum { get; }

        /// <summary>
        /// Gets the largest element in the domain of the distribution which can be represented by an integer.
        /// </summary>
        int Maximum { get; }

        /// <summary>
        /// Computes the probability mass (PMF) at k, i.e. P(X = k).
        /// </summary>
        /// <param name="k">The location in the domain where we want to evaluate the probability mass function.</param>
        /// <returns>the probability mass at location <paramref name="k"/>.</returns>
        double Probability(int k);

        /// <summary>
        /// Computes the log probability mass (lnPMF) at k, i.e. ln(P(X = k)).
        /// </summary>
        /// <param name="k">The location in the domain where we want to evaluate the log probability mass function.</param>
        /// <returns>the log probability mass at location <paramref name="k"/>.</returns>
        double ProbabilityLn(int k);

        /// <summary>
        /// Draws a random sample from the distribution.
        /// </summary>
        /// <returns>a sample from the distribution.</returns>
        int Sample();

        /// <summary>
        /// Fills an array with samples generated from the distribution.
        /// </summary>
        void Samples(int[] values);

        /// <summary>
        /// Draws a sequence of random samples from the distribution.
        /// </summary>
        /// <returns>an infinite sequence of samples from the distribution.</returns>
        IEnumerable<int> Samples();
    }
}
