using System;
using System.Collections.Generic;
using System.IO;
using Simula.Maths.LinearAlgebra;

namespace Simula.Maths.IO.Text
{
    /// <summary>
    /// Writes an <see cref="Matrix{TDataType}"/> to delimited text file. If the user does not
    /// specify a delimiter, a tab separator is used.
    /// </summary>
    public static class DelimitedWriter
    {
        /// <summary>
        /// Writes a matrix to the given TextWriter. Optionally accepts custom column headers, delimiter, number format and culture.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to write the matrix to.</param>
        /// <param name="matrix">The matrix to write.</param>
        /// <param name="delimiter">Number delimiter to write between numbers of the same line. Default:  "\t" (tabulator).</param>
        /// <param name="columnHeaders">Custom column header. Headers are only written if non-null and non-empty headers are provided. Default: null.</param>
        /// <param name="format">The number format to use on each element. Default: null.</param>
        /// <param name="formatProvider">The culture to use. Default: null.</param>
        /// <param name="missingValue">A value that represents a missing value. If not null, then elements of the matrix that have this value
        /// are not written to the output file.</param>
        /// <exception cref="ArgumentNullException">If either <paramref name="matrix"/> or <paramref name="writer"/> is <c>null</c>.</exception>
        /// <typeparam name="T">The data type of the Matrix. It can be either: Double, Single, Complex, or Complex32.</typeparam>
        public static void Write<T>(TextWriter writer, Matrix<T> matrix, string delimiter = "\t", IList<string> columnHeaders = null, string format = null, IFormatProvider formatProvider = null, T? missingValue = null)
            where T : struct, IEquatable<T>, IFormattable
        {
            if (matrix == null)
            {
                throw new ArgumentNullException("matrix");
            }

            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }

            if (columnHeaders != null && columnHeaders.Count > 0)
            {
                for (var i = 0; i < columnHeaders.Count - 1; i++)
                {
                    writer.Write(columnHeaders[i]);
                    writer.Write(delimiter);
                }

                writer.WriteLine(columnHeaders[columnHeaders.Count - 1]);
            }

            var cols = matrix.ColumnCount - 1;
            var rows = matrix.RowCount - 1;
            if (!missingValue.HasValue)
            {
                for (var i = 0; i < matrix.RowCount; i++)
                {
                    for (var j = 0; j < matrix.ColumnCount; j++)
                    {
                        writer.Write(matrix[i, j].ToString(format, formatProvider));

                        if (j != cols)
                        {
                            writer.Write(delimiter);
                        }
                    }

                    if (i != rows)
                    {
                        writer.Write(Environment.NewLine);
                    }
                }
            }
            else
            {
                var missing = missingValue.Value;

                for (var i = 0; i < matrix.RowCount; i++)
                {
                    for (var j = 0; j < matrix.ColumnCount; j++)
                    {
                        if (!matrix[i, j].Equals(missing))
                        {
                            writer.Write(matrix[i, j].ToString(format, formatProvider));
                        }

                        if (j != cols)
                        {
                            writer.Write(delimiter);
                        }
                    }

                    if (i != rows)
                    {
                        writer.Write(Environment.NewLine);
                    }
                }
            }
        }

        /// <summary>
        /// Writes a matrix to the given file. Optionally accepts custom column headers, delimiter, number format and culture.
        /// </summary>
        /// <param name="filePath">The path and name of the file to write the matrix to. If the file already exists, the file will be overwritten.</param>
        /// <param name="matrix">The matrix to write.</param>
        /// <param name="delimiter">Number delimiter to write between numbers of the same line. Default:  "\t" (tabulator).</param>
        /// <param name="columnHeaders">Custom column header. Headers are only written if non-null and non-empty headers are provided. Default: null.</param>
        /// <param name="format">The number format to use on each element. Default: null.</param>
        /// <param name="formatProvider">The culture to use. Default: null.</param>
        /// <param name="missingValue">A value that represents a missing value. If not null, then elements of the matrix that have this value
        /// are not written to the output file.</param>
        /// <exception cref="ArgumentNullException">If either <paramref name="matrix"/> or <paramref name="filePath"/> is <c>null</c>.</exception>
        /// <typeparam name="T">The data type of the Matrix. It can be either: Double, Single, Complex, or Complex32.</typeparam>
        public static void Write<T>(string filePath, Matrix<T> matrix, string delimiter = "\t", IList<string> columnHeaders = null, string format = null, IFormatProvider formatProvider = null, T? missingValue = null)
            where T : struct, IEquatable<T>, IFormattable
        {
            using (var stream = File.Create(filePath))
            using (var writer = new StreamWriter(stream))
            {
                Write(writer, matrix, delimiter: delimiter, columnHeaders: columnHeaders, format: format, formatProvider: formatProvider, missingValue: missingValue);
            }
        }

        /// <summary>
        /// Writes a matrix to the given stream. Optionally accepts custom column headers, delimiter, number format and culture.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to write the matrix to.</param>
        /// <param name="matrix">The matrix to write.</param>
        /// <param name="delimiter">Number delimiter to write between numbers of the same line. Default: "\t" (tabulator).</param>
        /// <param name="columnHeaders">Custom column header. Headers are only written if non-null and non-empty headers are provided. Default: null.</param>
        /// <param name="format">The number format to use on each element. Default: null.</param>
        /// <param name="formatProvider">The culture to use. Default: null.</param>
        /// <param name="missingValue">A value that represents a missing value. If not null, then elements of the matrix that have this value
        /// are not written to the output file.</param>
        /// <exception cref="ArgumentNullException">If either <paramref name="matrix"/> or <paramref name="stream"/> is <c>null</c>.</exception>
        /// <typeparam name="T">The data type of the Matrix. It can be either: Double, Single, Complex, or Complex32.</typeparam>
        public static void Write<T>(Stream stream, Matrix<T> matrix, string delimiter = "\t", IList<string> columnHeaders = null, string format = null, IFormatProvider formatProvider = null, T? missingValue = null)
            where T : struct, IEquatable<T>, IFormattable
        {
            using (var writer = new StreamWriter(stream))
            {
                Write(writer, matrix, delimiter: delimiter, columnHeaders: columnHeaders, format: format, formatProvider: formatProvider, missingValue: missingValue);
            }
        }
    }
}
