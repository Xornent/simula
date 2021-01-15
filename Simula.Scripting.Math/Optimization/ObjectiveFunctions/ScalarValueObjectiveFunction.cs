using System;

namespace Simula.Maths.Optimization.ObjectiveFunctions
{
    internal class ScalarValueObjectiveFunctionEvaluation : IScalarObjectiveFunctionEvaluation
    {
        public ScalarValueObjectiveFunctionEvaluation(double point, double value)
        {
            Point = point;
            Value = value;
        }

        public double Point { get; }
        public double Value { get; }

        public double Derivative => throw new NotSupportedException();

        public double SecondDerivative => throw new NotSupportedException();
    }

    internal class ScalarValueObjectiveFunction : IScalarObjectiveFunction
    {
        public Func<double, double> Objective { get; }

        public ScalarValueObjectiveFunction(Func<double, double> objective)
        {
            Objective = objective;
        }

        public bool IsDerivativeSupported => false;

        public bool IsSecondDerivativeSupported => false;

        public IScalarObjectiveFunctionEvaluation Evaluate(double point)
        {
            return new ScalarValueObjectiveFunctionEvaluation(point, Objective(point));
        }
    }
}
