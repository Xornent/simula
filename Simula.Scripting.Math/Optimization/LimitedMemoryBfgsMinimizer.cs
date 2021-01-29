using System;
using System.Collections.Generic;
using System.Linq;
using Simula.Maths.LinearAlgebra;
using Simula.Maths.Optimization.LineSearch;

namespace Simula.Maths.Optimization
{
    /// <summary>
    /// Limited Memory version of Broyden–Fletcher–Goldfarb–Shanno (BFGS) algorithm
    /// </summary>
    public class LimitedMemoryBfgsMinimizer : MinimizerBase, IUnconstrainedMinimizer
    {
        public int Memory { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// Creates L-BFGS minimizer
        /// </summary>
        /// <param name="memory">Numbers of gradients and steps to store.</param>
        public LimitedMemoryBfgsMinimizer(double gradientTolerance, double parameterTolerance, double functionProgressTolerance, int memory, int maximumIterations=1000) : base(gradientTolerance, parameterTolerance, functionProgressTolerance, maximumIterations)
        {
            Memory = memory;
        }

        /// <summary>
        /// Find the minimum of the objective function given lower and upper bounds
        /// </summary>
        /// <param name="objective">The objective function, must support a gradient</param>
        /// <param name="initialGuess">The initial guess</param>
        /// <returns>The MinimizationResult which contains the minimum and the ExitCondition</returns>
        public MinimizationResult FindMinimum(IObjectiveFunction objective, Vector<double> initialGuess)
        {
            if (!objective.IsGradientSupported)
                throw new IncompatibleObjectiveException("Gradient not supported in objective function, but required for L-BFGS minimization.");

            objective.EvaluateAt(initialGuess);
            ValidateGradientAndObjective(objective);

            // Check that we're not already done
            ExitCondition currentExitCondition = ExitCriteriaSatisfied(objective, null, 0);
            if (currentExitCondition != ExitCondition.None)
                return new MinimizationResult(objective, 0, currentExitCondition);

            // Set up line search algorithm
            var lineSearcher = new WeakWolfeLineSearch(1e-4, 0.9, Math.Max(ParameterTolerance, 1e-10), 1000);

            // First step

            var lineSearchDirection = -objective.Gradient;
            var stepSize = 100 * GradientTolerance / (lineSearchDirection * lineSearchDirection);

            var previousPoint = objective;

            LineSearchResult lineSearchResult;
            try
            {
                lineSearchResult = lineSearcher.FindConformingStep(objective, lineSearchDirection, stepSize);
            }
            catch (OptimizationException e)
            {
                throw new InnerOptimizationException("Line search failed.", e);
            }
            catch (ArgumentException e)
            {
                throw new InnerOptimizationException("Line search failed.", e);
            }

            var candidate = lineSearchResult.FunctionInfoAtMinimum;
            ValidateGradientAndObjective(candidate);

            var gradient = candidate.Gradient;
            var step = candidate.Point - initialGuess;
            var yk = candidate.Gradient - previousPoint.Gradient;
            var ykhistory = new List<Vector<double>>() {yk};
            var skhistory = new List<Vector<double>>() {step};
            var rhokhistory = new List<double>() {1.0/yk.DotProduct(step)};

            // Subsequent steps
            int iterations = 1;
            int totalLineSearchSteps = lineSearchResult.Iterations;
            int iterationsWithNontrivialLineSearch = lineSearchResult.Iterations > 0 ? 0 : 1;
            previousPoint = candidate;
            while (iterations++ < MaximumIterations && previousPoint.Gradient.Norm(2) >= GradientTolerance)
            {
                lineSearchDirection = -ApplyLbfgsUpdate(previousPoint, ykhistory, skhistory, rhokhistory);
                var directionalDerivative = previousPoint.Gradient.DotProduct(lineSearchDirection);
                if (directionalDerivative > 0)
                    throw new InnerOptimizationException("Direction is not a descent direction.");
                try
                {
                    lineSearchResult = lineSearcher.FindConformingStep(previousPoint, lineSearchDirection, 1.0);
                }
                catch (OptimizationException e)
                {
                    throw new InnerOptimizationException("Line search failed.", e);
                }
                catch (ArgumentException e)
                {
                    throw new InnerOptimizationException("Line search failed.", e);
                }
                iterationsWithNontrivialLineSearch += lineSearchResult.Iterations > 0 ? 1 : 0;
                totalLineSearchSteps += lineSearchResult.Iterations;

                candidate = lineSearchResult.FunctionInfoAtMinimum;
                currentExitCondition = ExitCriteriaSatisfied(candidate, previousPoint, iterations);
                if (currentExitCondition != ExitCondition.None)
                    break;
                step = candidate.Point - previousPoint.Point;
                yk = candidate.Gradient - previousPoint.Gradient;
                ykhistory.Add(yk);
                skhistory.Add(step);
                rhokhistory.Add(1.0/yk.DotProduct(step));
                previousPoint = candidate;
                if (ykhistory.Count > Memory)
                {
                    ykhistory.RemoveAt(0);
                    skhistory.RemoveAt(0);
                    rhokhistory.RemoveAt(0);
                }
            }

            if (iterations == MaximumIterations && currentExitCondition == ExitCondition.None)
                throw new MaximumIterationsException(FormattableString.Invariant($"Maximum iterations ({MaximumIterations}) reached."));

            return new MinimizationWithLineSearchResult(candidate, iterations, ExitCondition.AbsoluteGradient, totalLineSearchSteps, iterationsWithNontrivialLineSearch);
        }

        private Vector<double> ApplyLbfgsUpdate(IObjectiveFunction previousPoint, List<Vector<double>> ykhistory, List<Vector<double>> skhistory, List<double> rhokhistory)
        {
            var q = previousPoint.Gradient.Clone();
            var alphas = new Stack<double>();
            for (int k = ykhistory.Count - 1; k >= 0; k--)
            {
                var alpha = rhokhistory[k]*q.DotProduct(skhistory[k]);
                alphas.Push(alpha);
                q -= alpha*ykhistory[k];
            }
            var yk = ykhistory.Last();
            var sk = skhistory.Last();
            q *= yk.DotProduct(sk)/yk.DotProduct(yk);
            for (int k = 0; k < ykhistory.Count; k++)
            {
                var beta = rhokhistory[k]*ykhistory[k].DotProduct(q);
                q += skhistory[k]*(alphas.Pop() - beta);
            }
            return q;
        }
    }
}
