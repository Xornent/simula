namespace Simula.Maths.Statistics.Mcmc
{
    using System;
    using Distributions;

    /// <summary>
    /// Metropolis-Hastings sampling produces samples from distribution P by sampling from a proposal distribution Q
    /// and accepting/rejecting based on the density of P. Metropolis-Hastings sampling doesn't require that the
    /// proposal distribution Q is symmetric in comparison to <seealso cref="MetropolisSampler{T}"/>. It does need to
    /// be able to evaluate the proposal sampler's log density though. All densities are required to be in log space.
    ///
    /// The Metropolis-Hastings sampler is a stateful sampler. It keeps track of where it currently is in the domain
    /// of the distribution P.
    /// </summary>
    /// <typeparam name="T">The type of samples this sampler produces.</typeparam>
    public class MetropolisHastingsSampler<T> : McmcSampler<T>
    {
        /// <summary>
        /// Evaluates the log density function of the target distribution.
        /// </summary>
        private readonly DensityLn<T> _pdfLnP;

        /// <summary>
        /// Evaluates the log transition probability for the proposal distribution.
        /// </summary>
        private readonly TransitionKernelLn<T> _krnlQ;

        /// <summary>
        /// A function which samples from a proposal distribution.
        /// </summary>
        private readonly LocalProposalSampler<T> _proposal;

        /// <summary>
        /// The current location of the sampler.
        /// </summary>
        private T _current;

        /// <summary>
        /// The log density at the current location.
        /// </summary>
        private double _currentDensityLn;

        /// <summary>
        /// The number of burn iterations between two samples.
        /// </summary>
        private int _burnInterval;

        /// <summary>
        /// Constructs a new Metropolis-Hastings sampler using the default <see cref="System.Random"/> random number generator. This
        /// constructor will set the burn interval.
        /// </summary>
        /// <param name="x0">The initial sample.</param>
        /// <param name="pdfLnP">The log density of the distribution we want to sample from.</param>
        /// <param name="krnlQ">The log transition probability for the proposal distribution.</param>
        /// <param name="proposal">A method that samples from the proposal distribution.</param>
        /// <param name="burnInterval">The number of iterations in between returning samples.</param>
        /// <exception cref="ArgumentOutOfRangeException">When the number of burnInterval iteration is negative.</exception>
        public MetropolisHastingsSampler(T x0, DensityLn<T> pdfLnP, TransitionKernelLn<T> krnlQ, LocalProposalSampler<T> proposal, int burnInterval = 0)
        {
            _current = x0;
            _currentDensityLn = pdfLnP(x0);
            _pdfLnP = pdfLnP;
            _krnlQ = krnlQ;
            _proposal = proposal;
            BurnInterval = burnInterval;

            Burn(BurnInterval);
        }

        /// <summary>
        /// Gets or sets the number of iterations in between returning samples.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">When burn interval is negative.</exception>
        public int BurnInterval
        {
            get => _burnInterval;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException("Value must not be negative (zero is ok).");
                }
                _burnInterval = value;
            }
        }

        /// <summary>
        /// This method runs the sampler for a number of iterations without returning a sample
        /// </summary>
        private void Burn(int n)
        {
            for (int i = 0; i < n; i++)
            {
                // Get a sample from the proposal.
                T next = _proposal(_current);
                // Evaluate the density at the next sample.
                double p = _pdfLnP(next);
                // Evaluate the forward transition probability.
                double fwd = _krnlQ(next, _current);
                // Evaluate the backward transition probability
                double bwd = _krnlQ(_current, next);

                Samples++;

                double acc = Math.Min(0.0, p + bwd - _currentDensityLn - fwd);
                if (acc == 0.0)
                {
                    _current = next;
                    _currentDensityLn = p;
                    Accepts++;
                }
                else if (Bernoulli.Sample(RandomSource, Math.Exp(acc)) == 1)
                {
                    _current = next;
                    _currentDensityLn = p;
                    Accepts++;
                }
            }
        }

        /// <summary>
        /// Returns a sample from the distribution P.
        /// </summary>
        public override T Sample()
        {
            Burn(BurnInterval + 1);

            return _current;
        }
    }
}
