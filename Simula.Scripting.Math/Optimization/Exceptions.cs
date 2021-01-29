using System;

namespace Simula.Maths.Optimization
{
    public class OptimizationException : Exception
    {
        public OptimizationException(string message)
            : base(message) { }

        public OptimizationException(string message, Exception innerException)
            : base(message, innerException) { }
    }

    public class MaximumIterationsException : OptimizationException
    {
        public MaximumIterationsException(string message)
            : base(message) { }
    }

    public class EvaluationException : OptimizationException
    {
        public IObjectiveFunctionEvaluation ObjectiveFunction { get; private set; }

        public EvaluationException(string message, IObjectiveFunctionEvaluation eval)
            : base(message)
        {
            ObjectiveFunction = eval;
        }

        public EvaluationException(string message, IObjectiveFunctionEvaluation eval, Exception innerException)
            : base(message, innerException)
        {
            ObjectiveFunction = eval;
        }

    }

    public class InnerOptimizationException : OptimizationException
    {
        public InnerOptimizationException(string message)
            : base(message) { }

        public InnerOptimizationException(string message, Exception inner_exception)
            : base(message, inner_exception) { }
    }

    public class IncompatibleObjectiveException : OptimizationException
    {
        public IncompatibleObjectiveException(string message)
            : base(message) { }
    }
}
