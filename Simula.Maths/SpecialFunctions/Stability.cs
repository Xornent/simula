using System;
using Complex = System.Numerics.Complex;

// ReSharper disable CheckNamespace
namespace Simula.Maths
// ReSharper restore CheckNamespace
{
    public partial class SpecialFunctions
    {
        /// <summary>
        /// Numerically stable exponential minus one, i.e. <code>x -> exp(x)-1</code>
        /// </summary>
        /// <param name="power">A number specifying a power.</param>
        /// <returns>Returns <code>exp(power)-1</code>.</returns>
        public static double ExponentialMinusOne(double power)
        {
            double x = Math.Abs(power);
            if (x > 0.1)
            {
                return Math.Exp(power) - 1.0;
            }

            if (x < x.PositiveEpsilonOf())
            {
                return x;
            }

            // Series Expansion to x^k / k!
            int k = 0;
            double term = 1.0;
            return Evaluate.Series(
                () =>
                {
                    k++;
                    term *= power;
                    term /= k;
                    return term;
                });
        }

        /// <summary>
        /// Numerically stable hypotenuse of a right angle triangle, i.e. <code>(a,b) -> sqrt(a^2 + b^2)</code>
        /// </summary>
        /// <param name="a">The length of side a of the triangle.</param>
        /// <param name="b">The length of side b of the triangle.</param>
        /// <returns>Returns <code>sqrt(a<sup>2</sup> + b<sup>2</sup>)</code> without underflow/overflow.</returns>
        public static Complex Hypotenuse(Complex a, Complex b)
        {
            if (a.Magnitude > b.Magnitude)
            {
                var r = b.Magnitude/a.Magnitude;
                return a.Magnitude*Math.Sqrt(1 + (r*r));
            }

            if (b != 0.0)
            {
                // NOTE (ruegg): not "!b.AlmostZero()" to avoid convergence issues (e.g. in SVD algorithm)
                var r = a.Magnitude/b.Magnitude;
                return b.Magnitude*Math.Sqrt(1 + (r*r));
            }

            return 0d;
        }

        /// <summary>
        /// Numerically stable hypotenuse of a right angle triangle, i.e. <code>(a,b) -> sqrt(a^2 + b^2)</code>
        /// </summary>
        /// <param name="a">The length of side a of the triangle.</param>
        /// <param name="b">The length of side b of the triangle.</param>
        /// <returns>Returns <code>sqrt(a<sup>2</sup> + b<sup>2</sup>)</code> without underflow/overflow.</returns>
        public static Complex32 Hypotenuse(Complex32 a, Complex32 b)
        {
            if (a.Magnitude > b.Magnitude)
            {
                var r = b.Magnitude/a.Magnitude;
                return a.Magnitude*(float)Math.Sqrt(1 + (r*r));
            }

            if (b != 0.0f)
            {
                // NOTE (ruegg): not "!b.AlmostZero()" to avoid convergence issues (e.g. in SVD algorithm)
                var r = a.Magnitude/b.Magnitude;
                return b.Magnitude*(float)Math.Sqrt(1 + (r*r));
            }

            return 0f;
        }

        /// <summary>
        /// Numerically stable hypotenuse of a right angle triangle, i.e. <code>(a,b) -> sqrt(a^2 + b^2)</code>
        /// </summary>
        /// <param name="a">The length of side a of the triangle.</param>
        /// <param name="b">The length of side b of the triangle.</param>
        /// <returns>Returns <code>sqrt(a<sup>2</sup> + b<sup>2</sup>)</code> without underflow/overflow.</returns>
        public static double Hypotenuse(double a, double b)
        {
            if (Math.Abs(a) > Math.Abs(b))
            {
                double r = b/a;
                return Math.Abs(a)*Math.Sqrt(1 + (r*r));
            }

            if (b != 0.0)
            {
                // NOTE (ruegg): not "!b.AlmostZero()" to avoid convergence issues (e.g. in SVD algorithm)
                double r = a/b;
                return Math.Abs(b)*Math.Sqrt(1 + (r*r));
            }

            return 0d;
        }

        /// <summary>
        /// Numerically stable hypotenuse of a right angle triangle, i.e. <code>(a,b) -> sqrt(a^2 + b^2)</code>
        /// </summary>
        /// <param name="a">The length of side a of the triangle.</param>
        /// <param name="b">The length of side b of the triangle.</param>
        /// <returns>Returns <code>sqrt(a<sup>2</sup> + b<sup>2</sup>)</code> without underflow/overflow.</returns>
        public static float Hypotenuse(float a, float b)
        {
            if (Math.Abs(a) > Math.Abs(b))
            {
                float r = b/a;
                return Math.Abs(a)*(float)Math.Sqrt(1 + (r*r));
            }

            if (b != 0.0)
            {
                // NOTE (ruegg): not "!b.AlmostZero()" to avoid convergence issues (e.g. in SVD algorithm)
                float r = a/b;
                return Math.Abs(b)*(float)Math.Sqrt(1 + (r*r));
            }

            return 0f;
        }
    }
}
