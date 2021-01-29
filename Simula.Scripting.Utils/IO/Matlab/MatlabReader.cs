using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Simula.Maths.LinearAlgebra;

namespace Simula.Maths.IO.Matlab
{
    /// <summary>
    /// Creates matrices from MATLAB Level-5 Mat files.
    /// </summary>
    public static class MatlabReader
    {
        /// <summary>
        /// List all compatible matrices from a MATLAB file stream.
        /// </summary>
        public static List<MatlabMatrix> List(Stream stream)
        {
            return Parser.ParseFile(stream);
        }

        /// <summary>
        /// List all compatible matrices from a MATLAB file.
        /// </summary>
        public static List<MatlabMatrix> List(string filePath)
        {
            using (var stream = File.OpenRead(filePath))
            {
                return List(stream);
            }
        }

        /// <summary>
        /// Unpacks the matrix of a MATLAB matrix data object.
        /// </summary>
        /// <typeparam name="T">The data type of the Matrix. It can be either: double, float, Complex, or Complex32.</typeparam>
        public static Matrix<T> Unpack<T>(MatlabMatrix matrixData)
            where T : struct, IEquatable<T>, IFormattable
        {
            return Parser.ParseMatrix<T>(matrixData.Data);
        }

        /// <summary>
        /// Read the first or a specific matrix from a MATLAB file stream.
        /// </summary>
        /// <typeparam name="T">The data type of the Matrix. It can be either: double, float, Complex, or Complex32.</typeparam>
        public static Matrix<T> Read<T>(Stream stream, string matrixName = null)
            where T : struct, IEquatable<T>, IFormattable
        {
            var matrices = List(stream);

            if (string.IsNullOrEmpty(matrixName))
            {
                return Unpack<T>(matrices.First());
            }

            var matrix = matrices.Find(m => m.Name == matrixName);
            if (matrix == null)
            {
                throw new KeyNotFoundException("Matrix with the provided name was not found.");
            }

            return Unpack<T>(matrix);
        }

        /// <summary>
        /// Read the first or a specific matrix from a MATLAB file.
        /// </summary>
        /// <typeparam name="T">The data type of the Matrix. It can be either: double, float, Complex, or Complex32.</typeparam>
        public static Matrix<T> Read<T>(string filePath, string matrixName = null)
            where T : struct, IEquatable<T>, IFormattable
        {
            using (var stream = File.OpenRead(filePath))
            {
                return Read<T>(stream, matrixName);
            }
        }

        /// <summary>
        /// Read all matrices or those with matching name from a MATLAB file stream.
        /// </summary>
        /// <typeparam name="T">The data type of the Matrix. It can be either: double, float, Complex, or Complex32.</typeparam>
        public static Dictionary<string, Matrix<T>> ReadAll<T>(Stream stream, params string[] matrixNames)
            where T : struct, IEquatable<T>, IFormattable
        {
            var names = new HashSet<string>(matrixNames);
            return List(stream)
                .Where(m => names.Count == 0 || names.Contains(m.Name))
                .ToDictionary(m => m.Name, Unpack<T>);
        }

        /// <summary>
        /// Read all matrices or those with matching name from a MATLAB file.
        /// </summary>
        /// <typeparam name="T">The data type of the Matrix. It can be either: double, float, Complex, or Complex32.</typeparam>
        public static Dictionary<string, Matrix<T>> ReadAll<T>(string filePath, params string[] matrixNames)
            where T : struct, IEquatable<T>, IFormattable
        {
            using (var stream = File.OpenRead(filePath))
            {
                return ReadAll<T>(stream, matrixNames);
            }
        }
    }
}
