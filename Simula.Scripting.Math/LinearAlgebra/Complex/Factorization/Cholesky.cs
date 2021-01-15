using Simula.Maths.LinearAlgebra.Factorization;

namespace Simula.Maths.LinearAlgebra.Complex.Factorization
{
    using Complex = System.Numerics.Complex;

    /// <summary>
    /// <para>A class which encapsulates the functionality of a Cholesky factorization.</para>
    /// <para>For a symmetric, positive definite matrix A, the Cholesky factorization
    /// is an lower triangular matrix L so that A = L*L'.</para>
    /// </summary>
    /// <remarks>
    /// The computation of the Cholesky factorization is done at construction time. If the matrix is not symmetric
    /// or positive definite, the constructor will throw an exception.
    /// </remarks>
    internal abstract class Cholesky : Cholesky<Complex>
    {
        protected Cholesky(Matrix<Complex> factor)
            : base(factor)
        {
        }

        /// <summary>
        /// Gets the determinant of the matrix for which the Cholesky matrix was computed.
        /// </summary>
        public override Complex Determinant
        {
            get
            {
                var det = Complex.One;
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
        public override Complex DeterminantLn
        {
            get
            {
                var det = Complex.Zero;
                for (var j = 0; j < Factor.RowCount; j++)
                {
                    det += 2.0*Factor.At(j, j).Ln();
                }

                return det;
            }
        }
    }
}
