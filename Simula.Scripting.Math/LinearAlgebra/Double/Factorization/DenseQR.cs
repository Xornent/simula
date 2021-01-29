using System;
using Simula.Maths.LinearAlgebra.Factorization;
using Simula.Maths.Providers.LinearAlgebra;

namespace Simula.Maths.LinearAlgebra.Double.Factorization
{
    /// <summary>
    /// <para>A class which encapsulates the functionality of the QR decomposition.</para>
    /// <para>Any real square matrix A may be decomposed as A = QR where Q is an orthogonal matrix
    /// (its columns are orthogonal unit vectors meaning QTQ = I) and R is an upper triangular matrix
    /// (also called right triangular matrix).</para>
    /// </summary>
    /// <remarks>
    /// The computation of the QR decomposition is done at construction time by Householder transformation.
    /// </remarks>
    internal sealed class DenseQR : QR
    {
        /// <summary>
        ///  Gets or sets Tau vector. Contains additional information on Q - used for native solver.
        /// </summary>
        double[] Tau { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DenseQR"/> class. This object will compute the
        /// QR factorization when the constructor is called and cache it's factorization.
        /// </summary>
        /// <param name="matrix">The matrix to factor.</param>
        /// <param name="method">The type of QR factorization to perform.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="matrix"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="matrix"/> row count is less then column count</exception>
        public static DenseQR Create(DenseMatrix matrix, QRMethod method = QRMethod.Full)
        {
            if (matrix.RowCount < matrix.ColumnCount)
            {
                throw Matrix.DimensionsDontMatch<ArgumentException>(matrix);
            }

            var tau = new double[Math.Min(matrix.RowCount, matrix.ColumnCount)];
            Matrix<double> q;
            Matrix<double> r;

            if (method == QRMethod.Full)
            {
                r = matrix.Clone();
                q = new DenseMatrix(matrix.RowCount);
                LinearAlgebraControl.Provider.QRFactor(((DenseMatrix) r).Values, matrix.RowCount, matrix.ColumnCount, ((DenseMatrix) q).Values, tau);
            }
            else
            {
                q = matrix.Clone();
                r = new DenseMatrix(matrix.ColumnCount);
                LinearAlgebraControl.Provider.ThinQRFactor(((DenseMatrix) q).Values, matrix.RowCount, matrix.ColumnCount, ((DenseMatrix) r).Values, tau);
            }

            return new DenseQR(q, r, method, tau);
        }

        DenseQR(Matrix<double> q, Matrix<double> rFull, QRMethod method, double[] tau)
            : base(q, rFull, method)
        {
            Tau = tau;
        }

        /// <summary>
        /// Solves a system of linear equations, <b>AX = B</b>, with A QR factorized.
        /// </summary>
        /// <param name="input">The right hand side <see cref="Matrix{T}"/>, <b>B</b>.</param>
        /// <param name="result">The left hand side <see cref="Matrix{T}"/>, <b>X</b>.</param>
        public override void Solve(Matrix<double> input, Matrix<double> result)
        {
            // The solution X should have the same number of columns as B
            if (input.ColumnCount != result.ColumnCount)
            {
                throw new ArgumentException("Matrix column dimensions must agree.");
            }

            // The dimension compatibility conditions for X = A\B require the two matrices A and B to have the same number of rows
            if (Q.RowCount != input.RowCount)
            {
                throw new ArgumentException("Matrix row dimensions must agree.");
            }

            // The solution X row dimension is equal to the column dimension of A
            if (FullR.ColumnCount != result.RowCount)
            {
                throw new ArgumentException("Matrix column dimensions must agree.");
            }

            if (input is DenseMatrix dinput && result is DenseMatrix dresult)
            {
                LinearAlgebraControl.Provider.QRSolveFactored(((DenseMatrix) Q).Values, ((DenseMatrix) FullR).Values, Q.RowCount, FullR.ColumnCount, Tau, dinput.Values, input.ColumnCount, dresult.Values, Method);
            }
            else
            {
                throw new NotSupportedException("Can only do QR factorization for dense matrices at the moment.");
            }
        }

        /// <summary>
        /// Solves a system of linear equations, <b>Ax = b</b>, with A QR factorized.
        /// </summary>
        /// <param name="input">The right hand side vector, <b>b</b>.</param>
        /// <param name="result">The left hand side <see cref="Matrix{T}"/>, <b>x</b>.</param>
        public override void Solve(Vector<double> input, Vector<double> result)
        {
            // Ax=b where A is an m x n matrix
            // Check that b is a column vector with m entries
            if (Q.RowCount != input.Count)
            {
                throw new ArgumentException("All vectors must have the same dimensionality.");
            }

            // Check that x is a column vector with n entries
            if (FullR.ColumnCount != result.Count)
            {
                throw Matrix.DimensionsDontMatch<ArgumentException>(FullR, result);
            }

            if (input is DenseVector dinput && result is DenseVector dresult)
            {
                LinearAlgebraControl.Provider.QRSolveFactored(((DenseMatrix) Q).Values, ((DenseMatrix) FullR).Values, Q.RowCount, FullR.ColumnCount, Tau, dinput.Values, 1, dresult.Values, Method);
            }
            else
            {
                throw new NotSupportedException("Can only do QR factorization for dense vectors at the moment.");
            }
        }
    }
}
