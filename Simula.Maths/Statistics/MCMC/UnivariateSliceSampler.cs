using System;

namespace Simula.Maths.Statistics.Mcmc
{
    /// <summary>
    /// Slice sampling produces samples from distribution P by uniformly sampling from under the pdf of P using
    /// a technique described in "Slice Sampling", R. Neal, 2003. All densities are required to be in log space.
    ///
    /// The slice sampler is a stateful sampler. It keeps track of where it currently is in the domain
    /// of the distribution P.
    /// </summary>
    public class UnivariateSliceSampler : McmcSampler<double>
    {
        /// <summary>
        /// Evaluates the log density function of the target distribution.
        /// </summary>
        private readonly DensityLn<double> _pdfLnP;

        /// <summary>
        /// The current location of the sampler.
        /// </summary>
        private double _current;

        /// <summary>
        /// The log density at the current location.
        /// </summary>
        private double _currentDensityLn;

        /// <summary>
        /// The number of burn iterations between two samples.
        /// </summary>
        private int _burnInterval;

        /// <summary>
        /// The scale of the slice sampler.
        /// </summary>
        private double _scale;

        /// <summary>
        /// Constructs a new Slice sampler using the default <see cref="System.Random"/> random
        /// number generator. The burn interval will be set to 0.
        /// </summary>
        /// <param name="x0">The initial sample.</param>
        /// <param name="pdfLnP">The density of the distribution we want to sample from.</param>
        /// <param name="scale">The scale factor of the slice sampler.</param>
        /// <exception cref="ArgumentOutOfRangeException">When the scale of the slice sampler is not positive.</exception>
        public UnivariateSliceSampler(double x0, DensityLn<double> pdfLnP, double scale)
            : this(x0, pdfLnP, 0, scale)
        {
        }

        /// <summary>
        /// Constructs a new slice sampler using the default <see cref="System.Random"/> random number generator. It
        /// will set the number of burnInterval iterations and run a burnInterval phase.
        /// </summary>
        /// <param name="x0">The initial sample.</param>
        /// <param name="pdfLnP">The density of the distribution we want to sample from.</param>
        /// <param name="burnInterval">The number of iterations in between returning samples.</param>
        /// <param name="scale">The scale factor of the slice sampler.</param>
        /// <exception cref="ArgumentOutOfRangeException">When the number of burnInterval iteration is negative.</exception>
        /// <exception cref="ArgumentOutOfRangeException">When the scale of the slice sampler is not positive.</exception>
        public UnivariateSliceSampler(double x0, DensityLn<double> pdfLnP, int burnInterval, double scale)
        {
            _current = x0;
            _currentDensityLn = pdfLnP(x0);
            _pdfLnP = pdfLnP;
            Scale = scale;
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
        /// Gets or sets the scale of the slice sampler.
        /// </summary>
        public double Scale
        {
            get => _scale;
            set
            {
                if (value <= 0.0)
                {
                    throw new ArgumentException("Value must be positive (and not zero).");
                }
                _scale = value;
            }
        }

        /// <summary>
        /// This method runs the sampler for a number of iterations without returning a sample
        /// </summary>
        private void Burn(int n)
        {
            for (int i = 0; i < n; i++)
            {
                // The logarithm of the slice height.
                double lu = Math.Log(RandomSource.NextDouble()) + _currentDensityLn;

                // Create a horizontal interval (x_l, x_r) enclosing x.
                double r = RandomSource.NextDouble();
                double xL = _current - r * Scale;
                double xR = _current + (1.0 - r) * Scale;

                // Stepping out procedure.
                while (_pdfLnP(xL) > lu) { xL -= Scale; }
                while (_pdfLnP(xR) > lu) { xR += Scale; }

                // Shrinking: propose new x and shrink interval until good one found.
                while (true)
                {
                    double xnew = RandomSource.NextDouble() * (xR - xL) + xL;
                    _currentDensityLn = _pdfLnP(xnew);
                    if (_currentDensityLn > lu)
                    {
                        _current = xnew;
                        Accepts++;
                        Samples++;
                        break;
                    }
                    if (xnew > _current)
                    {
                        xR = xnew;
                    }
                    else
                    {
                        xL = xnew;
                    }
                }
            }
        }

        /// <summary>
        /// Returns a sample from the distribution P.
        /// </summary>
        public override double Sample()
        {
            Burn(BurnInterval + 1);

            return _current;
        }
    }
}
