using Simula.Maths.LinearAlgebra.Factorization;

namespace Simula.Maths.LinearAlgebra.Double.Factorization
{
    using Complex = System.Numerics.Complex;

    /// <summary>
    /// Eigenvalues and eigenvectors of a real matrix.
    /// </summary>
    /// <remarks>
    /// If A is symmetric, then A = V*D*V' where the eigenvalue matrix D is
    /// diagonal and the eigenvector matrix V is orthogonal.
    /// I.e. A = V*D*V' and V*VT=I.
    /// If A is not symmetric, then the eigenvalue matrix D is block diagonal
    /// with the real eigenvalues in 1-by-1 blocks and any complex eigenvalues,
    /// lambda + i*mu, in 2-by-2 blocks, [lambda, mu; -mu, lambda].  The
    /// columns of V represent the eigenvectors in the sense that A*V = V*D,
    /// i.e. A.Multiply(V) equals V.Multiply(D).  The matrix V may be badly
    /// conditioned, or even singular, so the validity of the equation
    /// A = V*D*Inverse(V) depends upon V.Condition().
    /// Matrix V is encoded in the property EigenVectors in the way that:
    ///  - column corresponding to real eigenvalue represents real eigenvector,
    ///  - columns corresponding to the pair of complex conjugate eigenvalues
    ///    lambda[i] and lambda[i+1] encode real and imaginary parts of eigenvectors.
    /// </remarks>
    internal abstract class Evd : Evd<double>
    {
        protected Evd(Matrix<double> eigenVectors, Vector<Complex> eigenValues, Matrix<double> blockDiagonal, bool isSymmetric)
            : base(eigenVectors, eigenValues, blockDiagonal, isSymmetric)
        {
        }

        /// <summary>
        /// Gets the absolute value of determinant of the square matrix for which the EVD was computed.
        /// </summary>
        public override double Determinant
        {
            get
            {
                var det = Complex.One;
                for (var i = 0; i < EigenValues.Count; i++)
                {
                    det *= EigenValues[i];

                    if (EigenValues[i].AlmostEqual(Complex.Zero))
                    {
                        return 0;
                    }
                }

                return det.Magnitude;
            }
        }

        /// <summary>
        /// Gets the effective numerical matrix rank.
        /// </summary>
        /// <value>The number of non-negligible singular values.</value>
        public override int Rank
        {
            get
            {
                var rank = 0;
                for (var i = 0; i < EigenValues.Count; i++)
                {
                    if (EigenValues[i].AlmostEqual(Complex.Zero))
                    {
                        continue;
                    }

                    rank++;
                }

                return rank;
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
                for (var i = 0; i < EigenValues.Count; i++)
                {
                    if (EigenValues[i].AlmostEqual(Complex.Zero))
                    {
                        return false;
                    }
                }

                return true;
            }
        }
    }
}
