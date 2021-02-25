using System;

namespace Simula.Maths.LinearAlgebra.Solvers
{
    /// <summary>
    /// Defines the interface for objects that can create an iterative solver with
    /// specific settings. This interface is used to pass iterative solver creation
    /// setup information around.
    /// </summary>
    public interface IIterativeSolverSetup<T> where T : struct, IEquatable<T>, IFormattable
    {
        /// <summary>
        /// Gets the type of the solver that will be created by this setup object.
        /// </summary>
        Type SolverType { get; }

        /// <summary>
        /// Gets type of preconditioner, if any, that will be created by this setup object.
        /// </summary>
        Type PreconditionerType { get; }

        /// <summary>
        /// Creates the iterative solver to be used.
        /// </summary>
        IIterativeSolver<T> CreateSolver();

        /// <summary>
        /// Creates the preconditioner to be used by default (can be overwritten).
        /// </summary>
        IPreconditioner<T> CreatePreconditioner();

        /// <summary>
        /// Gets the relative speed of the solver.
        /// </summary>
        /// <value>Returns a value between 0 and 1, inclusive.</value>
        double SolutionSpeed { get; }

        /// <summary>
        /// Gets the relative reliability of the solver.
        /// </summary>
        /// <value>Returns a value between 0 and 1 inclusive.</value>
        double Reliability { get; }
    }
}
