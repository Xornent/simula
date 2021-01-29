using System;

namespace Simula.Maths.LinearAlgebra.Solvers
{
    /// <summary>
    /// The base interface for preconditioner classes.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Preconditioners are used by iterative solvers to improve the convergence
    /// speed of the solving process. Increase in convergence speed
    /// is related to the number of iterations necessary to get a converged solution.
    /// So while in general the use of a preconditioner means that the iterative
    /// solver will perform fewer iterations it does not guarantee that the actual
    /// solution time decreases given that some preconditioners can be expensive to
    /// setup and run.
    /// </para>
    /// <para>
    /// Note that in general changes to the matrix will invalidate the preconditioner
    /// if the changes occur after creating the preconditioner.
    /// </para>
    /// </remarks>
    public interface IPreconditioner<T> where T : struct, IEquatable<T>, IFormattable
    {
        /// <summary>
        /// Initializes the preconditioner and loads the internal data structures.
        /// </summary>
        /// <param name="matrix">The matrix on which the preconditioner is based.</param>
        void Initialize(Matrix<T> matrix);

        /// <summary>
        /// Approximates the solution to the matrix equation <b>Mx = b</b>.
        /// </summary>
        /// <param name="rhs">The right hand side vector.</param>
        /// <param name="lhs">The left hand side vector. Also known as the result vector.</param>
        void Approximate(Vector<T> rhs, Vector<T> lhs);
    }
}
