using System;
using System.Linq;
using Simula.Maths.LinearAlgebra.Factorization;

namespace Simula.Maths.LinearAlgebra.Complex32.Factorization
{
    using Maths;

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
    internal abstract class Svd : Svd<Complex32>
    {
        protected Svd(Vector<Complex32> s, Matrix<Complex32> u, Matrix<Complex32> vt, bool vectorsComputed)
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
                double tolerance = Precision.EpsilonOf(S.AbsoluteMaximum().Magnitude)*Math.Max(U.RowCount, VT.RowCount);
                return S.Count(t => t.Magnitude > tolerance);
            }
        }

        /// <summary>
        /// Gets the two norm of the <see cref="Matrix{T}"/>.
        /// </summary>
        /// <returns>The 2-norm of the <see cref="Matrix{T}"/>.</returns>
        public override double L2Norm => S[0].Magnitude;

        /// <summary>
        /// Gets the condition number <b>max(S) / min(S)</b>
        /// </summary>
        /// <returns>The condition number.</returns>
        public override Complex32 ConditionNumber
        {
            get
            {
                var tmp = Math.Min(U.RowCount, VT.ColumnCount) - 1;
                return S[0].Magnitude / S[tmp].Magnitude;
            }
        }

        /// <summary>
        /// Gets the determinant of the square matrix for which the SVD was computed.
        /// </summary>
        public override Complex32 Determinant
        {
            get
            {
                if (U.RowCount != VT.ColumnCount)
                {
                    throw new ArgumentException("Matrix must be square.");
                }

                var det = Complex32.One;
                foreach (var value in S)
                {
                    det *= value;
                    if (value.Magnitude.AlmostEqual(0.0f))
                    {
                        return 0;
                    }
                }

                return det.Magnitude;
            }
        }
    }
}
