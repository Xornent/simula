using Simula.Maths.LinearAlgebra;

namespace Simula.Maths.Optimization.TrustRegion
{
    public interface ITrustRegionSubproblem
    {
        Vector<double> Pstep { get; }
        bool HitBoundary { get; }

        void Solve(IObjectiveModel objective, double radius);
    }
}
