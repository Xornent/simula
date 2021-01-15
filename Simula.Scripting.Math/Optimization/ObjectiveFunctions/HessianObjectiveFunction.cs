using System;
using Simula.Maths.LinearAlgebra;

namespace Simula.Maths.Optimization.ObjectiveFunctions
{
    internal class HessianObjectiveFunction : IObjectiveFunction
    {
        readonly Func<Vector<double>, Tuple<double, Matrix<double>>> _function;

        public HessianObjectiveFunction(Func<Vector<double>, Tuple<double, Matrix<double>>> function)
        {
            _function = function;
        }

        public IObjectiveFunction CreateNew()
        {
            return new HessianObjectiveFunction(_function);
        }

        public IObjectiveFunction Fork()
        {
            // no need to deep-clone values since they are replaced on evaluation
            return new HessianObjectiveFunction(_function)
            {
                Point = Point,
                Value = Value,
                Hessian = Hessian
            };
        }

        public bool IsGradientSupported => false;

        public bool IsHessianSupported => true;

        public void EvaluateAt(Vector<double> point)
        {
            Point = point;

            var result = _function(point);
            Value = result.Item1;
            Hessian = result.Item2;
        }

        public Vector<double> Point { get; private set; }
        public double Value { get; private set; }
        public Matrix<double> Hessian { get; private set; }

        public Vector<double> Gradient => throw new NotSupportedException();
    }
}
