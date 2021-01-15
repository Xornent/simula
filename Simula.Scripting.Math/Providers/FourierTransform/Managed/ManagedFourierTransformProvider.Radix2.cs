using System;
using System.Runtime.CompilerServices;
using Simula.Maths.Threading;
using Complex = System.Numerics.Complex;

namespace Simula.Maths.Providers.FourierTransform.Managed
{
    internal partial class ManagedFourierTransformProvider
    {
        /// <summary>
        /// Radix-2 Reorder Helper Method
        /// </summary>
        /// <typeparam name="T">Sample type</typeparam>
        /// <param name="samples">Sample vector</param>
        private static void Radix2Reorder<T>(T[] samples)
        {
            var j = 0;
            for (var i = 0; i < samples.Length - 1; i++)
            {
                if (i < j)
                {
                    var temp = samples[i];
                    samples[i] = samples[j];
                    samples[j] = temp;
                }

                var m = samples.Length;

                do
                {
                    m >>= 1;
                    j ^= m;
                }
                while ((j & m) == 0);
            }
        }

        /// <summary>
        /// Radix-2 Step Helper Method
        /// </summary>
        /// <param name="samples">Sample vector.</param>
        /// <param name="exponentSign">Fourier series exponent sign.</param>
        /// <param name="levelSize">Level Group Size.</param>
        /// <param name="k">Index inside of the level.</param>
#if !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static void Radix2Step(Complex32[] samples, int exponentSign, int levelSize, int k)
        {
            // Twiddle Factor
            var exponent = (exponentSign * k) * Constants.Pi / levelSize;
            var w = new Complex32((float)Math.Cos(exponent), (float)Math.Sin(exponent));

            var step = levelSize << 1;
            for (var i = k; i < samples.Length; i += step)
            {
                var ai = samples[i];
                var t = w * samples[i + levelSize];
                samples[i] = ai + t;
                samples[i + levelSize] = ai - t;
            }
        }

        /// <summary>
        /// Radix-2 Step Helper Method
        /// </summary>
        /// <param name="samples">Sample vector.</param>
        /// <param name="exponentSign">Fourier series exponent sign.</param>
        /// <param name="levelSize">Level Group Size.</param>
        /// <param name="k">Index inside of the level.</param>
#if !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static void Radix2Step(Complex[] samples, int exponentSign, int levelSize, int k)
        {
            // Twiddle Factor
            var exponent = (exponentSign * k) * Constants.Pi / levelSize;
            var w = new Complex(Math.Cos(exponent), Math.Sin(exponent));

            var step = levelSize << 1;
            for (var i = k; i < samples.Length; i += step)
            {
                var ai = samples[i];
                var t = w * samples[i + levelSize];
                samples[i] = ai + t;
                samples[i + levelSize] = ai - t;
            }
        }

        /// <summary>
        /// Radix-2 generic FFT for power-of-two sized sample vectors.
        /// </summary>
        private static void Radix2Forward(Complex32[] data)
        {
            Radix2Reorder(data);
            for (var levelSize = 1; levelSize < data.Length; levelSize *= 2)
            {
                for (var k = 0; k < levelSize; k++)
                {
                    Radix2Step(data, -1, levelSize, k);
                }
            }
        }

        /// <summary>
        /// Radix-2 generic FFT for power-of-two sized sample vectors.
        /// </summary>
        private static void Radix2Forward(Complex[] data)
        {
            Radix2Reorder(data);
            for (var levelSize = 1; levelSize < data.Length; levelSize *= 2)
            {
                for (var k = 0; k < levelSize; k++)
                {
                    Radix2Step(data, -1, levelSize, k);
                }
            }
        }

        /// <summary>
        /// Radix-2 generic FFT for power-of-two sized sample vectors.
        /// </summary>
        private static void Radix2Inverse(Complex32[] data)
        {
            Radix2Reorder(data);
            for (var levelSize = 1; levelSize < data.Length; levelSize *= 2)
            {
                for (var k = 0; k < levelSize; k++)
                {
                    Radix2Step(data, 1, levelSize, k);
                }
            }
        }

        /// <summary>
        /// Radix-2 generic FFT for power-of-two sized sample vectors.
        /// </summary>
        private static void Radix2Inverse(Complex[] data)
        {
            Radix2Reorder(data);
            for (var levelSize = 1; levelSize < data.Length; levelSize *= 2)
            {
                for (var k = 0; k < levelSize; k++)
                {
                    Radix2Step(data, 1, levelSize, k);
                }
            }
        }

        /// <summary>
        /// Radix-2 generic FFT for power-of-two sample vectors (Parallel Version).
        /// </summary>
        private static void Radix2ForwardParallel(Complex32[] data)
        {
            Radix2Reorder(data);
            for (var levelSize = 1; levelSize < data.Length; levelSize *= 2)
            {
                var size = levelSize;

                CommonParallel.For(0, size, 64, (u, v) =>
                {
                    for (int i = u; i < v; i++)
                    {
                        Radix2Step(data, -1, size, i);
                    }
                });
            }
        }

        /// <summary>
        /// Radix-2 generic FFT for power-of-two sample vectors (Parallel Version).
        /// </summary>
        private static void Radix2ForwardParallel(Complex[] data)
        {
            Radix2Reorder(data);
            for (var levelSize = 1; levelSize < data.Length; levelSize *= 2)
            {
                var size = levelSize;

                CommonParallel.For(0, size, 64, (u, v) =>
                {
                    for (int i = u; i < v; i++)
                    {
                        Radix2Step(data, -1, size, i);
                    }
                });
            }
        }

        /// <summary>
        /// Radix-2 generic FFT for power-of-two sample vectors (Parallel Version).
        /// </summary>
        private static void Radix2InverseParallel(Complex32[] data)
        {
            Radix2Reorder(data);
            for (var levelSize = 1; levelSize < data.Length; levelSize *= 2)
            {
                var size = levelSize;

                CommonParallel.For(0, size, 64, (u, v) =>
                {
                    for (int i = u; i < v; i++)
                    {
                        Radix2Step(data, 1, size, i);
                    }
                });
            }
        }

        /// <summary>
        /// Radix-2 generic FFT for power-of-two sample vectors (Parallel Version).
        /// </summary>
        private static void Radix2InverseParallel(Complex[] data)
        {
            Radix2Reorder(data);
            for (var levelSize = 1; levelSize < data.Length; levelSize *= 2)
            {
                var size = levelSize;

                CommonParallel.For(0, size, 64, (u, v) =>
                {
                    for (int i = u; i < v; i++)
                    {
                        Radix2Step(data, 1, size, i);
                    }
                });
            }
        }
    }
}
