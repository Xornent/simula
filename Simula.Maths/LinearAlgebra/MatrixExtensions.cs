namespace Simula.Maths.LinearAlgebra
{
    using Complex64 = System.Numerics.Complex;

    public static class MatrixExtensions
    {
        /// <summary>
        /// Converts a matrix to single precision.
        /// </summary>
        public static Matrix<float> ToSingle(this Matrix<double> matrix)
        {
            return matrix.Map(x => (float)x, Zeros.AllowSkip);
        }

        /// <summary>
        /// Converts a matrix to double precision.
        /// </summary>
        public static Matrix<double> ToDouble(this Matrix<float> matrix)
        {
            return matrix.Map(x => (double)x, Zeros.AllowSkip);
        }

        /// <summary>
        /// Converts a matrix to single precision complex numbers.
        /// </summary>
        public static Matrix<Maths.Complex32> ToComplex32(this Matrix<Complex64> matrix)
        {
            return matrix.Map(x => new Maths.Complex32((float)x.Real, (float)x.Imaginary), Zeros.AllowSkip);
        }

        /// <summary>
        /// Converts a matrix to double precision complex numbers.
        /// </summary>
        public static Matrix<Complex64> ToComplex(this Matrix<Maths.Complex32> matrix)
        {
            return matrix.Map(x => new Complex64(x.Real, x.Imaginary), Zeros.AllowSkip);
        }

        /// <summary>
        /// Gets a single precision complex matrix with the real parts from the given matrix.
        /// </summary>
        public static Matrix<Maths.Complex32> ToComplex32(this Matrix<float> matrix)
        {
            return matrix.Map(x => new Maths.Complex32(x, 0f), Zeros.AllowSkip);
        }

        /// <summary>
        /// Gets a double precision complex matrix with the real parts from the given matrix.
        /// </summary>
        public static Matrix<Complex64> ToComplex(this Matrix<double> matrix)
        {
            return matrix.Map(x => new Complex64(x, 0d), Zeros.AllowSkip);
        }

        /// <summary>
        /// Gets a real matrix representing the real parts of a complex matrix.
        /// </summary>
        public static Matrix<double> Real(this Matrix<Complex64> matrix)
        {
            return matrix.Map(x => x.Real, Zeros.AllowSkip);
        }

        /// <summary>
        /// Gets a real matrix representing the real parts of a complex matrix.
        /// </summary>
        public static Matrix<float> Real(this Matrix<Maths.Complex32> matrix)
        {
            return matrix.Map(x => x.Real, Zeros.AllowSkip);
        }

        /// <summary>
        /// Gets a real matrix representing the imaginary parts of a complex matrix.
        /// </summary>
        public static Matrix<double> Imaginary(this Matrix<Complex64> matrix)
        {
            return matrix.Map(x => x.Imaginary, Zeros.AllowSkip);
        }

        /// <summary>
        /// Gets a real matrix representing the imaginary parts of a complex matrix.
        /// </summary>
        public static Matrix<float> Imaginary(this Matrix<Maths.Complex32> matrix)
        {
            return matrix.Map(x => x.Imaginary, Zeros.AllowSkip);
        }
    }
}
