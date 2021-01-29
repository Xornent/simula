using System;
using Simula.Maths.LinearAlgebra;
using Simula.Maths.Optimization.LineSearch;

namespace Simula.Maths.Optimization
{
    public class ConjugateGradientMinimizer : IUnconstrainedMinimizer
    {
        public double GradientTolerance { get; set; }
        public int MaximumIterations { get; set; }

        public ConjugateGradientMinimizer(double gradientTolerance, int maximumIterations)
        {
            GradientTolerance = gradientTolerance;
            MaximumIterations = maximumIterations;
        }

        public MinimizationResult FindMinimum(IObjectiveFunction objective, Vector<double> initialGuess)
        {
            return Minimum(objective, initialGuess, GradientTolerance, MaximumIterations);
        }

        public static MinimizationResult Minimum(IObjectiveFunction objective, Vector<double> initialGuess, double gradientTolerance=1e-8, int maxIterations=1000)
        {
            if (!objective.IsGradientSupported)
            {
                throw new IncompatibleObjectiveException("Gradient not supported in objective function, but required for ConjugateGradient minimization.");
            }

            objective.EvaluateAt(initialGuess);
            var gradient = objective.Gradient;
            ValidateGradient(objective);

            // Check that we're not already done
            if (gradient.Norm(2.0) < gradientTolerance)
            {
                return new MinimizationResult(objective, 0, ExitCondition.AbsoluteGradient);
            }

            // Set up line search algorithm
            var lineSearcher = new WeakWolfeLineSearch(1e-4, 0.1, 1e-4, 1000);

            // First step
            var steepestDirection = -gradient;
            var searchDirection = steepestDirection;
            double initialStepSize = 100 * gradientTolerance / (gradient * gradient);

            LineSearchResult result;
            try
            {
                result = lineSearcher.FindConformingStep(objective, searchDirection, initialStepSize);
            }
            catch (Exception e)
            {
                throw new InnerOptimizationException("Line search failed.", e);
            }

            objective = result.FunctionInfoAtMinimum;
            ValidateGradient(objective);

            double stepSize = result.FinalStep;

            // Subsequent steps
            int iterations = 1;
            int totalLineSearchSteps = result.Iterations;
            int iterationsWithNontrivialLineSearch = result.Iterations > 0 ? 0 : 1;
            int steepestDescentResets = 0;
            while (objective.Gradient.Norm(2.0) >= gradientTolerance && iterations < maxIterations)
            {
                var previousSteepestDirection = steepestDirection;
                steepestDirection = -objective.Gradient;
                var searchDirectionAdjuster = Math.Max(0, steepestDirection*(steepestDirection - previousSteepestDirection)/(previousSteepestDirection*previousSteepestDirection));
                searchDirection = steepestDirection + searchDirectionAdjuster * searchDirection;
                if (searchDirection * objective.Gradient >= 0)
                {
                    searchDirection = steepestDirection;
                    steepestDescentResets += 1;
                }

                try
                {
                    result = lineSearcher.FindConformingStep(objective, searchDirection, stepSize);
                }
                catch (Exception e)
                {
                    throw new InnerOptimizationException("Line search failed.", e);
                }

                iterationsWithNontrivialLineSearch += result.Iterations == 0 ? 1 : 0;
                totalLineSearchSteps += result.Iterations;
                stepSize = result.FinalStep;
                objective = result.FunctionInfoAtMinimum;
                iterations += 1;
            }

            if (iterations == maxIterations)
            {
                throw new MaximumIterationsException(FormattableString.Invariant($"Maximum iterations ({maxIterations}) reached."));
            }

            return new MinimizationWithLineSearchResult(objective, iterations, ExitCondition.AbsoluteGradient, totalLineSearchSteps, iterationsWithNontrivialLineSearch);
        }

        static void ValidateGradient(IObjectiveFunctionEvaluation objective)
        {
            foreach (var x in objective.Gradient)
            {
                if (Double.IsNaN(x) || Double.IsInfinity(x))
                {
                    throw new EvaluationException("Non-finite gradient returned.", objective);
                }
            }
        }

        static void ValidateObjective(IObjectiveFunctionEvaluation objective)
        {
            if (Double.IsNaN(objective.Value) || Double.IsInfinity(objective.Value))
            {
                throw new EvaluationException("Non-finite objective function returned.", objective);
            }
        }
    }
}
