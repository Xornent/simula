using System;

namespace Simula.Maths.LinearAlgebra.Factorization
{
    /// <summary>
    /// The type of QR factorization go perform.
    /// </summary>
    public enum QRMethod
    {
        /// <summary>
        /// Compute the full QR factorization of a matrix.
        /// </summary>
        Full = 0,

        /// <summary>
        /// Compute the thin QR factorization of a matrix.
        /// </summary>
        Thin = 1
    }

    /// <summary>
    /// <para>A class which encapsulates the functionality of the QR decomposition.</para>
    /// <para>Any real square matrix A (m x n) may be decomposed as A = QR where Q is an orthogonal matrix
    /// (its columns are orthogonal unit vectors meaning QTQ = I) and R is an upper triangular matrix
    /// (also called right triangular matrix).</para>
    /// </summary>
    /// <remarks>
    /// The computation of the QR decomposition is done at construction time by Householder transformation.
    /// If a <seealso cref="QRMethod.Full"/> factorization is performed, the resulting Q matrix is an m x m matrix
    /// and the R matrix is an m x n matrix. If a <seealso cref="QRMethod.Thin"/> factorization is performed, the
    /// resulting Q matrix is an m x n matrix and the R matrix is an n x n matrix.
    /// </remarks>
    /// <typeparam name="T">Supported data types are double, single, <see cref="Complex"/>, and <see cref="Complex32"/>.</typeparam>
    public abstract class QR<T> : ISolver<T>
        where T : struct, IEquatable<T>, IFormattable
    {
        readonly Lazy<Matrix<T>> _lazyR;

        protected readonly Matrix<T> FullR;
        protected readonly QRMethod Method;

        protected QR(Matrix<T> q, Matrix<T> rFull, QRMethod method)
        {
            Q = q;
            FullR = rFull;
            Method = method;

            _lazyR = new Lazy<Matrix<T>>(FullR.UpperTriangle);
        }

        /// <summary>
        /// Gets or sets orthogonal Q matrix
        /// </summary>
        public Matrix<T> Q { get; }

        /// <summary>
        /// Gets the upper triangular factor R.
        /// </summary>
        public Matrix<T> R => _lazyR.Value;

        /// <summary>
        /// Gets the absolute determinant value of the matrix for which the QR matrix was computed.
        /// </summary>
        public abstract T Determinant { get; }

        /// <summary>
        /// Gets a value indicating whether the matrix is full rank or not.
        /// </summary>
        /// <value><c>true</c> if the matrix is full rank; otherwise <c>false</c>.</value>
        public abstract bool IsFullRank { get; }

        /// <summary>
        /// Solves a system of linear equations, <b>AX = B</b>, with A QR factorized.
        /// </summary>
        /// <param name="input">The right hand side <see cref="Matrix{T}"/>, <b>B</b>.</param>
        /// <returns>The left hand side <see cref="Matrix{T}"/>, <b>X</b>.</returns>
        public virtual Matrix<T> Solve(Matrix<T> input)
        {
            var x = Matrix<T>.Build.SameAs(input, FullR.ColumnCount, input.ColumnCount, fullyMutable: true);
            Solve(input, x);
            return x;
        }

        /// <summary>
        /// Solves a system of linear equations, <b>AX = B</b>, with A QR factorized.
        /// </summary>
        /// <param name="input">The right hand side <see cref="Matrix{T}"/>, <b>B</b>.</param>
        /// <param name="result">The left hand side <see cref="Matrix{T}"/>, <b>X</b>.</param>
        public abstract void Solve(Matrix<T> input, Matrix<T> result);

        /// <summary>
        /// Solves a system of linear equations, <b>Ax = b</b>, with A QR factorized.
        /// </summary>
        /// <param name="input">The right hand side vector, <b>b</b>.</param>
        /// <returns>The left hand side <see cref="Vector{T}"/>, <b>x</b>.</returns>
        public virtual Vector<T> Solve(Vector<T> input)
        {
            var x = Vector<T>.Build.SameAs(input, FullR.ColumnCount);
            Solve(input, x);
            return x;
        }

        /// <summary>
        /// Solves a system of linear equations, <b>Ax = b</b>, with A QR factorized.
        /// </summary>
        /// <param name="input">The right hand side vector, <b>b</b>.</param>
        /// <param name="result">The left hand side <see cref="Matrix{T}"/>, <b>x</b>.</param>
        public abstract void Solve(Vector<T> input, Vector<T> result);
    }
}
