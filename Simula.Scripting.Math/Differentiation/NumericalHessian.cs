using System;

namespace Simula.Maths.Differentiation
{
    /// <summary>
    /// Class for evaluating the Hessian of a smooth continuously differentiable function using finite differences.
    /// By default, a central 3-point method is used.
    /// </summary>
    public class NumericalHessian
    {
        /// <summary>
        /// Number of function evaluations.
        /// </summary>
        public int FunctionEvaluations => _df.Evaluations;

        private readonly NumericalDerivative _df;

        /// <summary>
        /// Creates a numerical Hessian object with a three point central difference method.
        /// </summary>
        public NumericalHessian() : this(3, 1) { }

        /// <summary>
        /// Creates a numerical Hessian with a specified differentiation scheme.
        /// </summary>
        /// <param name="points">Number of points for Hessian evaluation.</param>
        /// <param name="center">Center point for differentiation.</param>
        public NumericalHessian(int points, int center)
        {
            _df = new NumericalDerivative(points, center);
        }

        /// <summary>
        /// Evaluates the Hessian of the scalar univariate function f at points x.
        /// </summary>
        /// <param name="f">Scalar univariate function handle.</param>
        /// <param name="x">Point at which to evaluate Hessian.</param>
        /// <returns>Hessian tensor.</returns>
        public double[] Evaluate(Func<double, double> f, double x)
        {
            return new[] { _df.EvaluateDerivative(f, x, 2) };
        }

        /// <summary>
        /// Evaluates the Hessian of a multivariate function f at points x.
        /// </summary>
        /// <remarks>
        /// This method of computing the Hessian is only valid for Lipschitz continuous functions.
        /// The function mirrors the Hessian along the diagonal since d2f/dxdy = d2f/dydx for continuously differentiable functions.
        /// </remarks>
        /// <param name="f">Multivariate function handle.></param>
        /// <param name="x">Points at which to evaluate Hessian.></param>
        /// <returns>Hessian tensor.</returns>
        public double[,] Evaluate(Func<double[], double> f, double[] x)
        {
            var hessian = new double[x.Length, x.Length];

            // Compute diagonal elements
            for (var row = 0; row < x.Length; row++)
            {
                hessian[row, row] = _df.EvaluatePartialDerivative(f, x, row, 2);
            }

            // Compute non-diagonal elements
            for (var row = 0; row < x.Length; row++)
            {
                for (var col = 0; col < row; col++)
                {
                    var mixedPartial = _df.EvaluateMixedPartialDerivative(f, x, new[] { row, col }, 2);

                    hessian[row, col] = mixedPartial;
                    hessian[col, row] = mixedPartial;
                }
            }
            return hessian;
        }

        /// <summary>
        /// Resets the function evaluation counter for the Hessian.
        /// </summary>
        public void ResetFunctionEvaluations()
        {
            _df.ResetEvaluations();
        }
    }
}
