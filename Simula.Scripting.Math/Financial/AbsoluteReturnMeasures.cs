using System;
using System.Collections.Generic;
using System.Linq;
using Simula.Maths.Statistics;

namespace Simula.Maths.Financial
{
    public static class AbsoluteReturnMeasures
    {
        /// <summary>
        /// Compound Monthly Return or Geometric Return or Annualized Return
        /// </summary>
        public static double CompoundReturn(this IEnumerable<double> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            int count = 0;
            double compoundReturn = 1.0;
            foreach (var item in data)
            {
                count++;
                compoundReturn *= 1 + item;
            }

            return count == 0 ? double.NaN : Math.Pow(compoundReturn, 1.0/count) - 1.0;
        }

        /// <summary>
        /// Average Gain or Gain Mean
        /// This is a simple average (arithmetic mean) of the periods with a gain. It is calculated by summing the returns for gain periods (return 0)
        /// and then dividing the total by the number of gain periods.
        /// </summary>
        /// <remarks>http://www.offshore-library.com/kb/statistics.php</remarks>
        public static double GainMean(this IEnumerable<double> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            return data.Where(x => x >= 0).Mean();
        }

        /// <summary>
        /// Average Loss or LossMean
        /// This is a simple average (arithmetic mean) of the periods with a loss. It is calculated by summing the returns for loss periods (return &lt; 0)
        /// and then dividing the total by the number of loss periods.
        /// </summary>
        /// <remarks>http://www.offshore-library.com/kb/statistics.php</remarks>
        public static double LossMean(this IEnumerable<double> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            return data.Where(x => x < 0).Mean();
        }
    }
}
