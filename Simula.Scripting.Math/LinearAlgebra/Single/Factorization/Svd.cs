using System;
using System.Linq;
using Simula.Maths.LinearAlgebra.Factorization;

namespace Simula.Maths.LinearAlgebra.Single.Factorization
{
    /// <summary>
    /// <para>A class which encapsulates the functionality of the singular value decomposition (SVD).</para>
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
    internal abstract class Svd : Svd<float>
    {
        protected Svd(Vector<float> s, Matrix<float> u, Matrix<float> vt, bool vectorsComputed)
            : base(s, u, vt, vectorsComputed)
        {
        }

        /// <summary>
        /// Gets the effective numerical matrix rank.
        /// </summary>
        /// <value>The number of non-negligible singular values.</value>
        public override int Rank
        {
            get
            {
                double tolerance = Precision.EpsilonOf(S.Maximum())*Math.Max(U.RowCount, VT.RowCount);
                return S.Count(t => Math.Abs(t) > tolerance);
            }
        }

        /// <summary>
        /// Gets the two norm of the <see cref="Matrix{T}"/>.
        /// </summary>
        /// <returns>The 2-norm of the <see cref="Matrix{T}"/>.</returns>
        public override double L2Norm => Math.Abs(S[0]);

        /// <summary>
        /// Gets the condition number <b>max(S) / min(S)</b>
        /// </summary>
        /// <returns>The condition number.</returns>
        public override float ConditionNumber
        {
            get
            {
                var tmp = Math.Min(U.RowCount, VT.ColumnCount) - 1;
                return Math.Abs(S[0]) / Math.Abs(S[tmp]);
            }
        }

        /// <summary>
        /// Gets the determinant of the square matrix for which the SVD was computed.
        /// </summary>
        public override float Determinant
        {
            get
            {
                if (U.RowCount != VT.ColumnCount)
                {
                    throw new ArgumentException("Matrix must be square.");
                }

                var det = 1.0;
                foreach (var value in S)
                {
                    det *= value;
                    if (Math.Abs(value).AlmostEqual(0.0f))
                    {
                        return 0;
                    }
                }

                return Convert.ToSingle(Math.Abs(det));
            }
        }
    }
}
