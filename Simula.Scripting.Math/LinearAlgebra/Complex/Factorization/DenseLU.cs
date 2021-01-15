using System;
using Simula.Maths.Providers.LinearAlgebra;

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
    internal sealed class DenseLU : LU
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DenseLU"/> class. This object will compute the
        /// LU factorization when the constructor is called and cache it's factorization.
        /// </summary>
        /// <param name="matrix">The matrix to factor.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="matrix"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="matrix"/> is not a square matrix.</exception>
        public static DenseLU Create(DenseMatrix matrix)
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
            var pivots = new int[matrix.RowCount];

            // Create a new matrix for the LU factors, then perform factorization (while overwriting).
            var factors = (DenseMatrix) matrix.Clone();
            LinearAlgebraControl.Provider.LUFactor(factors.Values, factors.RowCount, pivots);

            return new DenseLU(factors, pivots);
        }

        DenseLU(Matrix<Complex> factors, int[] pivots)
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

            if (input is DenseMatrix dinput && result is DenseMatrix dresult)
            {
                // Copy the contents of input to result.
                Array.Copy(dinput.Values, 0, dresult.Values, 0, dinput.Values.Length);

                // LU solve by overwriting result.
                var dfactors = (DenseMatrix) Factors;
                LinearAlgebraControl.Provider.LUSolveFactored(input.ColumnCount, dfactors.Values, dfactors.RowCount, Pivots, dresult.Values);
            }
            else
            {
                throw new NotSupportedException("Can only do LU factorization for dense matrices at the moment.");
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

            if (input is DenseVector dinput && result is DenseVector dresult)
            {
                // Copy the contents of input to result.
                Array.Copy(dinput.Values, 0, dresult.Values, 0, dinput.Values.Length);

                // LU solve by overwriting result.
                var dfactors = (DenseMatrix) Factors;
                LinearAlgebraControl.Provider.LUSolveFactored(1, dfactors.Values, dfactors.RowCount, Pivots, dresult.Values);
            }
            else
            {
                throw new NotSupportedException("Can only do LU factorization for dense vectors at the moment.");
            }
        }

        /// <summary>
        /// Returns the inverse of this matrix. The inverse is calculated using LU decomposition.
        /// </summary>
        /// <returns>The inverse of this matrix.</returns>
        public override Matrix<Complex> Inverse()
        {
            var result = (DenseMatrix) Factors.Clone();
            LinearAlgebraControl.Provider.LUInverseFactored(result.Values, result.RowCount, Pivots);
            return result;
        }
    }
}
