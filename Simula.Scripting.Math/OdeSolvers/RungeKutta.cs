using System;
using Simula.Maths.LinearAlgebra;

namespace Simula.Maths.OdeSolvers
{
    /// <summary>
    /// ODE Solver Algorithms
    /// </summary>
    public static class RungeKutta
    {
        /// <summary>
        /// Second Order Runge-Kutta method
        /// </summary>
        /// <param name="y0">initial value</param>
        /// <param name="start">start time</param>
        /// <param name="end">end time</param>
        /// <param name="N">Size of output array(the larger, the finer)</param>
        /// <param name="f">ode function</param>
        /// <returns>approximations</returns>
        public static double[] SecondOrder(double y0, double start, double end, int N, Func<double, double, double> f)
        {
            double dt = (end - start) / (N - 1);
            double k1 = 0;
            double k2 = 0;
            double t = start;
            double[] y = new double[N];
            y[0] = y0;
            for (int i = 1; i < N; i++)
            {
                k1 = f(t, y0);
                k2 = f(t + dt, y0 + k1 * dt);
                y[i] = y0 + dt * 0.5 * (k1 + k2);
                t += dt;
                y0 = y[i];
            }
            return y;
        }

        /// <summary>
        /// Fourth Order Runge-Kutta method
        /// </summary>
        /// <param name="y0">initial value</param>
        /// <param name="start">start time</param>
        /// <param name="end">end time</param>
        /// <param name="N">Size of output array(the larger, the finer)</param>
        /// <param name="f">ode function</param>
        /// <returns>approximations</returns>
        public static double[] FourthOrder(double y0, double start, double end, int N, Func<double, double, double> f)
        {
            double dt = (end - start) / (N - 1);
            double k1 = 0;
            double k2 = 0;
            double k3 = 0;
            double k4 = 0;
            double t = start;
            double[] y = new double[N];
            y[0] = y0;
            for (int i = 1; i < N; i++)
            {
                k1 = f(t, y0);
                k2 = f(t + dt / 2, y0 + k1 * dt / 2);
                k3 = f(t + dt / 2, y0 + k2 * dt / 2);
                k4 = f(t + dt, y0 + k3 * dt);
                y[i] = y0 + dt / 6 * (k1 + 2 * k2 + 2 * k3 + k4);
                t += dt;
                y0 = y[i];
            }
            return y;
        }

        /// <summary>
        /// Second Order Runge-Kutta to solve ODE SYSTEM
        /// </summary>
        /// <param name="y0">initial vector</param>
        /// <param name="start">start time</param>
        /// <param name="end">end time</param>
        /// <param name="N">Size of output array(the larger, the finer)</param>
        /// <param name="f">ode function</param>
        /// <returns>approximations</returns>
        public static Vector<double>[] SecondOrder(Vector<double> y0, double start, double end, int N, Func<double, Vector<double>, Vector<double>> f)
        {
            double dt = (end - start) / (N - 1);
            Vector<double> k1, k2;
            Vector<double>[] y = new Vector<double>[N];
            double t = start;
            y[0] = y0;
            for (int i = 1; i < N; i++)
            {
                k1 = f(t, y0);
                k2 = f(t, y0 + k1 * dt);
                y[i] = y0 + dt * 0.5 * (k1 + k2);
                t += dt;
                y0 = y[i];
            }
            return y;
        }

        /// <summary>
        /// Fourth Order Runge-Kutta to solve ODE SYSTEM
        /// </summary>
        /// <param name="y0">initial vector</param>
        /// <param name="start">start time</param>
        /// <param name="end">end time</param>
        /// <param name="N">Size of output array(the larger, the finer)</param>
        /// <param name="f">ode function</param>
        /// <returns>approximations</returns>
        public static Vector<double>[] FourthOrder(Vector<double> y0, double start, double end, int N, Func<double, Vector<double>, Vector<double>> f)
        {
            double dt = (end - start) / (N - 1);
            Vector<double> k1, k2, k3, k4;
            Vector<double>[] y = new Vector<double>[N];
            double t = start;
            y[0] = y0;
            for (int i = 1; i < N; i++)
            {
                k1 = f(t, y0);
                k2 = f(t + dt / 2, y0 + k1 * dt / 2);
                k3 = f(t + dt / 2, y0 + k2 * dt / 2);
                k4 = f(t + dt, y0 + k3 * dt);
                y[i] = y0 + dt / 6 * (k1 + 2 * k2 + 2 * k3 + k4);
                t += dt;
                y0 = y[i];
            }
            return y;
        }
    }
}
