using Simula.Maths.LinearAlgebra;

namespace Simula.Maths.Optimization
{
    public interface IUnconstrainedMinimizer
    {
        MinimizationResult FindMinimum(IObjectiveFunction objective, Vector<double> initialGuess);
    }
}
