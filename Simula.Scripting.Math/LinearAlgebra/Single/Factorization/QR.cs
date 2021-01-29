using System;
using Simula.Maths.LinearAlgebra.Factorization;

namespace Simula.Maths.LinearAlgebra.Single.Factorization
{
    /// <summary>
    /// <para>A class which encapsulates the functionality of the QR decomposition.</para>
    /// <para>Any real square matrix A (m x n) may be decomposed as A = QR where Q is an orthogonal matrix
    /// (its columns are orthogonal unit vectors meaning QTQ = I) and R is an upper triangular matrix
    /// (also called right triangular matrix).</para>
    /// </summary>
    /// <remarks>
    /// The computation of the QR decomposition is done at construction time by Householder transformation.
    /// If a <seealso cref="QRMethod.Full"/> factorization is performed, the resulting Q matrix is an m x m matrix
    /// and the R matrix is an m x n matrix. If a <seealso cref="QRMethod.Thin"/> factorization is performed, the
    /// resulting Q matrix is an m x n matrix and the R matrix is an n x n matrix.
    /// </remarks>
    internal abstract class QR : QR<float>
    {
        protected QR(Matrix<float> q, Matrix<float> rFull, QRMethod method)
            : base(q, rFull, method)
        {
        }

        /// <summary>
        /// Gets the absolute determinant value of the matrix for which the QR matrix was computed.
        /// </summary>
        public override float Determinant
        {
            get
            {
                if (FullR.RowCount != FullR.ColumnCount)
                {
                    throw new ArgumentException("Matrix must be square.");
                }

                var det = 1.0;
                for (var i = 0; i < FullR.ColumnCount; i++)
                {
                    det *= FullR.At(i, i);
                    if (Math.Abs(FullR.At(i, i)).AlmostEqual(0.0f))
                    {
                        return 0;
                    }
                }

                return Convert.ToSingle(Math.Abs(det));
            }
        }

        /// <summary>
        /// Gets a value indicating whether the matrix is full rank or not.
        /// </summary>
        /// <value><c>true</c> if the matrix is full rank; otherwise <c>false</c>.</value>
        public override bool IsFullRank
        {
            get
            {
                for (var i = 0; i < FullR.ColumnCount; i++)
                {
                    if (Math.Abs(FullR.At(i, i)).AlmostEqual(0.0f))
                    {
                        return false;
                    }
                }

                return true;
            }
        }
    }
}
