using System;

namespace Simula.Maths.LinearAlgebra.Solvers
{
    /// <summary>
    /// The base interface for classes that provide stop criteria for iterative calculations.
    /// </summary>
    public interface IIterationStopCriterion<T> where T : struct, IEquatable<T>, IFormattable
    {
        /// <summary>
        /// Determines the status of the iterative calculation based on the stop criteria stored
        /// by the current IIterationStopCriterion. Status is set to <c>Status</c> field of current object.
        /// </summary>
        /// <param name="iterationNumber">The number of iterations that have passed so far.</param>
        /// <param name="solutionVector">The vector containing the current solution values.</param>
        /// <param name="sourceVector">The right hand side vector.</param>
        /// <param name="residualVector">The vector containing the current residual vectors.</param>
        /// <remarks>
        /// The individual stop criteria may internally track the progress of the calculation based
        /// on the invocation of this method. Therefore this method should only be called if the
        /// calculation has moved forwards at least one step.
        /// </remarks>
        IterationStatus DetermineStatus(int iterationNumber, Vector<T> solutionVector, Vector<T> sourceVector, Vector<T> residualVector);

        /// <summary>
        /// Gets the current calculation status.
        /// </summary>
        /// <remarks><see langword="null" /> is not a legal value. Status should be set in <see cref="DetermineStatus"/> implementation.</remarks>
        IterationStatus Status { get; }

        /// <summary>
        /// Resets the IIterationStopCriterion to the pre-calculation state.
        /// </summary>
        /// <remarks>To implementers: Invoking this method should not clear the user defined
        /// property values, only the state that is used to track the progress of the
        /// calculation.</remarks>
        void Reset();

        IIterationStopCriterion<T> Clone();
    }
}
