using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Simula.Maths.LinearAlgebra;

namespace Simula.Maths.IO.Matlab
{
    /// <summary>
    /// Writes matrices to a MATLAB Level-5 Mat file.
    /// </summary>
    public static class MatlabWriter
    {
        public static void Store(Stream stream, IEnumerable<MatlabMatrix> matrices)
        {
            Formatter.FormatFile(stream, matrices);
        }

        public static void Store(string filePath, IEnumerable<MatlabMatrix> matrices)
        {
            using (var stream = File.Create(filePath))
            {
                Store(stream, matrices);
            }
        }

        /// <typeparam name="T">The data type of the Matrix. It can be either: double, float, Complex, or Complex32.</typeparam>
        public static MatlabMatrix Pack<T>(Matrix<T> matrix, string matrixName)
            where T : struct, IEquatable<T>, IFormattable
        {
            return Formatter.FormatMatrix(matrix, matrixName);
        }

        /// <typeparam name="T">The data type of the Matrix. It can be either: double, float, Complex, or Complex32.</typeparam>
        public static void Write<T>(Stream stream, Matrix<T> matrix, string matrixName)
            where T : struct, IEquatable<T>, IFormattable
        {
            Store(stream, new[] { Pack(matrix, matrixName) });
        }

        /// <typeparam name="T">The data type of the Matrix. It can be either: double, float, Complex, or Complex32.</typeparam>
        public static void Write<T>(string filePath, Matrix<T> matrix, string matrixName)
            where T : struct, IEquatable<T>, IFormattable
        {
            Store(filePath, new[] { Pack(matrix, matrixName) });
        }

        /// <typeparam name="T">The data type of the Matrix. It can be either: double, float, Complex, or Complex32.</typeparam>
        public static void Write<T>(Stream stream, IList<Matrix<T>> matrices, IList<string> names)
            where T : struct, IEquatable<T>, IFormattable
        {
            if (matrices.Count != names.Count)
            {
                throw new ArgumentException("Each matrix must have a name. Number of matrices must equal to the number of names.");
            }

            Store(stream, matrices.Zip(names, Pack));
        }

        /// <typeparam name="T">The data type of the Matrix. It can be either: double, float, Complex, or Complex32.</typeparam>
        public static void Write<T>(string filePath, IList<Matrix<T>> matrices, IList<string> names)
            where T : struct, IEquatable<T>, IFormattable
        {
            if (matrices.Count != names.Count)
            {
                throw new ArgumentException("Each matrix must have a name. Number of matrices must equal to the number of names.");
            }

            Store(filePath, matrices.Zip(names, Pack));
        }

        /// <typeparam name="T">The data type of the Matrix. It can be either: double, float, Complex, or Complex32.</typeparam>
        public static void Write<T>(Stream stream, IEnumerable<KeyValuePair<string, Matrix<T>>> matrices)
            where T : struct, IEquatable<T>, IFormattable
        {
            Store(stream, matrices.Select(kv => Pack(kv.Value, kv.Key)));
        }

        /// <typeparam name="T">The data type of the Matrix. It can be either: double, float, Complex, or Complex32.</typeparam>
        public static void Write<T>(string filePath, IEnumerable<KeyValuePair<string, Matrix<T>>> matrices)
            where T : struct, IEquatable<T>, IFormattable
        {
            Store(filePath, matrices.Select(kv => Pack(kv.Value, kv.Key)));
        }
    }
}
