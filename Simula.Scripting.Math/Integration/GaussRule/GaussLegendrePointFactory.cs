﻿using System;

namespace Simula.Maths.Integration.GaussRule
{
    /// <summary>
    /// Creates and maps a Gauss-Legendre point.
    /// </summary>
    internal static class GaussLegendrePointFactory
    {
        [ThreadStatic]
        private static GaussPoint _gaussLegendrePoint;

        /// <summary>
        /// Getter for the GaussPoint.
        /// </summary>
        /// <param name="order">Defines an Nth order Gauss-Legendre rule. Precomputed Gauss-Legendre abscissas/weights for orders 2-20, 32, 64, 96, 100, 128, 256, 512, 1024 are used, otherwise they're calculated on the fly.</param>
        /// <returns>Object containing the non-negative abscissas/weights, order, and intervalBegin/intervalEnd. The non-negative abscissas/weights are generated over the interval [-1,1] for the given order.</returns>
        public static GaussPoint GetGaussPoint(int order)
        {
            // Try to get the GaussPoint from the cached static field.
            bool gaussLegendrePointIsCached = _gaussLegendrePoint != null && _gaussLegendrePoint.Order == order;
            if (!gaussLegendrePointIsCached)
            {
                // Try to find the GaussPoint in the precomputed dictionary.
                if (!GaussLegendrePoint.PreComputed.TryGetValue(order, out _gaussLegendrePoint))
                {
                    _gaussLegendrePoint = GaussLegendrePoint.Generate(order, 1e-10); // Generate the GaussPoint on the fly.
                }
            }

            return _gaussLegendrePoint;
        }

        /// <summary>
        /// Getter for the GaussPoint.
        /// </summary>
        /// <param name="intervalBegin">Where the interval starts, inclusive and finite.</param>
        /// <param name="intervalEnd">Where the interval stops, inclusive and finite.</param>
        /// <param name="order">Defines an Nth order Gauss-Legendre rule. Precomputed Gauss-Legendre abscissas/weights for orders 2-20, 32, 64, 96, 100, 128, 256, 512, 1024 are used, otherwise they're calculated on the fly.</param>
        /// <returns>Object containing the abscissas/weights, order, and intervalBegin/intervalEnd.</returns>
        public static GaussPoint GetGaussPoint(double intervalBegin, double intervalEnd, int order)
        {
            return Map(intervalBegin, intervalEnd, GetGaussPoint(order));
        }

        /// <summary>
        /// Maps the non-negative abscissas/weights from the interval [-1, 1] to the interval [intervalBegin, intervalEnd].
        /// </summary>
        /// <param name="intervalBegin">Where the interval starts, inclusive and finite.</param>
        /// <param name="intervalEnd">Where the interval stops, inclusive and finite.</param>
        /// <param name="gaussPoint">Object containing the non-negative abscissas/weights, order, and intervalBegin/intervalEnd. The non-negative abscissas/weights are generated over the interval [-1,1] for the given order.</param>
        /// <returns>Object containing the abscissas/weights, order, and intervalBegin/intervalEnd.</returns>
        private static GaussPoint Map(double intervalBegin, double intervalEnd, GaussPoint gaussPoint)
        {
            double[] abscissas = new double[gaussPoint.Order];
            double[] weights = new double[gaussPoint.Order];

            double a = 0.5 * (intervalEnd - intervalBegin);
            double b = 0.5 * (intervalEnd + intervalBegin);

            int m = (gaussPoint.Order + 1) >> 1;

            for (int i = 1; i <= m; i++)
            {
                int index1 = gaussPoint.Order - i;
                int index2 = i - 1;
                int index3 = m - i;

                abscissas[index1] = gaussPoint.Abscissas[index3] * a + b;
                abscissas[index2] = -gaussPoint.Abscissas[index3] * a + b;

                weights[index1] = gaussPoint.Weights[index3] * a;
                weights[index2] = gaussPoint.Weights[index3] * a;
            }

            return new GaussPoint(intervalBegin, intervalEnd, gaussPoint.Order, abscissas, weights);
        }
    }
}
