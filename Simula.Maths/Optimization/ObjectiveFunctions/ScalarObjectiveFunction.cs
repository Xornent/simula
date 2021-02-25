using System;

namespace Simula.Maths.Optimization.ObjectiveFunctions
{
    internal class LazyScalarObjectiveFunctionEvaluation : IScalarObjectiveFunctionEvaluation
    {
        double? _value;
        double? _derivative;
        double? _secondDerivative;
        readonly ScalarObjectiveFunction _objectiveObject;
        readonly double _point;

        public LazyScalarObjectiveFunctionEvaluation(ScalarObjectiveFunction f, double point)
        {
            _objectiveObject = f;
            _point = point;
        }

        double SetValue()
        {
            _value = _objectiveObject.Objective(_point);
            return _value.Value;
        }

        double SetDerivative()
        {
            _derivative = _objectiveObject.Derivative(_point);
            return _derivative.Value;
        }

        double SetSecondDerivative()
        {
            _secondDerivative = _objectiveObject.SecondDerivative(_point);
            return _secondDerivative.Value;
        }

        public double Point => _point;
        public double Value => _value ?? SetValue();
        public double Derivative => _derivative ?? SetDerivative();
        public double SecondDerivative => _secondDerivative ?? SetSecondDerivative();
    }

    internal class ScalarObjectiveFunction : IScalarObjectiveFunction
    {
        public Func<double, double> Objective { get; }
        public Func<double, double> Derivative { get; }
        public Func<double, double> SecondDerivative { get; }

        public ScalarObjectiveFunction(Func<double, double> objective)
        {
            Objective = objective;
            Derivative = null;
            SecondDerivative = null;
        }

        public ScalarObjectiveFunction(Func<double, double> objective, Func<double, double> derivative)
        {
            Objective = objective;
            Derivative = derivative;
            SecondDerivative = null;
        }

        public ScalarObjectiveFunction(Func<double, double> objective, Func<double, double> derivative, Func<double,double> secondDerivative)
        {
            Objective = objective;
            Derivative = derivative;
            SecondDerivative = secondDerivative;
        }

        public bool IsDerivativeSupported => Derivative != null;

        public bool IsSecondDerivativeSupported => SecondDerivative != null;

        public IScalarObjectiveFunctionEvaluation Evaluate(double point)
        {
            return new LazyScalarObjectiveFunctionEvaluation(this, point);
        }
    }
}
