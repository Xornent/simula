using Simula.Maths.LinearAlgebra.Factorization;

namespace Simula.Maths.LinearAlgebra.Complex32.Factorization
{
    using Maths;

    /// <summary>
    /// <para>A class which encapsulates the functionality of a Cholesky factorization.</para>
    /// <para>For a symmetric, positive definite matrix A, the Cholesky factorization
    /// is an lower triangular matrix L so that A = L*L'.</para>
    /// </summary>
    /// <remarks>
    /// The computation of the Cholesky factorization is done at construction time. If the matrix is not symmetric
    /// or positive definite, the constructor will throw an exception.
    /// </remarks>
    internal abstract class Cholesky : Cholesky<Complex32>
    {
        protected Cholesky(Matrix<Complex32> factor)
            : base(factor)
        {
        }

        /// <summary>
        /// Gets the determinant of the matrix for which the Cholesky matrix was computed.
        /// </summary>
        public override Complex32 Determinant
        {
            get
            {
                var det = Complex32.One;
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
        public override Complex32 DeterminantLn
        {
            get
            {
                var det = Complex32.Zero;
                for (var j = 0; j < Factor.RowCount; j++)
                {
                    det += 2.0f*Factor.At(j, j).NaturalLogarithm();
                }

                return det;
            }
        }
    }
}
