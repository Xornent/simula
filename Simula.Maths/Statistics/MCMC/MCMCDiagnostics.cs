﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Simula.Maths.Statistics.Mcmc
{
    /// <summary>
    /// Provides utilities to analysis the convergence of a set of samples from
    /// a <seealso cref="McmcSampler{T}"/>.
    /// </summary>
    public static class MCMCDiagnostics
    {
        /// <summary>
        /// Computes the auto correlations of a series evaluated by a function f.
        /// </summary>
        /// <param name="series">The series for computing the auto correlation.</param>
        /// <param name="lag">The lag in the series</param>
        /// <param name="f">The function used to evaluate the series.</param>
        /// <returns>The auto correlation.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Throws if lag is zero or if lag is
        /// greater than or equal to the length of Series.</exception>
        public static double ACF<T>(IEnumerable<T> series, int lag, Func<T,double> f)
        {
            if (lag < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(lag), "Lag must be positive");
            }

            int length = series.Count();
            if (lag >= length)
            {
                throw new ArgumentOutOfRangeException(nameof(lag), "Lag must be smaller than the sample size");
            }

            var transformedSeries = series.Select(f);

            var enumerable = transformedSeries as double[] ?? transformedSeries.ToArray();
            var firstSeries = enumerable.Take(length-lag);
            var secondSeries = enumerable.Skip(lag);

            return Correlation.Pearson(firstSeries, secondSeries);
        }

        /// <summary>
        /// Computes the effective size of the sample when evaluated by a function f.
        /// </summary>
        /// <param name="series">The samples.</param>
        /// <param name="f">The function use for evaluating the series.</param>
        /// <returns>The effective size when auto correlation is taken into account.</returns>
        public static double EffectiveSize<T>(IEnumerable<T> series, Func<T,double> f)
        {
            int length = series.Count();
            double rho = ACF(series, 1, f);
            return ((1 - rho) / (1 + rho)) * length;
        }
    }
}