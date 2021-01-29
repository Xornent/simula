using System;
using Simula.Maths.LinearAlgebra;

namespace Simula.Maths.Optimization.ObjectiveFunctions
{
    /// <summary>
    /// Adapts an objective function with only value implemented
    /// to provide a gradient as well. Gradient calculation is
    /// done using the finite difference method, specifically
    /// forward differences.
    ///
    /// For each gradient computed, the algorithm requires an
    /// additional number of function evaluations equal to the
    /// functions's number of input parameters.
    /// </summary>
    public class ForwardDifferenceGradientObjectiveFunction : IObjectiveFunction
    {
        public IObjectiveFunction InnerObjectiveFunction { get; protected set; }
        protected Vector<double> LowerBound { get; set; }
        protected Vector<double> UpperBound { get; set; }

        protected bool ValueEvaluated { get; set; } = false;
        protected bool GradientEvaluated { get; set; } = false;
        private Vector<double> _gradient;

        public double MinimumIncrement { get; set; }
        public double RelativeIncrement { get; set; }

        public ForwardDifferenceGradientObjectiveFunction(IObjectiveFunction valueOnlyObj, Vector<double> lowerBound, Vector<double> upperBound, double relativeIncrement=1e-5, double minimumIncrement=1e-8)
        {
            InnerObjectiveFunction = valueOnlyObj;
            LowerBound = lowerBound;
            UpperBound = upperBound;
            _gradient = new LinearAlgebra.Double.DenseVector(LowerBound.Count);
            RelativeIncrement = relativeIncrement;
            MinimumIncrement = minimumIncrement;
        }

        protected void EvaluateValue()
        {
            ValueEvaluated = true;
        }

        protected void EvaluateGradient()
        {
            if (!ValueEvaluated)
                EvaluateValue();

            var tmpPoint = Point.Clone();
            var tmpObj = InnerObjectiveFunction.CreateNew();
            for (int ii = 0; ii < _gradient.Count; ++ii)
            {
                var origPoint = tmpPoint[ii];
                var relIncr = origPoint * RelativeIncrement;
                var h = Math.Max(relIncr, MinimumIncrement);
                var mult = 1;
                if (origPoint + h > UpperBound[ii])
                    mult = -1;

                tmpPoint[ii] = origPoint + mult*h;
                tmpObj.EvaluateAt(tmpPoint);
                double bumpedValue = tmpObj.Value;
                _gradient[ii] = (mult * bumpedValue - mult * InnerObjectiveFunction.Value) / h;

                tmpPoint[ii] = origPoint;
            }
            GradientEvaluated = true;
        }

        public Vector<double> Gradient
        {
            get
            {
                if (!GradientEvaluated)
                    EvaluateGradient();
                return _gradient;
            }
            protected set => _gradient = value;
        }

        public Matrix<double> Hessian => throw new NotImplementedException();

        public bool IsGradientSupported => true;

        public bool IsHessianSupported => false;

        public Vector<double> Point { get; protected set; }

        public double Value
        {
            get
            {
                if (!ValueEvaluated)
                    EvaluateValue();
                return this.InnerObjectiveFunction.Value;
            }
        }

        public IObjectiveFunction CreateNew()
        {
            var tmp = new ForwardDifferenceGradientObjectiveFunction(InnerObjectiveFunction.CreateNew(), LowerBound, UpperBound, this.RelativeIncrement, this.MinimumIncrement);
            return tmp;
        }

        public void EvaluateAt(Vector<double> point)
        {
            Point = point;
            ValueEvaluated = false;
            GradientEvaluated = false;
            InnerObjectiveFunction.EvaluateAt(point);
        }

        public IObjectiveFunction Fork()
        {
            return new ForwardDifferenceGradientObjectiveFunction(InnerObjectiveFunction.Fork(), LowerBound, UpperBound, this.RelativeIncrement, this.MinimumIncrement)
            {
                Point = Point?.Clone(),
                GradientEvaluated = GradientEvaluated,
                ValueEvaluated = ValueEvaluated,
                _gradient = _gradient?.Clone()
            };
        }
    }
}
