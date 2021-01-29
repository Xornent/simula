using System.Collections.Generic;
using System.Runtime.Serialization;

#if !NETSTANDARD1_3
using System;
using System.Runtime;
#endif

namespace Simula.Maths.Random
{
    /// <summary>
    /// Multiplicative congruential generator using a modulus of 2^31-1 and a multiplier of 1132489760.
    /// </summary>
    [Serializable]
    [DataContract(Namespace = "urn:Simula.Maths/Random")]
    public class Mcg31m1 : RandomSource
    {
        const ulong Modulus = 2147483647;
        const ulong Multiplier = 1132489760;
        const double Reciprocal = 1.0/Modulus;

        [DataMember(Order = 1)]
        ulong _xn;

        /// <summary>
        /// Initializes a new instance of the <see cref="Mcg31m1"/> class using
        /// a seed based on time and unique GUIDs.
        /// </summary>
        public Mcg31m1() : this(RandomSeed.Robust())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Mcg31m1"/> class using
        /// a seed based on time and unique GUIDs.
        /// </summary>
        /// <param name="threadSafe">if set to <c>true</c> , the class is thread safe.</param>
        public Mcg31m1(bool threadSafe) : this(RandomSeed.Robust(), threadSafe)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Mcg31m1"/> class.
        /// </summary>
        /// <param name="seed">The seed value.</param>
        /// <remarks>If the seed value is zero, it is set to one. Uses the
        /// value of <see cref="Control.ThreadSafeRandomNumberGenerators"/> to
        /// set whether the instance is thread safe.</remarks>
        public Mcg31m1(int seed)
        {
            if (seed == 0)
            {
                seed = 1;
            }

            _xn = (uint)seed%Modulus;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Mcg31m1"/> class.
        /// </summary>
        /// <param name="seed">The seed value.</param>
        /// <param name="threadSafe">if set to <c>true</c>, the class is thread safe.</param>
        public Mcg31m1(int seed, bool threadSafe) : base(threadSafe)
        {
            if (seed == 0)
            {
                seed = 1;
            }

            _xn = (uint)seed%Modulus;
        }

        /// <summary>
        /// Returns a random double-precision floating point number greater than or equal to 0.0, and less than 1.0.
        /// </summary>
        protected sealed override double DoSample()
        {
            double ret = _xn*Reciprocal;
            _xn = (_xn*Multiplier)%Modulus;
            return ret;
        }

        /// <summary>
        /// Fills an array with random numbers greater than or equal to 0.0 and less than 1.0.
        /// </summary>
        /// <remarks>Supports being called in parallel from multiple threads.</remarks>
        public static void Doubles(double[] values, int seed)
        {
            if (seed == 0)
            {
                seed = 1;
            }

            ulong xn = (uint)seed%Modulus;

            for (int i = 0; i < values.Length; i++)
            {
                values[i] = xn*Reciprocal;
                xn = (xn*Multiplier)%Modulus;
            }
        }

        /// <summary>
        /// Returns an array of random numbers greater than or equal to 0.0 and less than 1.0.
        /// </summary>
        /// <remarks>Supports being called in parallel from multiple threads.</remarks>
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static double[] Doubles(int length, int seed)
        {
            var data = new double[length];
            Doubles(data, seed);
            return data;
        }

        /// <summary>
        /// Returns an infinite sequence of random numbers greater than or equal to 0.0 and less than 1.0.
        /// </summary>
        /// <remarks>Supports being called in parallel from multiple threads, but the result must be enumerated from a single thread each.</remarks>
        public static IEnumerable<double> DoubleSequence(int seed)
        {
            if (seed == 0)
            {
                seed = 1;
            }

            ulong xn = (uint)seed%Modulus;

            while (true)
            {
                yield return xn*Reciprocal;
                xn = (xn*Multiplier)%Modulus;
            }
        }
    }
}
