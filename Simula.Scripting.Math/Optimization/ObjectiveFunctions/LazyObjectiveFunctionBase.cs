using Simula.Maths.LinearAlgebra;

namespace Simula.Maths.Optimization.ObjectiveFunctions
{
    public abstract class LazyObjectiveFunctionBase : IObjectiveFunction
    {
        Vector<double> _point;

        protected bool HasFunctionValue { get; set; }
        protected double FunctionValue { get; set; }

        protected bool HasGradientValue { get; set; }
        protected Vector<double> GradientValue { get; set; }

        protected bool HasHessianValue { get; set; }
        protected Matrix<double> HessianValue { get; set; }

        protected LazyObjectiveFunctionBase(bool gradientSupported, bool hessianSupported)
        {
            IsGradientSupported = gradientSupported;
            IsHessianSupported = hessianSupported;
        }

        public abstract IObjectiveFunction CreateNew();

        public virtual IObjectiveFunction Fork()
        {
            // we need to deep-clone values since they may be updated inplace on evaluation
            LazyObjectiveFunctionBase fork = (LazyObjectiveFunctionBase)CreateNew();
            fork._point = _point?.Clone();
            fork.HasFunctionValue = HasFunctionValue;
            fork.FunctionValue = FunctionValue;
            fork.HasGradientValue = HasGradientValue;
            fork.GradientValue = GradientValue?.Clone();
            fork.HasHessianValue = HasHessianValue;
            fork.HessianValue = HessianValue?.Clone();
            return fork;
        }

        public bool IsGradientSupported { get; private set; }
        public bool IsHessianSupported { get; private set; }

        public void EvaluateAt(Vector<double> point)
        {
            _point = point;
            HasFunctionValue = false;
            HasGradientValue = false;
            HasHessianValue = false;
        }

        protected abstract void EvaluateValue();

        protected virtual void EvaluateGradient()
        {
            Gradient = null;
        }

        protected virtual void EvaluateHessian()
        {
            Hessian = null;
        }

        public Vector<double> Point => _point;

        public double Value
        {
            get
            {
                if (!HasFunctionValue)
                {
                    EvaluateValue();
                }
                return FunctionValue;
            }
            protected set
            {
                FunctionValue = value;
                HasFunctionValue = true;
            }
        }

        public Vector<double> Gradient
        {
            get
            {
                if (!HasGradientValue)
                {
                    EvaluateGradient();
                }
                return GradientValue;
            }
            protected set
            {
                GradientValue = value;
                HasGradientValue = true;
            }
        }

        public Matrix<double> Hessian
        {
            get
            {
                if (!HasHessianValue)
                {
                    EvaluateHessian();
                }
                return HessianValue;
            }
            protected set
            {
                HessianValue = value;
                HasHessianValue = true;
            }
        }
    }
}
