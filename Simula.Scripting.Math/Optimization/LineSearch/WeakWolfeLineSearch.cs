using System;
using Simula.Maths.LinearAlgebra;

namespace Simula.Maths.Optimization.LineSearch
{
    /// <summary>
    /// Search for a step size alpha that satisfies the weak Wolfe conditions. The weak Wolfe
    /// Conditions are
    /// i)  Armijo Rule:         f(x_k + alpha_k p_k) &lt;= f(x_k) + c1 alpha_k p_k^T g(x_k)
    /// ii) Curvature Condition: p_k^T g(x_k + alpha_k p_k) &gt;= c2 p_k^T g(x_k)
    /// where g(x) is the gradient of f(x), 0 &lt; c1 &lt; c2 &lt; 1.
    ///
    /// Implementation is based on http://www.math.washington.edu/~burke/crs/408/lectures/L9-weak-Wolfe.pdf
    ///
    /// references:
    /// http://en.wikipedia.org/wiki/Wolfe_conditions
    /// http://www.math.washington.edu/~burke/crs/408/lectures/L9-weak-Wolfe.pdf
    /// </summary>
    public class WeakWolfeLineSearch : WolfeLineSearch
    {
        public WeakWolfeLineSearch(double c1, double c2, double parameterTolerance, int maxIterations = 10)
            : base(c1,c2,parameterTolerance,maxIterations)
        {
            // Validation in base class
        }

        protected override ExitCondition WolfeExitCondition => ExitCondition.WeakWolfeCriteria;

        protected override bool WolfeCondition(double stepDd, double initialDd)
        {
            return stepDd < C2 * initialDd;
        }

        protected override void ValidateValue(IObjectiveFunctionEvaluation eval)
        {
            if (!IsFinite(eval.Value))
            {
                throw new EvaluationException(FormattableString.Invariant($"Non-finite value returned by objective function: {eval.Value}"), eval);
            }
        }

        protected override void ValidateInputArguments(IObjectiveFunctionEvaluation startingPoint, Vector<double> searchDirection, double initialStep, double upperBound)
        {
            if (!startingPoint.IsGradientSupported)
                throw new ArgumentException("objective function does not support gradient");
        }

        protected override void ValidateGradient(IObjectiveFunctionEvaluation eval)
        {
            foreach (double x in eval.Gradient)
            {
                if (!IsFinite(x))
                {
                    throw new EvaluationException(FormattableString.Invariant($"Non-finite value returned by gradient: {x}"), eval);
                }
            }
        }

        static bool IsFinite(double x)
        {
            return !(double.IsNaN(x) || double.IsInfinity(x));
        }
    }
}
