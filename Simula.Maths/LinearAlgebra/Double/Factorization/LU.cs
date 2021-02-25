using Simula.Maths.LinearAlgebra.Factorization;

namespace Simula.Maths.LinearAlgebra.Double.Factorization
{
    /// <summary>
    /// <para>A class which encapsulates the functionality of an LU factorization.</para>
    /// <para>For a matrix A, the LU factorization is a pair of lower triangular matrix L and
    /// upper triangular matrix U so that A = L*U.</para>
    /// <para>In the Math.Net implementation we also store a set of pivot elements for increased
    /// numerical stability. The pivot elements encode a permutation matrix P such that P*A = L*U.</para>
    /// </summary>
    /// <remarks>
    /// The computation of the LU factorization is done at construction time.
    /// </remarks>
    internal abstract class LU : LU<double>
    {
        protected LU(Matrix<double> factors, int[] pivots)
            : base(factors, pivots)
        {
        }

        /// <summary>
        /// Gets the determinant of the matrix for which the LU factorization was computed.
        /// </summary>
        public override double Determinant
        {
            get
            {
                var det = 1.0;
                for (var j = 0; j < Factors.RowCount; j++)
                {
                    if (Pivots[j] != j)
                    {
                        det *= -Factors.At(j, j);
                    }
                    else
                    {
                        det *= Factors.At(j, j);
                    }
                }

                return det;
            }
        }
    }
}
