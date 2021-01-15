namespace Simula.Maths.LinearAlgebra.Solvers
{
    /// <summary>
    /// Iterative Calculation Status
    /// </summary>
    public enum IterationStatus
    {
        Continue = 0,
        Converged,
        Diverged,
        StoppedWithoutConvergence,
        Cancelled,
        Failure
    }
}
