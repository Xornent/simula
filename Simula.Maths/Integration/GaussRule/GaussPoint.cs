namespace Simula.Maths.Integration.GaussRule
{
    /// <summary>
    /// Contains the abscissas/weights, order, and intervalBegin/intervalEnd.
    /// </summary>
    internal class GaussPoint
    {
        internal double[] Abscissas { get; private set; }

        internal double[] Weights { get; private set; }

        internal double IntervalBegin { get; private set; }

        internal double IntervalEnd { get; private set; }

        internal int Order { get; private set; }

        internal GaussPoint(double intervalBegin, double intervalEnd, int order, double[] abscissas, double[] weights)
        {
            Abscissas = abscissas;
            Weights = weights;
            IntervalBegin = intervalBegin;
            IntervalEnd = intervalEnd;
            Order = order;
        }

        internal GaussPoint(int order, double[] abscissas, double[] weights) : this(-1, 1, order, abscissas, weights)
        {
        }
    }
}