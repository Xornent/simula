using System;
using Simula.Maths.LinearAlgebra.Double;

namespace Simula.Maths.Differentiation
{
    /// <summary>
    /// Class to calculate finite difference coefficients using Taylor series expansion method.
    /// <remarks>
    /// <para>
    /// For n points, coefficients are calculated up to the maximum derivative order possible (n-1).
    /// The current function value position specifies the "center" for surrounding coefficients.
    /// Selecting the first, middle or last positions represent forward, backwards and central difference methods.
    /// </para>
    /// </remarks>
    /// </summary>
    public class FiniteDifferenceCoefficients
    {
        /// <summary>
        /// Number of points for finite difference coefficients. Changing this value recalculates the coefficients table.
        /// </summary>
        public int Points
        {
            get => _points;
            set
            {
                CalculateCoefficients(value);
                _points = value;
            }
        }

        private double[][,] _coefficients;
        private int _points;

        /// <summary>
        /// Initializes a new instance of the <see cref="FiniteDifferenceCoefficients"/> class.
        /// </summary>
        /// <param name="points">Number of finite difference coefficients.</param>
        public FiniteDifferenceCoefficients(int points)
        {
            Points = points;
            CalculateCoefficients(Points);
        }

        /// <summary>
        /// Gets the finite difference coefficients for a specified center and order.
        /// </summary>
        /// <param name="center">Current function position with respect to coefficients. Must be within point range.</param>
        /// <param name="order">Order of finite difference coefficients.</param>
        /// <returns>Vector of finite difference coefficients.</returns>
        public double[] GetCoefficients(int center, int order)
        {
            if (center >= _coefficients.Length)
                throw new ArgumentOutOfRangeException(nameof(center), "Center position must be within the point range.");
            if (order >= _coefficients.Length)
                throw new ArgumentOutOfRangeException(nameof(order), "Maximum difference order is points-1.");

            // Return proper row
            var columns = _coefficients[center].GetLength(1);
            var array = new double[columns];
            for (int i = 0; i < columns; ++i)
                array[i] = _coefficients[center][order, i];
            return array;
        }

        /// <summary>
        /// Gets the finite difference coefficients for all orders at a specified center.
        /// </summary>
        /// <param name="center">Current function position with respect to coefficients. Must be within point range.</param>
        /// <returns>Rectangular array of coefficients, with columns specifying order.</returns>
        public double[,] GetCoefficientsForAllOrders(int center)
        {
            if (center >= _coefficients.Length)
                throw new ArgumentOutOfRangeException(nameof(center), "Center position must be within the point range.");

            return _coefficients[center];
        }

        private void CalculateCoefficients(int points)
        {
            var c = new double[points][,];

            // For ever possible center given the number of points, compute ever possible coefficient for all possible orders.
            for (int center = 0; center < points; center++)
            {
                // Deltas matrix for center located at 'center'.
                var A = new DenseMatrix(points);
                var l = points - center - 1;
                for (int row = points - 1; row >= 0; row--)
                {
                    A[row, 0] = 1.0;
                    for (int col = 1; col < points; col++)
                    {
                        A[row, col] = A[row, col - 1] * l / col;
                    }
                    l -= 1;
                }

                c[center] = A.Inverse().ToArray();

                // "Polish" results by rounding.
                var fac = SpecialFunctions.Factorial(points);
                for (int j = 0; j < points; j++)
                {
                    for (int k = 0; k < points; k++)
                    {
                        c[center][j, k] = (Math.Round(c[center][j, k] * fac, MidpointRounding.AwayFromZero)) / fac;
                    }
                }
            }

            _coefficients = c;
        }
    }
}
