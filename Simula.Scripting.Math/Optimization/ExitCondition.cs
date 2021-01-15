namespace Simula.Maths.Optimization
{
    public enum ExitCondition
    {
        None,
        InvalidValues,
        ExceedIterations,
        RelativePoints,
        RelativeGradient,
        LackOfProgress,
        AbsoluteGradient,
        WeakWolfeCriteria,
        BoundTolerance,
        StrongWolfeCriteria,
        Converged,
        ManuallyStopped
    }
}
