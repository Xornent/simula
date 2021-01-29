using System;
using Simula.Maths.LinearAlgebra;
using Simula.Maths.LinearAlgebra.Double;

namespace Simula.Maths.Optimization
{
    public abstract class MinimizerBase
    {
        public double GradientTolerance { get; set; }
        public double ParameterTolerance { get; set; }
        public double FunctionProgressTolerance { get; set; }
        public int MaximumIterations { get; set; }

        protected const double VerySmall = 1e-15;

        /// <summary>
        /// Creates a base class for minimization
        /// </summary>
        /// <param name="gradientTolerance">The gradient tolerance</param>
        /// <param name="parameterTolerance">The parameter tolerance</param>
        /// <param name="functionProgressTolerance">The function progress tolerance</param>
        /// <param name="maximumIterations">The maximum number of iterations</param>
        protected MinimizerBase(double gradientTolerance, double parameterTolerance, double functionProgressTolerance, int maximumIterations)
        {
            GradientTolerance = gradientTolerance;
            ParameterTolerance = parameterTolerance;
            FunctionProgressTolerance = functionProgressTolerance;
            MaximumIterations = maximumIterations;
        }

        protected ExitCondition ExitCriteriaSatisfied(IObjectiveFunctionEvaluation candidatePoint, IObjectiveFunctionEvaluation lastPoint, int iterations)
        {
            Vector<double> relGrad = new DenseVector(candidatePoint.Point.Count);
            double relativeGradient = 0.0;
            double normalizer = Math.Max(Math.Abs(candidatePoint.Value), 1.0);
            for (int ii = 0; ii < relGrad.Count; ++ii)
            {
                double projectedGradient = GetProjectedGradient(candidatePoint, ii);

                double tmp = projectedGradient *
                    Math.Max(Math.Abs(candidatePoint.Point[ii]), 1.0) / normalizer;
                relativeGradient = Math.Max(relativeGradient, Math.Abs(tmp));
            }
            if (relativeGradient < GradientTolerance)
            {
                return ExitCondition.RelativeGradient;
            }

            if (lastPoint != null)
            {
                double mostProgress = 0.0;
                for (int ii = 0; ii < candidatePoint.Point.Count; ++ii)
                {
                    var tmp = Math.Abs(candidatePoint.Point[ii] - lastPoint.Point[ii]) /
                        Math.Max(Math.Abs(lastPoint.Point[ii]), 1.0);
                    mostProgress = Math.Max(mostProgress, tmp);
                }
                if (mostProgress < ParameterTolerance)
                {
                    return ExitCondition.LackOfProgress;
                }

                double functionChange = candidatePoint.Value - lastPoint.Value;
                if (iterations > 500 && functionChange < 0 && Math.Abs(functionChange) < FunctionProgressTolerance)
                    return ExitCondition.LackOfProgress;
            }

            return ExitCondition.None;
        }

        protected virtual double GetProjectedGradient(IObjectiveFunctionEvaluation candidatePoint, int ii)
        {
            return candidatePoint.Gradient[ii];
        }

        protected void ValidateGradientAndObjective(IObjectiveFunctionEvaluation eval)
        {
            foreach (var x in eval.Gradient)
            {
                if (Double.IsNaN(x) || Double.IsInfinity(x))
                    throw new EvaluationException("Non-finite gradient returned.", eval);
            }
            if (Double.IsNaN(eval.Value) || Double.IsInfinity(eval.Value))
                throw new EvaluationException("Non-finite objective function returned.", eval);
        }
    }
}
