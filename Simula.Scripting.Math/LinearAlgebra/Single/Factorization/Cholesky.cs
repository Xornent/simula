using Simula.Maths.LinearAlgebra.Factorization;

namespace Simula.Maths.LinearAlgebra.Single.Factorization
{
    using System;

    /// <summary>
    /// <para>A class which encapsulates the functionality of a Cholesky factorization.</para>
    /// <para>For a symmetric, positive definite matrix A, the Cholesky factorization
    /// is an lower triangular matrix L so that A = L*L'.</para>
    /// </summary>
    /// <remarks>
    /// The computation of the Cholesky factorization is done at construction time. If the matrix is not symmetric
    /// or positive definite, the constructor will throw an exception.
    /// </remarks>
    internal abstract class Cholesky : Cholesky<float>
    {
        protected Cholesky(Matrix<float> factor)
            : base(factor)
        {
        }

        /// <summary>
        /// Gets the determinant of the matrix for which the Cholesky matrix was computed.
        /// </summary>
        public override float Determinant
        {
            get
            {
                var det = 1.0f;
                for (var j = 0; j < Factor.RowCount; j++)
                {
                    var d = Factor.At(j, j);
                    det *= d*d;
                }

                return det;
            }
        }

        /// <summary>
        /// Gets the log determinant of the matrix for which the Cholesky matrix was computed.
        /// </summary>
        public override float DeterminantLn
        {
            get
            {
                var det = 0.0f;
                for (var j = 0; j < Factor.RowCount; j++)
                {
                    det += 2.0f*Convert.ToSingle(Math.Log(Factor.At(j, j)));
                }

                return det;
            }
        }
    }
}
