﻿using System;
using Simula.Maths.Random;

namespace Simula.Maths.Statistics.Mcmc
{
    /// <summary>
    /// A method which samples datapoints from a proposal distribution. The implementation of this sampler
    /// is stateless: no variables are saved between two calls to Sample. This proposal is different from
    /// <seealso cref="LocalProposalSampler{T}"/> in that it doesn't take any parameters; it samples random
    /// variables from the whole domain.
    /// </summary>
    /// <typeparam name="T">The type of the datapoints.</typeparam>
    /// <returns>A sample from the proposal distribution.</returns>
    public delegate T GlobalProposalSampler<out T>();

    /// <summary>
    /// A method which samples datapoints from a proposal distribution given an initial sample. The implementation
    /// of this sampler is stateless: no variables are saved between two calls to Sample. This proposal is different from
    /// <seealso cref="GlobalProposalSampler{T}"/> in that it samples locally around an initial point. In other words, it
    /// makes a small local move rather than producing a global sample from the proposal.
    /// </summary>
    /// <typeparam name="T">The type of the datapoints.</typeparam>
    /// <param name="init">The initial sample.</param>
    /// <returns>A sample from the proposal distribution.</returns>
    public delegate T LocalProposalSampler<T>(T init);

    /// <summary>
    /// A function which evaluates a density.
    /// </summary>
    /// <typeparam name="T">The type of data the distribution is over.</typeparam>
    /// <param name="sample">The sample we want to evaluate the density for.</param>
    public delegate double Density<in T>(T sample);

    /// <summary>
    /// A function which evaluates a log density.
    /// </summary>
    /// <typeparam name="T">The type of data the distribution is over.</typeparam>
    /// <param name="sample">The sample we want to evaluate the log density for.</param>
    public delegate double DensityLn<in T>(T sample);

    /// <summary>
    /// A function which evaluates the log of a transition kernel probability.
    /// </summary>
    /// <typeparam name="T">The type for the space over which this transition kernel is defined.</typeparam>
    /// <param name="to">The new state in the transition.</param>
    /// <param name="from">The previous state in the transition.</param>
    /// <returns>The log probability of the transition.</returns>
    public delegate double TransitionKernelLn<in T>(T to, T from);

    /// <summary>
    /// The interface which every sampler must implement.
    /// </summary>
    /// <typeparam name="T">The type of samples this sampler produces.</typeparam>
    public abstract class McmcSampler<T>
    {
        /// <summary>
        /// The random number generator for this class.
        /// </summary>
        private System.Random _randomNumberGenerator;

        /// <summary>
        /// Keeps track of the number of accepted samples.
        /// </summary>
        protected int Accepts;

        /// <summary>
        /// Keeps track of the number of calls to the proposal sampler.
        /// </summary>
        protected int Samples;

        /// <summary>
        /// Initializes a new instance of the <see cref="McmcSampler{T}"/> class.
        /// </summary>
        /// <remarks>Thread safe instances are two and half times slower than non-thread
        /// safe classes.</remarks>
        protected McmcSampler()
        {
            Accepts = 0;
            Samples = 0;
            RandomSource = SystemRandomSource.Default;
        }

        /// <summary>
        /// Gets or sets the random number generator.
        /// </summary>
        /// <exception cref="ArgumentNullException">When the random number generator is null.</exception>
        public System.Random RandomSource
        {
            get => _randomNumberGenerator;
            set => _randomNumberGenerator = value ?? SystemRandomSource.Default;
        }

        /// <summary>
        /// Returns one sample.
        /// </summary>
        public abstract T Sample();

        /// <summary>
        /// Returns a number of samples.
        /// </summary>
        /// <param name="n">The number of samples we want.</param>
        /// <returns>An array of samples.</returns>
        public virtual T[] Sample(int n)
        {
            var ret = new T[n];
            for (int i = 0; i < n; i++)
            {
                ret[i] = Sample();
            }
            return ret;
        }

        /// <summary>
        /// Gets the acceptance rate of the sampler.
        /// </summary>
        public double AcceptanceRate => Accepts / (double)Samples;
    }
}