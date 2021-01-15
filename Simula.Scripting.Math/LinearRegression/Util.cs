using System;
using System.Collections.Generic;

namespace Simula.Maths.LinearRegression
{
    internal static class Util
    {
        public static Tuple<TU[], TV[]> UnpackSinglePass<TU, TV>(this IEnumerable<Tuple<TU, TV>> samples)
        {
            var u = new List<TU>();
            var v = new List<TV>();

            foreach (var tuple in samples)
            {
                u.Add(tuple.Item1);
                v.Add(tuple.Item2);
            }

            return new Tuple<TU[], TV[]>(u.ToArray(), v.ToArray());
        }
    }
}
