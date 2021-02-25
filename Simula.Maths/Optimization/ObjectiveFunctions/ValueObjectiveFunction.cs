using System;
using Simula.Maths.LinearAlgebra;

namespace Simula.Maths.Optimization.ObjectiveFunctions
{
    internal class ValueObjectiveFunction : IObjectiveFunction
    {
        readonly Func<Vector<double>, double> _function;

        public ValueObjectiveFunction(Func<Vector<double>, double> function)
        {
            _function = function;
        }

        public IObjectiveFunction CreateNew()
        {
            return new ValueObjectiveFunction(_function);
        }

        public IObjectiveFunction Fork()
        {
            // no need to deep-clone values since they are replaced on evaluation
            return new ValueObjectiveFunction(_function)
            {
                Point = Point,
                Value = Value,
            };
        }

        public bool IsGradientSupported => false;

        public bool IsHessianSupported => false;

        public void EvaluateAt(Vector<double> point)
        {
            Point = point;
            Value = _function(point);
        }

        public Vector<double> Point { get; private set; }
        public double Value { get; private set; }

        public Matrix<double> Hessian => throw new NotSupportedException();

        public Vector<double> Gradient => throw new NotSupportedException();
    }
}
