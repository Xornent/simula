using Simula.Maths.LinearAlgebra.Factorization;

namespace Simula.Maths.LinearAlgebra.Double.Factorization
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
    internal abstract class Cholesky : Cholesky<double>
    {
        protected Cholesky(Matrix<double> factor)
            : base(factor)
        {
        }

        /// <summary>
        /// Gets the determinant of the matrix for which the Cholesky matrix was computed.
        /// </summary>
        public override double Determinant
        {
            get
            {
                var det = 1.0;
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
        public override double DeterminantLn
        {
            get
            {
                var det = 0.0;
                for (var j = 0; j < Factor.RowCount; j++)
                {
                    det += 2*Math.Log(Factor.At(j, j));
                }

                return det;
            }
        }
    }
}
