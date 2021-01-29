﻿namespace Simula.Maths.Optimization
{
    public class MinimizationWithLineSearchResult : MinimizationResult
    {
        public int TotalLineSearchIterations { get; private set; }
        public int IterationsWithNonTrivialLineSearch { get; private set; }

        public MinimizationWithLineSearchResult(IObjectiveFunction functionInfo, int iterations, ExitCondition reasonForExit, int totalLineSearchIterations, int iterationsWithNonTrivialLineSearch)
            : base(functionInfo, iterations, reasonForExit)
        {
            TotalLineSearchIterations = totalLineSearchIterations;
            IterationsWithNonTrivialLineSearch = iterationsWithNonTrivialLineSearch;
        }
    }
}
