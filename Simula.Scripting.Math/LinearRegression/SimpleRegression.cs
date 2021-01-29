﻿using System;
using System.Collections.Generic;

namespace Simula.Maths.LinearRegression
{
    public static class SimpleRegression
    {
        /// <summary>
        /// Least-Squares fitting the points (x,y) to a line y : x -> a+b*x,
        /// returning its best fitting parameters as (a, b) tuple,
        /// where a is the intercept and b the slope.
        /// </summary>
        /// <param name="x">Predictor (independent)</param>
        /// <param name="y">Response (dependent)</param>
        public static Tuple<double, double> Fit(double[] x, double[] y)
        {
            if (x.Length != y.Length)
            {
                throw new ArgumentException($"All sample vectors must have the same length. However, vectors with disagreeing length {x.Length} and {y.Length} have been provided. A sample with index i is given by the value at index i of each provided vector.");
            }

            if (x.Length <= 1)
            {
                throw new ArgumentException($"A regression of the requested order requires at least {2} samples. Only {x.Length} samples have been provided.");
            }

            // First Pass: Mean (Less robust but faster than ArrayStatistics.Mean)
            double mx = 0.0;
            double my = 0.0;
            for (int i = 0; i < x.Length; i++)
            {
                mx += x[i];
                my += y[i];
            }

            mx /= x.Length;
            my /= y.Length;

            // Second Pass: Covariance/Variance
            double covariance = 0.0;
            double variance = 0.0;
            for (int i = 0; i < x.Length; i++)
            {
                double diff = x[i] - mx;
                covariance += diff*(y[i] - my);
                variance += diff*diff;
            }

            var b = covariance/variance;
            return new Tuple<double, double>(my - b*mx, b);
        }

        /// <summary>
        /// Least-Squares fitting the points (x,y) to a line y : x -> a+b*x,
        /// returning its best fitting parameters as (a, b) tuple,
        /// where a is the intercept and b the slope.
        /// </summary>
        /// <param name="samples">Predictor-Response samples as tuples</param>
        public static Tuple<double, double> Fit(IEnumerable<Tuple<double, double>> samples)
        {
            var xy = samples.UnpackSinglePass();
            return Fit(xy.Item1, xy.Item2);
        }

        /// <summary>
        /// Least-Squares fitting the points (x,y) to a line y : x -> b*x,
        /// returning its best fitting parameter b,
        /// where the intercept is zero and b the slope.
        /// </summary>
        /// <param name="x">Predictor (independent)</param>
        /// <param name="y">Response (dependent)</param>
        public static double FitThroughOrigin(double[] x, double[] y)
        {
            if (x.Length != y.Length)
            {
                throw new ArgumentException($"All sample vectors must have the same length. However, vectors with disagreeing length {x.Length} and {y.Length} have been provided. A sample with index i is given by the value at index i of each provided vector.");
            }

            if (x.Length <= 1)
            {
                throw new ArgumentException($"A regression of the requested order requires at least {2} samples. Only {x.Length} samples have been provided.");
            }

            double mxy = 0.0;
            double mxx = 0.0;
            for (int i = 0; i < x.Length; i++)
            {
                mxx += x[i]*x[i];
                mxy += x[i]*y[i];
            }

            return mxy / mxx;
        }


        /// <summary>
        /// Least-Squares fitting the points (x,y) to a line y : x -> b*x,
        /// returning its best fitting parameter b,
        /// where the intercept is zero and b the slope.
        /// </summary>
        /// <param name="samples">Predictor-Response samples as tuples</param>
        public static double FitThroughOrigin(IEnumerable<Tuple<double, double>> samples)
        {
            double mxy = 0.0;
            double mxx = 0.0;

            foreach (var sample in samples)
            {
                mxx += sample.Item1 * sample.Item1;
                mxy += sample.Item1 * sample.Item2;
            }

            return mxy / mxx;
        }
    }
}
