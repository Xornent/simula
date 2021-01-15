using System;
using System.Collections.Generic;
using System.Linq;
using Simula.Maths.Threading;

namespace Simula.Maths.Interpolation
{
    /// <summary>
    /// Piece-wise Log-Linear Interpolation
    /// </summary>
    /// <remarks>This algorithm supports differentiation, not integration.</remarks>
    public class LogLinear : IInterpolation
    {
        /// <summary>
        /// Internal Spline Interpolation
        /// </summary>
        readonly LinearSpline _spline;

        /// <param name="x">Sample points (N), sorted ascending</param>
        /// <param name="logy">Natural logarithm of the sample values (N) at the corresponding points</param>
        public LogLinear(double[] x, double[] logy)
        {
            _spline = LinearSpline.InterpolateSorted(x, logy);
        }

        /// <summary>
        /// Create a piecewise log-linear interpolation from a set of (x,y) value pairs, sorted ascendingly by x.
        /// </summary>
        public static LogLinear InterpolateSorted(double[] x, double[] y)
        {
            if (x.Length != y.Length)
            {
                throw new ArgumentException("All vectors must have the same dimensionality.");
            }

            var logy = new double[y.Length];
            CommonParallel.For(0, y.Length, 4096, (a, b) =>
            {
                for (int i = a; i < b; i++)
                {
                    logy[i] = Math.Log(y[i]);
                }
            });

            return new LogLinear(x, logy);
        }

        /// <summary>
        /// Create a piecewise log-linear interpolation from an unsorted set of (x,y) value pairs.
        /// WARNING: Works in-place and can thus causes the data array to be reordered and modified.
        /// </summary>
        public static LogLinear InterpolateInplace(double[] x, double[] y)
        {
            if (x.Length != y.Length)
            {
                throw new ArgumentException("All vectors must have the same dimensionality.");
            }

            Sorting.Sort(x, y);
            CommonParallel.For(0, y.Length, 4096, (a, b) =>
            {
                for (int i = a; i < b; i++)
                {
                    y[i] = Math.Log(y[i]);
                }
            });

            return new LogLinear(x, y);
        }

        /// <summary>
        /// Create a piecewise log-linear interpolation from an unsorted set of (x,y) value pairs.
        /// </summary>
        public static LogLinear Interpolate(IEnumerable<double> x, IEnumerable<double> y)
        {
            // note: we must make a copy, even if the input was arrays already
            return InterpolateInplace(x.ToArray(), y.ToArray());
        }

        /// <summary>
        /// Gets a value indicating whether the algorithm supports differentiation (interpolated derivative).
        /// </summary>
        bool IInterpolation.SupportsDifferentiation => true;

        /// <summary>
        /// Gets a value indicating whether the algorithm supports integration (interpolated quadrature).
        /// </summary>
        bool IInterpolation.SupportsIntegration => false;

        /// <summary>
        /// Interpolate at point t.
        /// </summary>
        /// <param name="t">Point t to interpolate at.</param>
        /// <returns>Interpolated value x(t).</returns>
        public double Interpolate(double t)
        {
            return Math.Exp(_spline.Interpolate(t));
        }

        /// <summary>
        /// Differentiate at point t.
        /// </summary>
        /// <param name="t">Point t to interpolate at.</param>
        /// <returns>Interpolated first derivative at point t.</returns>
        public double Differentiate(double t)
        {
            return Interpolate(t)*_spline.Differentiate(t);
        }

        /// <summary>
        /// Differentiate twice at point t.
        /// </summary>
        /// <param name="t">Point t to interpolate at.</param>
        /// <returns>Interpolated second derivative at point t.</returns>
        public double Differentiate2(double t)
        {
            var linearFirstDerivative = _spline.Differentiate(t);
            var linearSecondDerivative = _spline.Differentiate2(t);

            var secondDerivative = Differentiate(t)*linearFirstDerivative +
                                   Interpolate(t)*linearSecondDerivative;

            return secondDerivative;
        }

        /// <summary>
        /// Indefinite integral at point t.
        /// </summary>
        /// <param name="t">Point t to integrate at.</param>
        double IInterpolation.Integrate(double t)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Definite integral between points a and b.
        /// </summary>
        /// <param name="a">Left bound of the integration interval [a,b].</param>
        /// <param name="b">Right bound of the integration interval [a,b].</param>
        double IInterpolation.Integrate(double a, double b)
        {
            throw new NotSupportedException();
        }
    }
}
