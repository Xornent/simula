using Simula.Maths.LinearAlgebra;

namespace Simula.Maths.Optimization
{
    public class MinimizationResult
    {
        public Vector<double> MinimizingPoint => FunctionInfoAtMinimum.Point;
        public IObjectiveFunction FunctionInfoAtMinimum { get; }
        public int Iterations { get; }
        public ExitCondition ReasonForExit { get; }

        public MinimizationResult(IObjectiveFunction functionInfo, int iterations, ExitCondition reasonForExit)
        {
            FunctionInfoAtMinimum = functionInfo;
            Iterations = iterations;
            ReasonForExit = reasonForExit;
        }
    }
}
