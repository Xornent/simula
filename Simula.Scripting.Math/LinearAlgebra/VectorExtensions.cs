namespace Simula.Maths.LinearAlgebra
{
    using Complex64 = System.Numerics.Complex;

    public static class VectorExtensions
    {
        /// <summary>
        /// Converts a vector to single precision.
        /// </summary>
        public static Vector<float> ToSingle(this Vector<double> vector)
        {
            return vector.Map(x => (float)x, Zeros.AllowSkip);
        }

        /// <summary>
        /// Converts a vector to double precision.
        /// </summary>
        public static Vector<double> ToDouble(this Vector<float> vector)
        {
            return vector.Map(x => (double)x, Zeros.AllowSkip);
        }

        /// <summary>
        /// Converts a vector to single precision complex numbers.
        /// </summary>
        public static Vector<Maths.Complex32> ToComplex32(this Vector<Complex64> vector)
        {
            return vector.Map(x => new Maths.Complex32((float)x.Real, (float)x.Imaginary), Zeros.AllowSkip);
        }

        /// <summary>
        /// Converts a vector to double precision complex numbers.
        /// </summary>
        public static Vector<Complex64> ToComplex(this Vector<Maths.Complex32> vector)
        {
            return vector.Map(x => new Complex64(x.Real, x.Imaginary), Zeros.AllowSkip);
        }

        /// <summary>
        /// Gets a single precision complex vector with the real parts from the given vector.
        /// </summary>
        public static Vector<Maths.Complex32> ToComplex32(this Vector<float> vector)
        {
            return vector.Map(x => new Maths.Complex32(x, 0f), Zeros.AllowSkip);
        }

        /// <summary>
        /// Gets a double precision complex vector with the real parts from the given vector.
        /// </summary>
        public static Vector<Complex64> ToComplex(this Vector<double> vector)
        {
            return vector.Map(x => new Complex64(x, 0d), Zeros.AllowSkip);
        }

        /// <summary>
        /// Gets a real vector representing the real parts of a complex vector.
        /// </summary>
        public static Vector<double> Real(this Vector<Complex64> vector)
        {
            return vector.Map(x => x.Real, Zeros.AllowSkip);
        }

        /// <summary>
        /// Gets a real vector representing the real parts of a complex vector.
        /// </summary>
        public static Vector<float> Real(this Vector<Maths.Complex32> vector)
        {
            return vector.Map(x => x.Real, Zeros.AllowSkip);
        }

        /// <summary>
        /// Gets a real vector representing the imaginary parts of a complex vector.
        /// </summary>
        public static Vector<double> Imaginary(this Vector<Complex64> vector)
        {
            return vector.Map(x => x.Imaginary, Zeros.AllowSkip);
        }

        /// <summary>
        /// Gets a real vector representing the imaginary parts of a complex vector.
        /// </summary>
        public static Vector<float> Imaginary(this Vector<Maths.Complex32> vector)
        {
            return vector.Map(x => x.Imaginary, Zeros.AllowSkip);
        }
    }
}
