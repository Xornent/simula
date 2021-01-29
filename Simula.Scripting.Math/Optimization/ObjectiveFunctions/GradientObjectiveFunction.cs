using System;
using Simula.Maths.LinearAlgebra;

namespace Simula.Maths.Optimization.ObjectiveFunctions
{
    internal class GradientObjectiveFunction : IObjectiveFunction
    {
        readonly Func<Vector<double>, Tuple<double, Vector<double>>> _function;

        public GradientObjectiveFunction(Func<Vector<double>, Tuple<double, Vector<double>>> function)
        {
            _function = function;
        }

        public IObjectiveFunction CreateNew()
        {
            return new GradientObjectiveFunction(_function);
        }

        public IObjectiveFunction Fork()
        {
            // no need to deep-clone values since they are replaced on evaluation
            return new GradientObjectiveFunction(_function)
            {
                Point = Point,
                Value = Value,
                Gradient = Gradient
            };
        }

        public bool IsGradientSupported => true;

        public bool IsHessianSupported => false;

        public void EvaluateAt(Vector<double> point)
        {
            Point = point;

            var result = _function(point);
            Value = result.Item1;
            Gradient = result.Item2;
        }

        public Vector<double> Point { get; private set; }
        public double Value { get; private set; }
        public Vector<double> Gradient { get; private set; }

        public Matrix<double> Hessian => throw new NotSupportedException();
    }
}
