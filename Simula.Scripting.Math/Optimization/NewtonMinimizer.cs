using System;
using Simula.Maths.LinearAlgebra;
using Simula.Maths.Optimization.LineSearch;

namespace Simula.Maths.Optimization
{
    public sealed class NewtonMinimizer : IUnconstrainedMinimizer
    {
        public double GradientTolerance { get; set; }
        public int MaximumIterations { get; set; }
        public bool UseLineSearch { get; set; }

        public NewtonMinimizer(double gradientTolerance, int maximumIterations, bool useLineSearch = false)
        {
            GradientTolerance = gradientTolerance;
            MaximumIterations = maximumIterations;
            UseLineSearch = useLineSearch;
        }

        public MinimizationResult FindMinimum(IObjectiveFunction objective, Vector<double> initialGuess)
        {
            return Minimum(objective, initialGuess, GradientTolerance, MaximumIterations, UseLineSearch);
        }

        public static MinimizationResult Minimum(IObjectiveFunction objective, Vector<double> initialGuess, double gradientTolerance=1e-8, int maxIterations=1000, bool useLineSearch=false)
        {
            if (!objective.IsGradientSupported)
            {
                throw new IncompatibleObjectiveException("Gradient not supported in objective function, but required for Newton minimization.");
            }

            if (!objective.IsHessianSupported)
            {
                throw new IncompatibleObjectiveException("Hessian not supported in objective function, but required for Newton minimization.");
            }

            // Check that we're not already done
            objective.EvaluateAt(initialGuess);
            ValidateGradient(objective);
            if (objective.Gradient.Norm(2.0) < gradientTolerance)
            {
                return new MinimizationResult(objective, 0, ExitCondition.AbsoluteGradient);
            }

            // Set up line search algorithm
            var lineSearcher = new WeakWolfeLineSearch(1e-4, 0.9, 1e-4, maxIterations: 1000);

            // Subsequent steps
            int iterations = 0;
            int totalLineSearchSteps = 0;
            int iterationsWithNontrivialLineSearch = 0;
            bool tmpLineSearch = false;
            while (objective.Gradient.Norm(2.0) >= gradientTolerance && iterations < maxIterations)
            {
                ValidateHessian(objective);

                var searchDirection = objective.Hessian.LU().Solve(-objective.Gradient);
                if (searchDirection * objective.Gradient >= 0)
                {
                    searchDirection = -objective.Gradient;
                    tmpLineSearch = true;
                }

                if (useLineSearch || tmpLineSearch)
                {
                    LineSearchResult result;
                    try
                    {
                        result = lineSearcher.FindConformingStep(objective, searchDirection, 1.0);
                    }
                    catch (Exception e)
                    {
                        throw new InnerOptimizationException("Line search failed.", e);
                    }

                    iterationsWithNontrivialLineSearch += result.Iterations > 0 ? 1 : 0;
                    totalLineSearchSteps += result.Iterations;
                    objective = result.FunctionInfoAtMinimum;
                }
                else
                {
                    objective.EvaluateAt(objective.Point + searchDirection);
                }

                ValidateGradient(objective);

                tmpLineSearch = false;
                iterations += 1;
            }

            if (iterations == maxIterations)
            {
                throw new MaximumIterationsException(FormattableString.Invariant($"Maximum iterations ({maxIterations}) reached."));
            }

            return new MinimizationWithLineSearchResult(objective, iterations, ExitCondition.AbsoluteGradient, totalLineSearchSteps, iterationsWithNontrivialLineSearch);
        }

        static void ValidateGradient(IObjectiveFunctionEvaluation eval)
        {
            foreach (var x in eval.Gradient)
            {
                if (Double.IsNaN(x) || Double.IsInfinity(x))
                {
                    throw new EvaluationException("Non-finite gradient returned.", eval);
                }
            }
        }

        static void ValidateHessian(IObjectiveFunctionEvaluation eval)
        {
            for (int ii = 0; ii < eval.Hessian.RowCount; ++ii)
            {
                for (int jj = 0; jj < eval.Hessian.ColumnCount; ++jj)
                {
                    if (Double.IsNaN(eval.Hessian[ii, jj]) || Double.IsInfinity(eval.Hessian[ii, jj]))
                    {
                        throw new EvaluationException("Non-finite Hessian returned.", eval);
                    }
                }
            }
        }
    }
}
