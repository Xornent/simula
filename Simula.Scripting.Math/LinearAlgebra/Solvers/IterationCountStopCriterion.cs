using System;
using System.Diagnostics;

namespace Simula.Maths.LinearAlgebra.Solvers
{
    /// <summary>
    /// Defines an <see cref="IIterationStopCriterion{T}"/> that monitors the numbers of iteration
    /// steps as stop criterion.
    /// </summary>
    public sealed class IterationCountStopCriterion<T> : IIterationStopCriterion<T> where T : struct, IEquatable<T>, IFormattable
    {
        /// <summary>
        /// The default value for the maximum number of iterations the process is allowed
        /// to perform.
        /// </summary>
        public const int DefaultMaximumNumberOfIterations = 1000;

        /// <summary>
        /// The maximum number of iterations the calculation is allowed to perform.
        /// </summary>
        int _maximumNumberOfIterations;

        /// <summary>
        /// The status of the calculation
        /// </summary>
        IterationStatus _status = IterationStatus.Continue;

        /// <summary>
        /// Initializes a new instance of the <see cref="IterationCountStopCriterion{T}"/> class with the default maximum
        /// number of iterations.
        /// </summary>
        public IterationCountStopCriterion() : this(DefaultMaximumNumberOfIterations)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IterationCountStopCriterion{T}"/> class with the specified maximum
        /// number of iterations.
        /// </summary>
        /// <param name="maximumNumberOfIterations">The maximum number of iterations the calculation is allowed to perform.</param>
        public IterationCountStopCriterion(int maximumNumberOfIterations)
        {
            if (maximumNumberOfIterations < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(maximumNumberOfIterations));
            }

            _maximumNumberOfIterations = maximumNumberOfIterations;
        }

        /// <summary>
        /// Gets or sets the maximum number of iterations the calculation is allowed to perform.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the <c>Maximum</c> is set to a negative value.</exception>
        public int MaximumNumberOfIterations
        {
            [DebuggerStepThrough]
            get => _maximumNumberOfIterations;

            [DebuggerStepThrough]
            set
            {
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _maximumNumberOfIterations = value;
            }
        }

        /// <summary>
        /// Returns the maximum number of iterations to the default.
        /// </summary>
        public void ResetMaximumNumberOfIterationsToDefault()
        {
            _maximumNumberOfIterations = DefaultMaximumNumberOfIterations;
        }

        /// <summary>
        /// Determines the status of the iterative calculation based on the stop criteria stored
        /// by the current <see cref="IterationCountStopCriterion{T}"/>. Result is set into <c>Status</c> field.
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
        public IterationStatus DetermineStatus(int iterationNumber, Vector<T> solutionVector, Vector<T> sourceVector, Vector<T> residualVector)
        {
            if (iterationNumber < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(iterationNumber));
            }

            _status = iterationNumber >= _maximumNumberOfIterations ? IterationStatus.StoppedWithoutConvergence : IterationStatus.Continue;

            return _status;
        }

        /// <summary>
        /// Gets the current calculation status.
        /// </summary>
        public IterationStatus Status
        {
            [DebuggerStepThrough]
            get => _status;
        }

        /// <summary>
        /// Resets the <see cref="IterationCountStopCriterion{T}"/> to the pre-calculation state.
        /// </summary>
        public void Reset()
        {
            _status = IterationStatus.Continue;
        }

        /// <summary>
        /// Clones the current <see cref="IterationCountStopCriterion{T}"/> and its settings.
        /// </summary>
        /// <returns>A new instance of the <see cref="IterationCountStopCriterion{T}"/> class.</returns>
        public IIterationStopCriterion<T> Clone()
        {
            return new IterationCountStopCriterion<T>(_maximumNumberOfIterations);
        }
    }
}
