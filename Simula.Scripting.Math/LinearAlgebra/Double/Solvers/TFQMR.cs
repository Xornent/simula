using System;
using Simula.Maths.LinearAlgebra.Solvers;

namespace Simula.Maths.LinearAlgebra.Double.Solvers
{
    /// <summary>
    /// A Transpose Free Quasi-Minimal Residual (TFQMR) iterative matrix solver.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The TFQMR algorithm was taken from: <br/>
    /// Iterative methods for sparse linear systems.
    /// <br/>
    /// Yousef Saad
    /// <br/>
    /// Algorithm is described in Chapter 7, section 7.4.3, page 219
    /// </para>
    /// <para>
    /// The example code below provides an indication of the possible use of the
    /// solver.
    /// </para>
    /// </remarks>
    public sealed class TFQMR : IIterativeSolver<double>
    {
        /// <summary>
        /// Calculates the <c>true</c> residual of the matrix equation Ax = b according to: residual = b - Ax
        /// </summary>
        /// <param name="matrix">Instance of the <see cref="Matrix"/> A.</param>
        /// <param name="residual">Residual values in <see cref="Vector"/>.</param>
        /// <param name="x">Instance of the <see cref="Vector"/> x.</param>
        /// <param name="b">Instance of the <see cref="Vector"/> b.</param>
        static void CalculateTrueResidual(Matrix<double> matrix, Vector<double> residual, Vector<double> x, Vector<double> b)
        {
            // -Ax = residual
            matrix.Multiply(x, residual);
            residual.Multiply(-1, residual);

            // residual + b
            residual.Add(b, residual);
        }

        /// <summary>
        /// Is <paramref name="number"/> even?
        /// </summary>
        /// <param name="number">Number to check</param>
        /// <returns><c>true</c> if <paramref name="number"/> even, otherwise <c>false</c></returns>
        static bool IsEven(int number)
        {
            return number % 2 == 0;
        }

        /// <summary>
        /// Solves the matrix equation Ax = b, where A is the coefficient matrix, b is the
        /// solution vector and x is the unknown vector.
        /// </summary>
        /// <param name="matrix">The coefficient matrix, <c>A</c>.</param>
        /// <param name="input">The solution vector, <c>b</c></param>
        /// <param name="result">The result vector, <c>x</c></param>
        /// <param name="iterator">The iterator to use to control when to stop iterating.</param>
        /// <param name="preconditioner">The preconditioner to use for approximations.</param>
        public void Solve(Matrix<double> matrix, Vector<double> input, Vector<double> result, Iterator<double> iterator, IPreconditioner<double> preconditioner)
        {
            if (matrix.RowCount != matrix.ColumnCount)
            {
                throw new ArgumentException("Matrix must be square.", nameof(matrix));
            }

            if (result.Count != input.Count)
            {
                throw new ArgumentException("All vectors must have the same dimensionality.");
            }

            if (input.Count != matrix.RowCount)
            {
                throw Matrix.DimensionsDontMatch<ArgumentException>(input, matrix);
            }

            if (iterator == null)
            {
                iterator = new Iterator<double>();
            }

            if (preconditioner == null)
            {
                preconditioner = new UnitPreconditioner<double>();
            }

            preconditioner.Initialize(matrix);

            var d = new DenseVector(input.Count);
            var r = DenseVector.OfVector(input);

            var uodd = new DenseVector(input.Count);
            var ueven = new DenseVector(input.Count);

            var v = new DenseVector(input.Count);
            var pseudoResiduals = DenseVector.OfVector(input);

            var x = new DenseVector(input.Count);
            var yodd = new DenseVector(input.Count);
            var yeven = DenseVector.OfVector(input);

            // Temp vectors
            var temp = new DenseVector(input.Count);
            var temp1 = new DenseVector(input.Count);
            var temp2 = new DenseVector(input.Count);

            // Define the scalars
            double alpha = 0;
            double eta = 0;
            double theta = 0;

            // Initialize
            var tau = input.L2Norm();
            var rho = tau*tau;

            // Calculate the initial values for v
            // M temp = yEven
            preconditioner.Approximate(yeven, temp);

            // v = A temp
            matrix.Multiply(temp, v);

            // Set uOdd
            v.CopyTo(ueven);

            // Start the iteration
            var iterationNumber = 0;
            while (iterator.DetermineStatus(iterationNumber, result, input, pseudoResiduals) == IterationStatus.Continue)
            {
                // First part of the step, the even bit
                if (IsEven(iterationNumber))
                {
                    // sigma = (v, r)
                    var sigma = v.DotProduct(r);
                    if (sigma.AlmostEqualNumbersBetween(0, 1))
                    {
                        // FAIL HERE
                        iterator.Cancel();
                        break;
                    }

                    // alpha = rho / sigma
                    alpha = rho/sigma;

                    // yOdd = yEven - alpha * v
                    v.Multiply(-alpha, temp1);
                    yeven.Add(temp1, yodd);

                    // Solve M temp = yOdd
                    preconditioner.Approximate(yodd, temp);

                    // uOdd = A temp
                    matrix.Multiply(temp, uodd);
                }

                // The intermediate step which is equal for both even and
                // odd iteration steps.
                // Select the correct vector
                var uinternal = IsEven(iterationNumber) ? ueven : uodd;
                var yinternal = IsEven(iterationNumber) ? yeven : yodd;

                // pseudoResiduals = pseudoResiduals - alpha * uOdd
                uinternal.Multiply(-alpha, temp1);
                pseudoResiduals.Add(temp1, temp2);
                temp2.CopyTo(pseudoResiduals);

                // d = yOdd + theta * theta * eta / alpha * d
                d.Multiply(theta*theta*eta/alpha, temp);
                yinternal.Add(temp, d);

                // theta = ||pseudoResiduals||_2 / tau
                theta = pseudoResiduals.L2Norm()/tau;
                var c = 1/Math.Sqrt(1 + (theta*theta));

                // tau = tau * theta * c
                tau *= theta*c;

                // eta = c^2 * alpha
                eta = c*c*alpha;

                // x = x + eta * d
                d.Multiply(eta, temp1);
                x.Add(temp1, temp2);
                temp2.CopyTo(x);

                // Check convergence and see if we can bail
                if (iterator.DetermineStatus(iterationNumber, result, input, pseudoResiduals) != IterationStatus.Continue)
                {
                    // Calculate the real values
                    preconditioner.Approximate(x, result);

                    // Calculate the true residual. Use the temp vector for that
                    // so that we don't pollute the pseudoResidual vector for no
                    // good reason.
                    CalculateTrueResidual(matrix, temp, result, input);

                    // Now recheck the convergence
                    if (iterator.DetermineStatus(iterationNumber, result, input, temp) != IterationStatus.Continue)
                    {
                        // We're all good now.
                        return;
                    }
                }

                // The odd step
                if (!IsEven(iterationNumber))
                {
                    if (rho.AlmostEqualNumbersBetween(0, 1))
                    {
                        // FAIL HERE
                        iterator.Cancel();
                        break;
                    }

                    var rhoNew = pseudoResiduals.DotProduct(r);
                    var beta = rhoNew/rho;

                    // Update rho for the next loop
                    rho = rhoNew;

                    // yOdd = pseudoResiduals + beta * yOdd
                    yodd.Multiply(beta, temp1);
                    pseudoResiduals.Add(temp1, yeven);

                    // Solve M temp = yOdd
                    preconditioner.Approximate(yeven, temp);

                    // uOdd = A temp
                    matrix.Multiply(temp, ueven);

                    // v = uEven + beta * (uOdd + beta * v)
                    v.Multiply(beta, temp1);
                    uodd.Add(temp1, temp);

                    temp.Multiply(beta, temp1);
                    ueven.Add(temp1, v);
                }

                // Calculate the real values
                preconditioner.Approximate(x, result);

                iterationNumber++;
            }
        }
    }
}