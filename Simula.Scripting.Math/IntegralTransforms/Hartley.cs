using System;

namespace Simula.Maths.IntegralTransforms
{
    /// <summary>
    /// Fast (FHT) Implementation of the Discrete Hartley Transform (DHT).
    /// </summary>
    public static partial class Hartley
    {
        /// <summary>
        /// Naive forward DHT, useful e.g. to verify faster algorithms.
        /// </summary>
        /// <param name="timeSpace">Time-space sample vector.</param>
        /// <param name="options">Hartley Transform Convention Options.</param>
        /// <returns>Corresponding frequency-space vector.</returns>
        public static double[] NaiveForward(double[] timeSpace, HartleyOptions options)
        {
            var frequencySpace = Naive(timeSpace);
            ForwardScaleByOptions(options, frequencySpace);
            return frequencySpace;
        }

        /// <summary>
        /// Naive inverse DHT, useful e.g. to verify faster algorithms.
        /// </summary>
        /// <param name="frequencySpace">Frequency-space sample vector.</param>
        /// <param name="options">Hartley Transform Convention Options.</param>
        /// <returns>Corresponding time-space vector.</returns>
        public static double[] NaiveInverse(double[] frequencySpace, HartleyOptions options)
        {
            var timeSpace = Naive(frequencySpace);
            InverseScaleByOptions(options, timeSpace);
            return timeSpace;
        }

        /// <summary>
        /// Rescale FFT-the resulting vector according to the provided convention options.
        /// </summary>
        /// <param name="options">Fourier Transform Convention Options.</param>
        /// <param name="samples">Sample Vector.</param>
        static void ForwardScaleByOptions(HartleyOptions options, double[] samples)
        {
            if ((options & HartleyOptions.NoScaling) == HartleyOptions.NoScaling ||
                (options & HartleyOptions.AsymmetricScaling) == HartleyOptions.AsymmetricScaling)
            {
                return;
            }

            var scalingFactor = Math.Sqrt(1.0/samples.Length);
            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] *= scalingFactor;
            }
        }

        /// <summary>
        /// Rescale the iFFT-resulting vector according to the provided convention options.
        /// </summary>
        /// <param name="options">Fourier Transform Convention Options.</param>
        /// <param name="samples">Sample Vector.</param>
        static void InverseScaleByOptions(HartleyOptions options, double[] samples)
        {
            if ((options & HartleyOptions.NoScaling) == HartleyOptions.NoScaling)
            {
                return;
            }

            var scalingFactor = 1.0/samples.Length;
            if ((options & HartleyOptions.AsymmetricScaling) != HartleyOptions.AsymmetricScaling)
            {
                scalingFactor = Math.Sqrt(scalingFactor);
            }

            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] *= scalingFactor;
            }
        }
    }
}
