using Complex = System.Numerics.Complex;

namespace Simula.Maths.Statistics
{
    public static partial class ArrayStatistics
    {
        /// <summary>
        /// Returns the smallest absolute value from the unsorted data array.
        /// Returns NaN if data is empty or any entry is NaN.
        /// </summary>
        /// <param name="data">Sample array, no sorting is assumed.</param>
        public static Complex MinimumMagnitudePhase(Complex[] data)
        {
            if (data.Length == 0)
            {
                return new Complex(double.NaN, double.NaN);
            }

            double minMagnitude = double.PositiveInfinity;
            Complex min = new Complex(double.PositiveInfinity, double.PositiveInfinity);
            for (int i = 0; i < data.Length; i++)
            {
                double magnitude = data[i].Magnitude;
                if (double.IsNaN(magnitude))
                {
                    return new Complex(double.NaN, double.NaN);
                }
                if (magnitude < minMagnitude || magnitude == minMagnitude && data[i].Phase < min.Phase)
                {
                    minMagnitude = magnitude;
                    min = data[i];
                }
            }

            return min;
        }

        /// <summary>
        /// Returns the smallest absolute value from the unsorted data array.
        /// Returns NaN if data is empty or any entry is NaN.
        /// </summary>
        /// <param name="data">Sample array, no sorting is assumed.</param>
        public static Complex32 MinimumMagnitudePhase(Complex32[] data)
        {
            if (data.Length == 0)
            {
                return new Complex32(float.NaN, float.NaN);
            }

            float minMagnitude = float.PositiveInfinity;
            Complex32 min = new Complex32(float.PositiveInfinity, float.PositiveInfinity);
            for (int i = 0; i < data.Length; i++)
            {
                float magnitude = data[i].Magnitude;
                if (float.IsNaN(magnitude))
                {
                    return new Complex32(float.NaN, float.NaN);
                }
                if (magnitude < minMagnitude || magnitude == minMagnitude && data[i].Phase < min.Phase)
                {
                    minMagnitude = magnitude;
                    min = data[i];
                }
            }

            return min;
        }

        /// <summary>
        /// Returns the largest absolute value from the unsorted data array.
        /// Returns NaN if data is empty or any entry is NaN.
        /// </summary>
        /// <param name="data">Sample array, no sorting is assumed.</param>
        public static Complex MaximumMagnitudePhase(Complex[] data)
        {
            if (data.Length == 0)
            {
                return new Complex(double.NaN, double.NaN);
            }

            double maxMagnitude = 0.0d;
            Complex max = Complex.Zero;
            for (int i = 0; i < data.Length; i++)
            {
                double magnitude = data[i].Magnitude;
                if (double.IsNaN(magnitude))
                {
                    return new Complex(double.NaN, double.NaN);
                }
                if (magnitude > maxMagnitude || magnitude == maxMagnitude && data[i].Phase > max.Phase)
                {
                    maxMagnitude = magnitude;
                    max = data[i];
                }
            }

            return max;
        }

        /// <summary>
        /// Returns the largest absolute value from the unsorted data array.
        /// Returns NaN if data is empty or any entry is NaN.
        /// </summary>
        /// <param name="data">Sample array, no sorting is assumed.</param>
        public static Complex32 MaximumMagnitudePhase(Complex32[] data)
        {
            if (data.Length == 0)
            {
                return new Complex32(float.NaN, float.NaN);
            }

            float maxMagnitude = 0.0f;
            Complex32 max = Complex32.Zero;
            for (int i = 0; i < data.Length; i++)
            {
                float magnitude = data[i].Magnitude;
                if (float.IsNaN(magnitude))
                {
                    return new Complex32(float.NaN, float.NaN);
                }
                if (magnitude > maxMagnitude || magnitude == maxMagnitude && data[i].Phase > max.Phase)
                {
                    maxMagnitude = magnitude;
                    max = data[i];
                }
            }

            return max;
        }
    }
}
