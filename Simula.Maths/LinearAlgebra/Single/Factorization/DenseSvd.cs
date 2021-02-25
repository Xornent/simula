using System;
using Simula.Maths.Providers.LinearAlgebra;

namespace Simula.Maths.LinearAlgebra.Single.Factorization
{

    /// <summary>
    /// <para>A class which encapsulates the functionality of the singular value decomposition (SVD) for <see cref="DenseMatrix"/>.</para>
    /// <para>Suppose M is an m-by-n matrix whose entries are real numbers.
    /// Then there exists a factorization of the form M = UΣVT where:
    /// - U is an m-by-m unitary matrix;
    /// - Σ is m-by-n diagonal matrix with nonnegative real numbers on the diagonal;
    /// - VT denotes transpose of V, an n-by-n unitary matrix;
    /// Such a factorization is called a singular-value decomposition of M. A common convention is to order the diagonal
    /// entries Σ(i,i) in descending order. In this case, the diagonal matrix Σ is uniquely determined
    /// by M (though the matrices U and V are not). The diagonal entries of Σ are known as the singular values of M.</para>
    /// </summary>
    /// <remarks>
    /// The computation of the singular value decomposition is done at construction time.
    /// </remarks>
    internal sealed class DenseSvd : Svd
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DenseSvd"/> class. This object will compute the
        /// the singular value decomposition when the constructor is called and cache it's decomposition.
        /// </summary>
        /// <param name="matrix">The matrix to factor.</param>
        /// <param name="computeVectors">Compute the singular U and VT vectors or not.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="matrix"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">If SVD algorithm failed to converge with matrix <paramref name="matrix"/>.</exception>
        public static DenseSvd Create(DenseMatrix matrix, bool computeVectors)
        {
            var nm = Math.Min(matrix.RowCount, matrix.ColumnCount);
            var s = new DenseVector(nm);
            var u = new DenseMatrix(matrix.RowCount);
            var vt = new DenseMatrix(matrix.ColumnCount);
            LinearAlgebraControl.Provider.SingularValueDecomposition(computeVectors, ((DenseMatrix) matrix.Clone()).Values, matrix.RowCount, matrix.ColumnCount, s.Values, u.Values, vt.Values);

            return new DenseSvd(s, u, vt, computeVectors);
        }

        DenseSvd(Vector<float> s, Matrix<float> u, Matrix<float> vt, bool vectorsComputed)
            : base(s, u, vt, vectorsComputed)
        {
        }

        /// <summary>
        /// Solves a system of linear equations, <b>AX = B</b>, with A SVD factorized.
        /// </summary>
        /// <param name="input">The right hand side <see cref="Matrix{T}"/>, <b>B</b>.</param>
        /// <param name="result">The left hand side <see cref="Matrix{T}"/>, <b>X</b>.</param>
        public override void Solve(Matrix<float> input, Matrix<float> result)
        {
            if (!VectorsComputed)
            {
                throw new InvalidOperationException("The singular vectors were not computed.");
            }

            // The solution X should have the same number of columns as B
            if (input.ColumnCount != result.ColumnCount)
            {
                throw new ArgumentException("Matrix column dimensions must agree.");
            }

            // The dimension compatibility conditions for X = A\B require the two matrices A and B to have the same number of rows
            if (U.RowCount != input.RowCount)
            {
                throw new ArgumentException("Matrix row dimensions must agree.");
            }

            // The solution X row dimension is equal to the column dimension of A
            if (VT.ColumnCount != result.RowCount)
            {
                throw new ArgumentException("Matrix column dimensions must agree.");
            }

            if (input is DenseMatrix dinput && result is DenseMatrix dresult)
            {
                LinearAlgebraControl.Provider.SvdSolveFactored(U.RowCount, VT.ColumnCount, ((DenseVector) S).Values, ((DenseMatrix) U).Values, ((DenseMatrix) VT).Values, dinput.Values, input.ColumnCount, dresult.Values);
            }
            else
            {
                throw new NotSupportedException("Can only do SVD factorization for dense matrices at the moment.");
            }
        }

        /// <summary>
        /// Solves a system of linear equations, <b>Ax = b</b>, with A SVD factorized.
        /// </summary>
        /// <param name="input">The right hand side vector, <b>b</b>.</param>
        /// <param name="result">The left hand side <see cref="Matrix{T}"/>, <b>x</b>.</param>
        public override void Solve(Vector<float> input, Vector<float> result)
        {
            if (!VectorsComputed)
            {
                throw new InvalidOperationException("The singular vectors were not computed.");
            }

            // Ax=b where A is an m x n matrix
            // Check that b is a column vector with m entries
            if (U.RowCount != input.Count)
            {
                throw new ArgumentException("All vectors must have the same dimensionality.");
            }

            // Check that x is a column vector with n entries
            if (VT.ColumnCount != result.Count)
            {
                throw Matrix.DimensionsDontMatch<ArgumentException>(VT, result);
            }

            if (input is DenseVector dinput && result is DenseVector dresult)
            {
                LinearAlgebraControl.Provider.SvdSolveFactored(U.RowCount, VT.ColumnCount, ((DenseVector) S).Values, ((DenseMatrix) U).Values, ((DenseMatrix) VT).Values, dinput.Values, 1, dresult.Values);
            }
            else
            {
                throw new NotSupportedException("Can only do SVD factorization for dense vectors at the moment.");
            }
        }
    }
}
