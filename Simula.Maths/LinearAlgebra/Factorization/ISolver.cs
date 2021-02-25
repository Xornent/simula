using System;

namespace Simula.Maths.LinearAlgebra.Factorization
{
    /// <summary>
    /// Classes that solves a system of linear equations, <c>AX = B</c>.
    /// </summary>
    /// <typeparam name="T">Supported data types are double, single, <see cref="Complex"/>, and <see cref="Complex32"/>.</typeparam>
    public interface ISolver<T> where T : struct, IEquatable<T>, IFormattable
    {
        /// <summary>
        /// Solves a system of linear equations, <c>AX = B</c>.
        /// </summary>
        /// <param name="input">The right hand side Matrix, <c>B</c>.</param>
        /// <returns>The left hand side Matrix, <c>X</c>.</returns>
        Matrix<T> Solve(Matrix<T> input);

        /// <summary>
        /// Solves a system of linear equations, <c>AX = B</c>.
        /// </summary>
        /// <param name="input">The right hand side Matrix, <c>B</c>.</param>
        /// <param name="result">The left hand side Matrix, <c>X</c>.</param>
        void Solve(Matrix<T> input, Matrix<T> result);

        /// <summary>
        /// Solves a system of linear equations, <c>Ax = b</c>
        /// </summary>
        /// <param name="input">The right hand side vector, <c>b</c>.</param>
        /// <returns>The left hand side Vector, <c>x</c>.</returns>
        Vector<T> Solve(Vector<T> input);

        /// <summary>
        /// Solves a system of linear equations, <c>Ax = b</c>.
        /// </summary>
        /// <param name="input">The right hand side vector, <c>b</c>.</param>
        /// <param name="result">The left hand side Matrix>, <c>x</c>.</param>
        void Solve(Vector<T> input, Vector<T> result);
    }
}
