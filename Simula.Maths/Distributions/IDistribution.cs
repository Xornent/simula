namespace Simula.Maths.Distributions
{
    /// <summary>
    /// Probability Distribution.
    /// </summary>
    /// <seealso cref="IContinuousDistribution"/>
    /// <seealso cref="IDiscreteDistribution"/>
    public interface IDistribution
    {
        /// <summary>
        /// Gets or sets the random number generator which is used to draw random samples.
        /// </summary>
        System.Random RandomSource { get; set; }
    }
}
