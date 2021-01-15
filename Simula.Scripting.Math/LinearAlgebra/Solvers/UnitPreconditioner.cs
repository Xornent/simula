using System;

namespace Simula.Maths.LinearAlgebra.Solvers
{
    /// <summary>
    /// A unit preconditioner. This preconditioner does not actually do anything
    /// it is only used when running an <see cref="IIterativeSolver{T}"/> without
    /// a preconditioner.
    /// </summary>
    public sealed class UnitPreconditioner<T> : IPreconditioner<T> where T : struct, IEquatable<T>, IFormattable
    {
        /// <summary>
        /// The coefficient matrix on which this preconditioner operates.
        /// Is used to check dimensions on the different vectors that are processed.
        /// </summary>
        int _size;

        /// <summary>
        /// Initializes the preconditioner and loads the internal data structures.
        /// </summary>
        /// <param name="matrix">
        /// The matrix upon which the preconditioner is based.
        /// </param>
        /// <exception cref="ArgumentException">If <paramref name="matrix"/> is not a square matrix.</exception>
        public void Initialize(Matrix<T> matrix)
        {
            if (matrix.RowCount != matrix.ColumnCount)
            {
                throw new ArgumentException("Matrix must be square.", nameof(matrix));
            }

            _size = matrix.RowCount;
        }

        /// <summary>
        /// Approximates the solution to the matrix equation <b>Ax = b</b>.
        /// </summary>
        /// <param name="rhs">The right hand side vector.</param>
        /// <param name="lhs">The left hand side vector. Also known as the result vector.</param>
        /// <exception cref="ArgumentException">
        ///   <para>
        ///     If <paramref name="rhs"/> and <paramref name="lhs"/> do not have the same size.
        ///   </para>
        ///   <para>
        ///     - or -
        ///   </para>
        ///   <para>
        ///     If the size of <paramref name="rhs"/> is different the number of rows of the coefficient matrix.
        ///   </para>
        /// </exception>
        public void Approximate(Vector<T> rhs, Vector<T> lhs)
        {
            if ((lhs.Count != rhs.Count) || (lhs.Count != _size))
            {
                throw new ArgumentException("All vectors must have the same dimensionality.");
            }

            rhs.CopyTo(lhs);
        }
    }
}
