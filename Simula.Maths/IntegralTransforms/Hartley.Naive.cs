using System;
using Simula.Maths.Threading;

namespace Simula.Maths.IntegralTransforms
{
    /// <summary>
    /// Fast (FHT) Implementation of the Discrete Hartley Transform (DHT).
    /// </summary>
    public static partial class Hartley
    {
        /// <summary>
        /// Naive generic DHT, useful e.g. to verify faster algorithms.
        /// </summary>
        /// <param name="samples">Time-space sample vector.</param>
        /// <returns>Corresponding frequency-space vector.</returns>
        internal static double[] Naive(double[] samples)
        {
            var w0 = Constants.Pi2/samples.Length;
            var spectrum = new double[samples.Length];

            CommonParallel.For(0, samples.Length, (u, v) =>
                {
                    for (int i = u; i < v; i++)
                    {
                        var wk = w0*i;
                        var sum = 0.0;
                        for (var n = 0; n < samples.Length; n++)
                        {
                            var w = n*wk;
                            sum += samples[n]*Constants.Sqrt2*Math.Cos(w - Constants.PiOver4);
                        }

                        spectrum[i] = sum;
                    }
                });

            return spectrum;
        }
    }
}
