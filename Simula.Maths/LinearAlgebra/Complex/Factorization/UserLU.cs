﻿using System;

namespace Simula.Maths.LinearAlgebra.Complex.Factorization
{
    using Complex = System.Numerics.Complex;

    /// <summary>
    /// <para>A class which encapsulates the functionality of an LU factorization.</para>
    /// <para>For a matrix A, the LU factorization is a pair of lower triangular matrix L and
    /// upper triangular matrix U so that A = L*U.</para>
    /// </summary>
    /// <remarks>
    /// The computation of the LU factorization is done at construction time.
    /// </remarks>
    internal sealed class UserLU : LU
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserLU"/> class. This object will compute the
        /// LU factorization when the constructor is called and cache it's factorization.
        /// </summary>
        /// <param name="matrix">The matrix to factor.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="matrix"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="matrix"/> is not a square matrix.</exception>
        public static UserLU Create(Matrix<Complex> matrix)
        {
            if (matrix == null)
            {
                throw new ArgumentNullException(nameof(matrix));
            }

            if (matrix.RowCount != matrix.ColumnCount)
            {
                throw new ArgumentException("Matrix must be square.");
            }

            // Create an array for the pivot indices.
            var order = matrix.RowCount;
            var factors = matrix.Clone();
            var pivots = new int[order];

            // Initialize the pivot matrix to the identity permutation.
            for (var i = 0; i < order; i++)
            {
                pivots[i] = i;
            }

            var vectorLUcolj = new Complex[order];
            for (var j = 0; j < order; j++)
            {
                // Make a copy of the j-th column to localize references.
                for (var i = 0; i < order; i++)
                {
                    vectorLUcolj[i] = factors.At(i, j);
                }

                // Apply previous transformations.
                for (var i = 0; i < order; i++)
                {
                    var kmax = Math.Min(i, j);
                    var s = Complex.Zero;
                    for (var k = 0; k < kmax; k++)
                    {
                        s += factors.At(i, k)*vectorLUcolj[k];
                    }

                    vectorLUcolj[i] -= s;
                    factors.At(i, j, vectorLUcolj[i]);
                }

                // Find pivot and exchange if necessary.
                var p = j;
                for (var i = j + 1; i < order; i++)
                {
                    if (vectorLUcolj[i].Magnitude > vectorLUcolj[p].Magnitude)
                    {
                        p = i;
                    }
                }

                if (p != j)
                {
                    for (var k = 0; k < order; k++)
                    {
                        var temp = factors.At(p, k);
                        factors.At(p, k, factors.At(j, k));
                        factors.At(j, k, temp);
                    }

                    pivots[j] = p;
                }

                // Compute multipliers.
                if (j < order & factors.At(j, j) != 0.0)
                {
                    for (var i = j + 1; i < order; i++)
                    {
                        factors.At(i, j, (factors.At(i, j)/factors.At(j, j)));
                    }
                }
            }

            return new UserLU(factors, pivots);
        }

        UserLU(Matrix<Complex> factors, int[] pivots)
            : base(factors, pivots)
        {
        }

        /// <summary>
        /// Solves a system of linear equations, <c>AX = B</c>, with A LU factorized.
        /// </summary>
        /// <param name="input">The right hand side <see cref="Matrix{T}"/>, <c>B</c>.</param>
        /// <param name="result">The left hand side <see cref="Matrix{T}"/>, <c>X</c>.</param>
        public override void Solve(Matrix<Complex> input, Matrix<Complex> result)
        {
            // Check for proper arguments.
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            // Check for proper dimensions.
            if (result.RowCount != input.RowCount)
            {
                throw new ArgumentException("Matrix row dimensions must agree.");
            }

            if (result.ColumnCount != input.ColumnCount)
            {
                throw new ArgumentException("Matrix column dimensions must agree.");
            }

            if (input.RowCount != Factors.RowCount)
            {
                throw Matrix.DimensionsDontMatch<ArgumentException>(input, Factors);
            }

            // Copy the contents of input to result.
            input.CopyTo(result);
            for (var i = 0; i < Pivots.Length; i++)
            {
                if (Pivots[i] == i)
                {
                    continue;
                }

                var p = Pivots[i];
                for (var j = 0; j < result.ColumnCount; j++)
                {
                    var temp = result.At(p, j);
                    result.At(p, j, result.At(i, j));
                    result.At(i, j, temp);
                }
            }

            var order = Factors.RowCount;

            // Solve L*Y = P*B
            for (var k = 0; k < order; k++)
            {
                for (var i = k + 1; i < order; i++)
                {
                    for (var j = 0; j < result.ColumnCount; j++)
                    {
                        var temp = result.At(k, j)*Factors.At(i, k);
                        result.At(i, j, result.At(i, j) - temp);
                    }
                }
            }

            // Solve U*X = Y;
            for (var k = order - 1; k >= 0; k--)
            {
                for (var j = 0; j < result.ColumnCount; j++)
                {
                    result.At(k, j, (result.At(k, j)/Factors.At(k, k)));
                }

                for (var i = 0; i < k; i++)
                {
                    for (var j = 0; j < result.ColumnCount; j++)
                    {
                        var temp = result.At(k, j)*Factors.At(i, k);
                        result.At(i, j, result.At(i, j) - temp);
                    }
                }
            }
        }

        /// <summary>
        /// Solves a system of linear equations, <c>Ax = b</c>, with A LU factorized.
        /// </summary>
        /// <param name="input">The right hand side vector, <c>b</c>.</param>
        /// <param name="result">The left hand side <see cref="Matrix{T}"/>, <c>x</c>.</param>
        public override void Solve(Vector<Complex> input, Vector<Complex> result)
        {
            // Check for proper arguments.
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            // Check for proper dimensions.
            if (input.Count != result.Count)
            {
                throw new ArgumentException("All vectors must have the same dimensionality.");
            }

            if (input.Count != Factors.RowCount)
            {
                throw Matrix.DimensionsDontMatch<ArgumentException>(input, Factors);
            }

            // Copy the contents of input to result.
            input.CopyTo(result);
            for (var i = 0; i < Pivots.Length; i++)
            {
                if (Pivots[i] == i)
                {
                    continue;
                }

                var p = Pivots[i];
                var temp = result[p];
                result[p] = result[i];
                result[i] = temp;
            }

            var order = Factors.RowCount;

            // Solve L*Y = P*B
            for (var k = 0; k < order; k++)
            {
                for (var i = k + 1; i < order; i++)
                {
                    result[i] -= result[k]*Factors.At(i, k);
                }
            }

            // Solve U*X = Y;
            for (var k = order - 1; k >= 0; k--)
            {
                result[k] /= Factors.At(k, k);
                for (var i = 0; i < k; i++)
                {
                    result[i] -= result[k]*Factors.At(i, k);
                }
            }
        }

        /// <summary>
        /// Returns the inverse of this matrix. The inverse is calculated using LU decomposition.
        /// </summary>
        /// <returns>The inverse of this matrix.</returns>
        public override Matrix<Complex> Inverse()
        {
            var order = Factors.RowCount;
            var inverse = Matrix<Complex>.Build.SameAs(Factors, order, order);
            for (var i = 0; i < order; i++)
            {
                inverse.At(i, i, 1.0);
            }

            return Solve(inverse);
        }
    }
}
