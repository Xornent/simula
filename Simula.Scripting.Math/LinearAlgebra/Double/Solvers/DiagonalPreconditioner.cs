using System;
using Simula.Maths.LinearAlgebra.Solvers;

namespace Simula.Maths.LinearAlgebra.Double.Solvers
{
    /// <summary>
    /// A diagonal preconditioner. The preconditioner uses the inverse
    /// of the matrix diagonal as preconditioning values.
    /// </summary>
    public sealed class DiagonalPreconditioner : IPreconditioner<double>
    {
        /// <summary>
        /// The inverse of the matrix diagonal.
        /// </summary>
        double[] _inverseDiagonals;

        /// <summary>
        /// Returns the decomposed matrix diagonal.
        /// </summary>
        /// <returns>The matrix diagonal.</returns>
        internal DiagonalMatrix DiagonalEntries()
        {
            var result = new DiagonalMatrix(_inverseDiagonals.Length);
            for (var i = 0; i < _inverseDiagonals.Length; i++)
            {
                result[i, i] = 1/_inverseDiagonals[i];
            }

            return result;
        }

        /// <summary>
        /// Initializes the preconditioner and loads the internal data structures.
        /// </summary>
        /// <param name="matrix">
        /// The <see cref="Matrix"/> upon which this preconditioner is based.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="matrix"/> is <see langword="null" />. </exception>
        /// <exception cref="ArgumentException">If <paramref name="matrix"/> is not a square matrix.</exception>
        public void Initialize(Matrix<double> matrix)
        {
            if (matrix.RowCount != matrix.ColumnCount)
            {
                throw new ArgumentException("Matrix must be square.", nameof(matrix));
            }

            _inverseDiagonals = new double[matrix.RowCount];
            for (var i = 0; i < matrix.RowCount; i++)
            {
                _inverseDiagonals[i] = 1/matrix[i, i];
            }
        }

        /// <summary>
        /// Approximates the solution to the matrix equation <b>Ax = b</b>.
        /// </summary>
        /// <param name="rhs">The right hand side vector.</param>
        /// <param name="lhs">The left hand side vector. Also known as the result vector.</param>
        public void Approximate(Vector<double> rhs, Vector<double> lhs)
        {
            if (_inverseDiagonals == null)
            {
                throw new ArgumentException("The requested matrix does not exist.");
            }

            if ((lhs.Count != rhs.Count) || (lhs.Count != _inverseDiagonals.Length))
            {
                throw new ArgumentException("All vectors must have the same dimensionality.", nameof(rhs));
            }

            for (var i = 0; i < _inverseDiagonals.Length; i++)
            {
                lhs[i] = rhs[i]*_inverseDiagonals[i];
            }
        }
    }
}
