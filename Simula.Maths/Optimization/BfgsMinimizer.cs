using System;
using Simula.Maths.LinearAlgebra;
using Simula.Maths.Optimization.LineSearch;

namespace Simula.Maths.Optimization
{
    /// <summary>
    /// Broyden–Fletcher–Goldfarb–Shanno (BFGS) algorithm is an iterative method for solving unconstrained nonlinear optimization problems
    /// </summary>
    public class BfgsMinimizer : BfgsMinimizerBase, IUnconstrainedMinimizer
    {
        /// <summary>
        /// Creates BFGS minimizer
        /// </summary>
        /// <param name="gradientTolerance">The gradient tolerance</param>
        /// <param name="parameterTolerance">The parameter tolerance</param>
        /// <param name="functionProgressTolerance">The function progress tolerance</param>
        /// <param name="maximumIterations">The maximum number of iterations</param>
        public BfgsMinimizer(double gradientTolerance, double parameterTolerance, double functionProgressTolerance, int maximumIterations=1000)
            :base(gradientTolerance,parameterTolerance,functionProgressTolerance,maximumIterations)
        {
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
                throw new IncompatibleObjectiveException("Gradient not supported in objective function, but required for BFGS minimization.");

            objective.EvaluateAt(initialGuess);
            ValidateGradientAndObjective(objective);

            // Check that we're not already done
            ExitCondition currentExitCondition = ExitCriteriaSatisfied(objective, null, 0);
            if (currentExitCondition != ExitCondition.None)
                return new MinimizationResult(objective, 0, currentExitCondition);

            // Set up line search algorithm
            var lineSearcher = new WeakWolfeLineSearch(1e-4, 0.9, Math.Max(ParameterTolerance, 1e-10), 1000);

            // First step
            var inversePseudoHessian = CreateMatrix.DenseIdentity<double>(initialGuess.Count);
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

            // Subsequent steps
            Matrix<double> I = CreateMatrix.DiagonalIdentity<double>(initialGuess.Count);
            int iterations;
            int totalLineSearchSteps = lineSearchResult.Iterations;
            int iterationsWithNontrivialLineSearch = lineSearchResult.Iterations > 0 ? 0 : 1;
            iterations = DoBfgsUpdate(ref currentExitCondition, lineSearcher, ref inversePseudoHessian, ref lineSearchDirection, ref previousPoint, ref lineSearchResult, ref candidate, ref step, ref totalLineSearchSteps, ref iterationsWithNontrivialLineSearch);

            if (iterations == MaximumIterations && currentExitCondition == ExitCondition.None)
                throw new MaximumIterationsException(FormattableString.Invariant($"Maximum iterations ({MaximumIterations}) reached."));

            return new MinimizationWithLineSearchResult(candidate, iterations, ExitCondition.AbsoluteGradient, totalLineSearchSteps, iterationsWithNontrivialLineSearch);
        }

        protected override Vector<double> CalculateSearchDirection(ref Matrix<double> inversePseudoHessian,
            out double maxLineSearchStep,
            out double startingStepSize,
            IObjectiveFunction previousPoint,
            IObjectiveFunction candidate,
            Vector<double> step)
        {
            startingStepSize = 1.0;
            maxLineSearchStep = double.PositiveInfinity;

            Vector<double> lineSearchDirection;
            var y = candidate.Gradient - previousPoint.Gradient;

            double sy = step * y;
            inversePseudoHessian = inversePseudoHessian + ((sy + y * inversePseudoHessian * y) / Math.Pow(sy, 2.0)) * step.OuterProduct(step) - ((inversePseudoHessian * y.ToColumnMatrix()) * step.ToRowMatrix() + step.ToColumnMatrix() * (y.ToRowMatrix() * inversePseudoHessian)) * (1.0 / sy);
            lineSearchDirection = -inversePseudoHessian * candidate.Gradient;

            if (lineSearchDirection * candidate.Gradient >= 0.0)
            {
                lineSearchDirection = -candidate.Gradient;
                inversePseudoHessian = CreateMatrix.DenseIdentity<double>(candidate.Point.Count);
            }

            return lineSearchDirection;
        }
    }
}
