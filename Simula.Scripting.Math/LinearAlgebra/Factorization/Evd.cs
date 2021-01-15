using System;

namespace Simula.Maths.LinearAlgebra.Factorization
{
    using Maths;
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
    /// </remarks>
    /// <typeparam name="T">Supported data types are double, single, <see cref="Complex"/>, and <see cref="Complex32"/>.</typeparam>
    public abstract class Evd<T> : ISolver<T>
    where T : struct, IEquatable<T>, IFormattable
    {
        protected Evd(Matrix<T> eigenVectors, Vector<Complex> eigenValues, Matrix<T> blockDiagonal, bool isSymmetric)
        {
            EigenVectors = eigenVectors;
            EigenValues = eigenValues;
            D = blockDiagonal;
            IsSymmetric = isSymmetric;
        }

        /// <summary>
        /// Gets or sets a value indicating whether matrix is symmetric or not
        /// </summary>
        public bool IsSymmetric { get; private set; }

        /// <summary>
        /// Gets the absolute value of determinant of the square matrix for which the EVD was computed.
        /// </summary>
        public abstract T Determinant { get; }

        /// <summary>
        /// Gets the effective numerical matrix rank.
        /// </summary>
        /// <value>The number of non-negligible singular values.</value>
        public abstract int Rank { get; }

        /// <summary>
        /// Gets a value indicating whether the matrix is full rank or not.
        /// </summary>
        /// <value><c>true</c> if the matrix is full rank; otherwise <c>false</c>.</value>
        public abstract bool IsFullRank { get; }

        /// <summary>
        /// Gets or sets the eigen values (λ) of matrix in ascending value.
        /// </summary>
        public Vector<Complex> EigenValues { get; private set; }

        /// <summary>
        /// Gets or sets eigenvectors.
        /// </summary>
        public Matrix<T> EigenVectors { get; private set; }

        /// <summary>
        /// Gets or sets the block diagonal eigenvalue matrix.
        /// </summary>
        public Matrix<T> D { get; private set; }

        /// <summary>
        /// Solves a system of linear equations, <b>AX = B</b>, with A EVD factorized.
        /// </summary>
        /// <param name="input">The right hand side <see cref="Matrix{T}"/>, <b>B</b>.</param>
        /// <returns>The left hand side <see cref="Matrix{T}"/>, <b>X</b>.</returns>
        public virtual Matrix<T> Solve(Matrix<T> input)
        {
            var x = Matrix<T>.Build.SameAs(EigenVectors, EigenVectors.ColumnCount, input.ColumnCount, fullyMutable: true);
            Solve(input, x);
            return x;
        }

        /// <summary>
        /// Solves a system of linear equations, <b>AX = B</b>, with A EVD factorized.
        /// </summary>
        /// <param name="input">The right hand side <see cref="Matrix{T}"/>, <b>B</b>.</param>
        /// <param name="result">The left hand side <see cref="Matrix{T}"/>, <b>X</b>.</param>
        public abstract void Solve(Matrix<T> input, Matrix<T> result);

        /// <summary>
        /// Solves a system of linear equations, <b>Ax = b</b>, with A EVD factorized.
        /// </summary>
        /// <param name="input">The right hand side vector, <b>b</b>.</param>
        /// <returns>The left hand side <see cref="Vector{T}"/>, <b>x</b>.</returns>
        public virtual Vector<T> Solve(Vector<T> input)
        {
            var x = Vector<T>.Build.SameAs(EigenVectors, EigenVectors.ColumnCount);
            Solve(input, x);
            return x;
        }

        /// <summary>
        /// Solves a system of linear equations, <b>Ax = b</b>, with A EVD factorized.
        /// </summary>
        /// <param name="input">The right hand side vector, <b>b</b>.</param>
        /// <param name="result">The left hand side <see cref="Matrix{T}"/>, <b>x</b>.</param>
        public abstract void Solve(Vector<T> input, Vector<T> result);
    }
}
