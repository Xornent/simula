using System;
using Simula.Maths.LinearAlgebra;
using Simula.Maths.Optimization.LineSearch;

namespace Simula.Maths.Optimization
{
    public abstract class BfgsMinimizerBase : MinimizerBase
    {
        /// <inheritdoc />
        /// <summary>
        /// Creates a base class for BFGS minimization
        /// </summary>
        protected BfgsMinimizerBase(double gradientTolerance, double parameterTolerance, double functionProgressTolerance, int maximumIterations) : base(gradientTolerance, parameterTolerance, functionProgressTolerance, maximumIterations)
        {
        }


        protected int DoBfgsUpdate(ref ExitCondition currentExitCondition, WolfeLineSearch lineSearcher, ref Matrix<double> inversePseudoHessian, ref Vector<double> lineSearchDirection, ref IObjectiveFunction previousPoint, ref LineSearchResult lineSearchResult, ref IObjectiveFunction candidate, ref Vector<double> step, ref int totalLineSearchSteps, ref int iterationsWithNontrivialLineSearch)
        {
            int iterations;
            for (iterations = 1; iterations < MaximumIterations; ++iterations)
            {
                double startingStepSize;
                double maxLineSearchStep;
                lineSearchDirection = CalculateSearchDirection(ref inversePseudoHessian, out maxLineSearchStep, out startingStepSize, previousPoint, candidate, step);

                try
                {
                    lineSearchResult = lineSearcher.FindConformingStep(candidate, lineSearchDirection, startingStepSize, maxLineSearchStep);
                }
                catch (Exception e)
                {
                    throw new InnerOptimizationException("Line search failed.", e);
                }

                iterationsWithNontrivialLineSearch += lineSearchResult.Iterations > 0 ? 1 : 0;
                totalLineSearchSteps += lineSearchResult.Iterations;

                step = lineSearchResult.FunctionInfoAtMinimum.Point - candidate.Point;
                previousPoint = candidate;
                candidate = lineSearchResult.FunctionInfoAtMinimum;

                currentExitCondition = ExitCriteriaSatisfied(candidate, previousPoint, iterations);
                if (currentExitCondition != ExitCondition.None)
                    break;
            }

            return iterations;
        }

        protected abstract Vector<double> CalculateSearchDirection(ref Matrix<double> inversePseudoHessian,
            out double maxLineSearchStep,
            out double startingStepSize,
            IObjectiveFunction previousPoint,
            IObjectiveFunction candidate,
            Vector<double> step);
    }
}
