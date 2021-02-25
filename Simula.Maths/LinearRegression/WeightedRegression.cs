using System;
using System.Collections.Generic;
using Simula.Maths.LinearAlgebra;

namespace Simula.Maths.LinearRegression
{
    public static class WeightedRegression
    {
        /// <summary>
        /// Weighted Linear Regression using normal equations.
        /// </summary>
        /// <param name="x">Predictor matrix X</param>
        /// <param name="y">Response vector Y</param>
        /// <param name="w">Weight matrix W, usually diagonal with an entry for each predictor (row).</param>
        public static Vector<T> Weighted<T>(Matrix<T> x, Vector<T> y, Matrix<T> w) where T : struct, IEquatable<T>, IFormattable
        {
            return x.TransposeThisAndMultiply(w*x).Cholesky().Solve(x.TransposeThisAndMultiply(w*y));
        }

        /// <summary>
        /// Weighted Linear Regression using normal equations.
        /// </summary>
        /// <param name="x">Predictor matrix X</param>
        /// <param name="y">Response matrix Y</param>
        /// <param name="w">Weight matrix W, usually diagonal with an entry for each predictor (row).</param>
        public static Matrix<T> Weighted<T>(Matrix<T> x, Matrix<T> y, Matrix<T> w) where T : struct, IEquatable<T>, IFormattable
        {
            return x.TransposeThisAndMultiply(w*x).Cholesky().Solve(x.TransposeThisAndMultiply(w*y));
        }

        /// <summary>
        /// Weighted Linear Regression using normal equations.
        /// </summary>
        /// <param name="x">Predictor matrix X</param>
        /// <param name="y">Response vector Y</param>
        /// <param name="w">Weight matrix W, usually diagonal with an entry for each predictor (row).</param>
        /// <param name="intercept">True if an intercept should be added as first artificial predictor value. Default = false.</param>
        public static T[] Weighted<T>(T[][] x, T[] y, T[] w, bool intercept = false) where T : struct, IEquatable<T>, IFormattable
        {
            var predictor = Matrix<T>.Build.DenseOfRowArrays(x);
            if (intercept)
            {
                predictor = predictor.InsertColumn(0, Vector<T>.Build.Dense(predictor.RowCount, Vector<T>.One));
            }

            var response = Vector<T>.Build.Dense(y);
            var weights = Matrix<T>.Build.Diagonal(w);
            return predictor.TransposeThisAndMultiply(weights*predictor).Cholesky().Solve(predictor.TransposeThisAndMultiply(weights*response)).ToArray();
        }

        /// <summary>
        /// Weighted Linear Regression using normal equations.
        /// </summary>
        /// <param name="samples">List of sample vectors (predictor) together with their response.</param>
        /// <param name="weights">List of weights, one for each sample.</param>
        /// <param name="intercept">True if an intercept should be added as first artificial predictor value. Default = false.</param>
        public static T[] Weighted<T>(IEnumerable<Tuple<T[], T>> samples, T[] weights, bool intercept = false) where T : struct, IEquatable<T>, IFormattable
        {
            var xy = samples.UnpackSinglePass();
            return Weighted(xy.Item1, xy.Item2, weights, intercept);
        }

        /// <summary>
        /// Locally-Weighted Linear Regression using normal equations.
        /// </summary>
        [Obsolete("Warning: This function is here to stay but its signature will likely change. Opting out from semantic versioning.")]
        public static Vector<T> Local<T>(Matrix<T> x, Vector<T> y, Vector<T> t, double radius, Func<double, T> kernel) where T : struct, IEquatable<T>, IFormattable
        {
            // TODO: Weird kernel definition
            var w = Matrix<T>.Build.Dense(x.RowCount, x.RowCount);
            for (int i = 0; i < x.RowCount; i++)
            {
                w.At(i, i, kernel(Distance.Euclidean(t, x.Row(i))/radius));
            }

            return Weighted(x, y, w);
        }

        /// <summary>
        /// Locally-Weighted Linear Regression using normal equations.
        /// </summary>
        [Obsolete("Warning: This function is here to stay but its signature will likely change. Opting out from semantic versioning.")]
        public static Matrix<T> Local<T>(Matrix<T> x, Matrix<T> y, Vector<T> t, double radius, Func<double, T> kernel) where T : struct, IEquatable<T>, IFormattable
        {
            // TODO: Weird kernel definition
            var w = Matrix<T>.Build.Dense(x.RowCount, x.RowCount);
            for (int i = 0; i < x.RowCount; i++)
            {
                w.At(i, i, kernel(Distance.Euclidean(t, x.Row(i))/radius));
            }

            return Weighted(x, y, w);
        }

        [Obsolete("Warning: This function is here to stay but will likely be refactored and/or moved to another place. Opting out from semantic versioning.")]
        public static double GaussianKernel(double normalizedDistance)
        {
            return Math.Exp(-0.5*normalizedDistance*normalizedDistance);
        }
    }
}
