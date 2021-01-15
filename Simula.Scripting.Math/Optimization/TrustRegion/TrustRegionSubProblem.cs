using Simula.Maths.Optimization.TrustRegion.Subproblems;

namespace Simula.Maths.Optimization.TrustRegion
{
    public static class TrustRegionSubproblem
    {
        public static ITrustRegionSubproblem DogLeg()
        {
            return new DogLegSubproblem();
        }

        public static ITrustRegionSubproblem NewtonCG()
        {
            return new NewtonCGSubproblem();
        }
    }
}
