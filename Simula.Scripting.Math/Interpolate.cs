using System.Collections.Generic;
using Simula.Maths.Interpolation;

namespace Simula.Maths
{
    /// <summary>
    /// Interpolation Factory.
    /// </summary>
    public static class Interpolate
    {
        /// <summary>
        /// Creates an interpolation based on arbitrary points.
        /// </summary>
        /// <param name="points">The sample points t.</param>
        /// <param name="values">The sample point values x(t).</param>
        /// <returns>
        /// An interpolation scheme optimized for the given sample points and values,
        /// which can then be used to compute interpolations and extrapolations
        /// on arbitrary points.
        /// </returns>
        /// <remarks>
        /// if your data is already sorted in arrays, consider to use
        /// Simula.Maths.Interpolation.Barycentric.InterpolateRationalFloaterHormannSorted
        /// instead, which is more efficient.
        /// </remarks>
        public static IInterpolation Common(IEnumerable<double> points, IEnumerable<double> values)
        {
            return Barycentric.InterpolateRationalFloaterHormann(points, values);
        }

        /// <summary>
        /// Create a Floater-Hormann rational pole-free interpolation based on arbitrary points.
        /// </summary>
        /// <param name="points">The sample points t.</param>
        /// <param name="values">The sample point values x(t).</param>
        /// <returns>
        /// An interpolation scheme optimized for the given sample points and values,
        /// which can then be used to compute interpolations and extrapolations
        /// on arbitrary points.
        /// </returns>
        /// <remarks>
        /// if your data is already sorted in arrays, consider to use
        /// Simula.Maths.Interpolation.Barycentric.InterpolateRationalFloaterHormannSorted
        /// instead, which is more efficient.
        /// </remarks>
        public static IInterpolation RationalWithoutPoles(IEnumerable<double> points, IEnumerable<double> values)
        {
            return Barycentric.InterpolateRationalFloaterHormann(points, values);
        }

        /// <summary>
        /// Create a Bulirsch Stoer rational interpolation based on arbitrary points.
        /// </summary>
        /// <param name="points">The sample points t.</param>
        /// <param name="values">The sample point values x(t).</param>
        /// <returns>
        /// An interpolation scheme optimized for the given sample points and values,
        /// which can then be used to compute interpolations and extrapolations
        /// on arbitrary points.
        /// </returns>
        /// <remarks>
        /// if your data is already sorted in arrays, consider to use
        /// Simula.Maths.Interpolation.BulirschStoerRationalInterpolation.InterpolateSorted
        /// instead, which is more efficient.
        /// </remarks>
        public static IInterpolation RationalWithPoles(IEnumerable<double> points, IEnumerable<double> values)
        {
            return BulirschStoerRationalInterpolation.Interpolate(points, values);
        }

        /// <summary>
        /// Create a barycentric polynomial interpolation where the given sample points are equidistant.
        /// </summary>
        /// <param name="points">The sample points t, must be equidistant.</param>
        /// <param name="values">The sample point values x(t).</param>
        /// <returns>
        /// An interpolation scheme optimized for the given sample points and values,
        /// which can then be used to compute interpolations and extrapolations
        /// on arbitrary points.
        /// </returns>
        /// <remarks>
        /// if your data is already sorted in arrays, consider to use
        /// Simula.Maths.Interpolation.Barycentric.InterpolatePolynomialEquidistantSorted
        /// instead, which is more efficient.
        /// </remarks>
        public static IInterpolation PolynomialEquidistant(IEnumerable<double> points, IEnumerable<double> values)
        {
            return Barycentric.InterpolatePolynomialEquidistant(points, values);
        }

        /// <summary>
        /// Create a Neville polynomial interpolation based on arbitrary points.
        /// If the points happen to be equidistant, consider to use the much more robust PolynomialEquidistant instead.
        /// Otherwise, consider whether RationalWithoutPoles would not be a more robust alternative.
        /// </summary>
        /// <param name="points">The sample points t.</param>
        /// <param name="values">The sample point values x(t).</param>
        /// <returns>
        /// An interpolation scheme optimized for the given sample points and values,
        /// which can then be used to compute interpolations and extrapolations
        /// on arbitrary points.
        /// </returns>
        /// <remarks>
        /// if your data is already sorted in arrays, consider to use
        /// Simula.Maths.Interpolation.NevillePolynomialInterpolation.InterpolateSorted
        /// instead, which is more efficient.
        /// </remarks>
        public static IInterpolation Polynomial(IEnumerable<double> points, IEnumerable<double> values)
        {
            return NevillePolynomialInterpolation.Interpolate(points, values);
        }

        /// <summary>
        /// Create a piecewise linear interpolation based on arbitrary points.
        /// </summary>
        /// <param name="points">The sample points t.</param>
        /// <param name="values">The sample point values x(t).</param>
        /// <returns>
        /// An interpolation scheme optimized for the given sample points and values,
        /// which can then be used to compute interpolations and extrapolations
        /// on arbitrary points.
        /// </returns>
        /// <remarks>
        /// if your data is already sorted in arrays, consider to use
        /// Simula.Maths.Interpolation.LinearSpline.InterpolateSorted
        /// instead, which is more efficient.
        /// </remarks>
        public static IInterpolation Linear(IEnumerable<double> points, IEnumerable<double> values)
        {
            return Interpolation.LinearSpline.Interpolate(points, values);
        }

        /// <summary>
        /// Create piecewise log-linear interpolation based on arbitrary points.
        /// </summary>
        /// <param name="points">The sample points t.</param>
        /// <param name="values">The sample point values x(t).</param>
        /// <returns>
        /// An interpolation scheme optimized for the given sample points and values,
        /// which can then be used to compute interpolations and extrapolations
        /// on arbitrary points.
        /// </returns>
        /// <remarks>
        /// if your data is already sorted in arrays, consider to use
        /// Simula.Maths.Interpolation.LogLinear.InterpolateSorted
        /// instead, which is more efficient.
        /// </remarks>
        public static IInterpolation LogLinear(IEnumerable<double> points, IEnumerable<double> values)
        {
            return Interpolation.LogLinear.Interpolate(points, values);
        }

        /// <summary>
        /// Create an piecewise natural cubic spline interpolation based on arbitrary points,
        /// with zero secondary derivatives at the boundaries.
        /// </summary>
        /// <param name="points">The sample points t.</param>
        /// <param name="values">The sample point values x(t).</param>
        /// <returns>
        /// An interpolation scheme optimized for the given sample points and values,
        /// which can then be used to compute interpolations and extrapolations
        /// on arbitrary points.
        /// </returns>
        /// <remarks>
        /// if your data is already sorted in arrays, consider to use
        /// Simula.Maths.Interpolation.CubicSpline.InterpolateNaturalSorted
        /// instead, which is more efficient.
        /// </remarks>
        public static IInterpolation CubicSpline(IEnumerable<double> points, IEnumerable<double> values)
        {
            return Interpolation.CubicSpline.InterpolateNatural(points, values);
        }

        /// <summary>
        /// Create a piecewise cubic Akima spline interpolation based on arbitrary points.
        /// Akima splines are robust to outliers.
        /// </summary>
        /// <param name="points">The sample points t.</param>
        /// <param name="values">The sample point values x(t).</param>
        /// <returns>
        /// An interpolation scheme optimized for the given sample points and values,
        /// which can then be used to compute interpolations and extrapolations
        /// on arbitrary points.
        /// </returns>
        /// <remarks>
        /// if your data is already sorted in arrays, consider to use
        /// Simula.Maths.Interpolation.CubicSpline.InterpolateAkimaSorted
        /// instead, which is more efficient.
        /// </remarks>
        public static IInterpolation CubicSplineRobust(IEnumerable<double> points, IEnumerable<double> values)
        {
            return Interpolation.CubicSpline.InterpolateAkima(points, values);
        }

        /// <summary>
        /// Create a piecewise cubic monotone spline interpolation based on arbitrary points.
        /// This is a shape-preserving spline with continuous first derivative.
        /// </summary>
        /// <param name="points">The sample points t.</param>
        /// <param name="values">The sample point values x(t).</param>
        /// <returns>
        /// An interpolation scheme optimized for the given sample points and values,
        /// which can then be used to compute interpolations and extrapolations
        /// on arbitrary points.
        /// </returns>
        /// <remarks>
        /// if your data is already sorted in arrays, consider to use
        /// Simula.Maths.Interpolation.CubicSpline.InterpolatePchipSorted
        /// instead, which is more efficient.
        /// </remarks>
        public static IInterpolation CubicSplineMonotone(IEnumerable<double> points, IEnumerable<double> values)
        {
            return Interpolation.CubicSpline.InterpolatePchip(points, values);
        }

        /// <summary>
        /// Create a piecewise cubic Hermite spline interpolation based on arbitrary points
        /// and their slopes/first derivative.
        /// </summary>
        /// <param name="points">The sample points t.</param>
        /// <param name="values">The sample point values x(t).</param>
        /// <param name="firstDerivatives">The slope at the sample points. Optimized for arrays.</param>
        /// <returns>
        /// An interpolation scheme optimized for the given sample points and values,
        /// which can then be used to compute interpolations and extrapolations
        /// on arbitrary points.
        /// </returns>
        /// <remarks>
        /// if your data is already sorted in arrays, consider to use
        /// Simula.Maths.Interpolation.CubicSpline.InterpolateHermiteSorted
        /// instead, which is more efficient.
        /// </remarks>
        public static IInterpolation CubicSplineWithDerivatives(IEnumerable<double> points, IEnumerable<double> values, IEnumerable<double> firstDerivatives)
        {
            return Interpolation.CubicSpline.InterpolateHermite(points, values, firstDerivatives);
        }

        /// <summary>
        /// Create a step-interpolation based on arbitrary points.
        /// </summary>
        /// <param name="points">The sample points t.</param>
        /// <param name="values">The sample point values x(t).</param>
        /// <returns>
        /// An interpolation scheme optimized for the given sample points and values,
        /// which can then be used to compute interpolations and extrapolations
        /// on arbitrary points.
        /// </returns>
        /// <remarks>
        /// if your data is already sorted in arrays, consider to use
        /// Simula.Maths.Interpolation.StepInterpolation.InterpolateSorted
        /// instead, which is more efficient.
        /// </remarks>
        public static IInterpolation Step(IEnumerable<double> points, IEnumerable<double> values)
        {
            return StepInterpolation.Interpolate(points, values);
        }
    }
}
