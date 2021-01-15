﻿using System;
using Simula.Maths.Providers.LinearAlgebra;

namespace Simula.Maths.LinearAlgebra.Complex32.Factorization
{
    using Maths;

    /// <summary>
    /// <para>A class which encapsulates the functionality of a Cholesky factorization for dense matrices.</para>
    /// <para>For a symmetric, positive definite matrix A, the Cholesky factorization
    /// is an lower triangular matrix L so that A = L*L'.</para>
    /// </summary>
    /// <remarks>
    /// The computation of the Cholesky factorization is done at construction time. If the matrix is not symmetric
    /// or positive definite, the constructor will throw an exception.
    /// </remarks>
    internal sealed class DenseCholesky : Cholesky
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DenseCholesky"/> class. This object will compute the
        /// Cholesky factorization when the constructor is called and cache it's factorization.
        /// </summary>
        /// <param name="matrix">The matrix to factor.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="matrix"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="matrix"/> is not a square matrix.</exception>
        /// <exception cref="ArgumentException">If <paramref name="matrix"/> is not positive definite.</exception>
        public static DenseCholesky Create(DenseMatrix matrix)
        {
            if (matrix.RowCount != matrix.ColumnCount)
            {
                throw new ArgumentException("Matrix must be square.");
            }

            // Create a new matrix for the Cholesky factor, then perform factorization (while overwriting).
            var factor = (DenseMatrix) matrix.Clone();
            LinearAlgebraControl.Provider.CholeskyFactor(factor.Values, factor.RowCount);
            return new DenseCholesky(factor);
        }

        DenseCholesky(Matrix<Complex32> factor)
            : base(factor)
        {
        }

        /// <summary>
        /// Solves a system of linear equations, <b>AX = B</b>, with A Cholesky factorized.
        /// </summary>
        /// <param name="input">The right hand side <see cref="Matrix{T}"/>, <b>B</b>.</param>
        /// <param name="result">The left hand side <see cref="Matrix{T}"/>, <b>X</b>.</param>
        public override void Solve(Matrix<Complex32> input, Matrix<Complex32> result)
        {
            if (result.RowCount != input.RowCount)
            {
                throw new ArgumentException("Matrix row dimensions must agree.");
            }

            if (result.ColumnCount != input.ColumnCount)
            {
                throw new ArgumentException("Matrix column dimensions must agree.");
            }

            if (input.RowCount != Factor.RowCount)
            {
                throw Matrix.DimensionsDontMatch<ArgumentException>(input, Factor);
            }

            if (input is DenseMatrix dinput && result is DenseMatrix dresult)
            {
                // Copy the contents of input to result.
                Array.Copy(dinput.Values, 0, dresult.Values, 0, dinput.Values.Length);

                // Cholesky solve by overwriting result.
                var dfactor = (DenseMatrix) Factor;
                LinearAlgebraControl.Provider.CholeskySolveFactored(dfactor.Values, dfactor.RowCount, dresult.Values, dresult.ColumnCount);
            }
            else
            {
                throw new NotSupportedException("Can only do Cholesky factorization for dense matrices at the moment.");
            }
        }

        /// <summary>
        /// Solves a system of linear equations, <b>Ax = b</b>, with A Cholesky factorized.
        /// </summary>
        /// <param name="input">The right hand side vector, <b>b</b>.</param>
        /// <param name="result">The left hand side <see cref="Matrix{T}"/>, <b>x</b>.</param>
        public override void Solve(Vector<Complex32> input, Vector<Complex32> result)
        {
            if (input.Count != result.Count)
            {
                throw new ArgumentException("All vectors must have the same dimensionality.");
            }

            if (input.Count != Factor.RowCount)
            {
                throw Matrix.DimensionsDontMatch<ArgumentException>(input, Factor);
            }

            if (input is DenseVector dinput && result is DenseVector dresult)
            {
                // Copy the contents of input to result.
                Array.Copy(dinput.Values, 0, dresult.Values, 0, dinput.Values.Length);

                // Cholesky solve by overwriting result.
                var dfactor = (DenseMatrix) Factor;
                LinearAlgebraControl.Provider.CholeskySolveFactored(dfactor.Values, dfactor.RowCount, dresult.Values, 1);
            }
            else
            {
                throw new NotSupportedException("Can only do Cholesky factorization for dense vectors at the moment.");
            }
        }

        /// <summary>
        /// Calculates the Cholesky factorization of the input matrix.
        /// </summary>
        /// <param name="matrix">The matrix to be factorized<see cref="Matrix{T}"/>.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="matrix"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="matrix"/> is not a square matrix.</exception>
        /// <exception cref="ArgumentException">If <paramref name="matrix"/> is not positive definite.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="matrix"/> does not have the same dimensions as the existing factor.</exception>
        public override void Factorize(Matrix<Complex32> matrix)
        {
            if (matrix.RowCount != matrix.ColumnCount)
            {
                throw new ArgumentException("Matrix must be square.");
            }

            if (matrix.RowCount != Factor.RowCount || matrix.ColumnCount != Factor.ColumnCount)
            {
                throw Matrix.DimensionsDontMatch<ArgumentException>(matrix, Factor);
            }

            if (matrix is DenseMatrix dmatrix)
            {
                var dfactor = (DenseMatrix) Factor;

                // Overwrite the existing Factor matrix with the input.
                Array.Copy(dmatrix.Values, 0, dfactor.Values, 0, dmatrix.Values.Length);

                // Perform factorization (while overwriting).
                LinearAlgebraControl.Provider.CholeskyFactor(dfactor.Values, dfactor.RowCount);
            }
            else
            {
                throw new NotSupportedException("Can only do Cholesky factorization for dense matrices at the moment.");
            }
        }
    }
}
