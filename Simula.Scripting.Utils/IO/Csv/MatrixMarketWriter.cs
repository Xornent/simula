using System;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Numerics;
using Simula.Maths.LinearAlgebra;
using Simula.Maths.LinearAlgebra.Storage;

namespace Simula.Maths.IO.Text
{
    /// <summary>
    /// NIST MatrixMarket Format Writer (http://math.nist.gov/MatrixMarket/)
    /// </summary>
    public static class MatrixMarketWriter
    {
        static readonly NumberFormatInfo Format = CultureInfo.InvariantCulture.NumberFormat;

        public static void WriteMatrix<T>(string filePath, Matrix<T> matrix, Compression compression = Compression.Uncompressed) where T : struct, IEquatable<T>, IFormattable
        {
            using (var stream = File.Create(filePath))
            {
                switch (compression)
                {
                    case Compression.Uncompressed:
                        using (var writer = new StreamWriter(stream))
                        {
                            WriteMatrix(writer, matrix);
                        }

                        break;

                    case Compression.GZip:
                        using (var compressed = new GZipStream(stream, CompressionMode.Compress))
#if NETSTANDARD1_3
                        using (var writer = new StreamWriter(compressed))
#else
                        using (var buffered = new BufferedStream(compressed, 4096))
                        using (var writer = new StreamWriter(buffered))
#endif
                        {
                            WriteMatrix(writer, matrix);
                        }

                        break;

                    default:
                        throw new NotSupportedException("Compression not supported: " + compression);
                }
            }
        }

        public static void WriteVector<T>(string filePath, LinearAlgebra.Vector<T> vector, Compression compression = Compression.Uncompressed) where T : struct, IEquatable<T>, IFormattable
        {
            using (var stream = File.Create(filePath))
            {
                switch (compression)
                {
                    case Compression.Uncompressed:
                        using (var writer = new StreamWriter(stream))
                        {
                            WriteVector(writer, vector);
                        }

                        break;

                    case Compression.GZip:
                        using (var compressed = new GZipStream(stream, CompressionMode.Compress))
#if NETSTANDARD1_3
                        using (var writer = new StreamWriter(compressed))
#else
                        using (var buffered = new BufferedStream(compressed, 4096))
                        using (var writer = new StreamWriter(buffered))
#endif
                        {
                            WriteVector(writer, vector);
                        }

                        break;

                    default:
                        throw new NotSupportedException("Compression not supported: " + compression);
                }
            }
        }

        public static void WriteMatrix<T>(Stream stream, Matrix<T> matrix) where T : struct, IEquatable<T>, IFormattable
        {
            using (var writer = new StreamWriter(stream))
            {
                WriteMatrix(writer, matrix);
            }
        }

        public static void WriteVector<T>(Stream stream, LinearAlgebra.Vector<T> vector) where T : struct, IEquatable<T>, IFormattable
        {
            using (var writer = new StreamWriter(stream))
            {
                WriteVector(writer, vector);
            }
        }

        public static void WriteMatrix<T>(TextWriter writer, Matrix<T> matrix) where T : struct, IEquatable<T>, IFormattable
        {
            var complex = typeof (T) == typeof (Complex) || typeof (T) == typeof (Complex32);
            var format = CreateValueFormatter<T>();
            var storage = matrix.Storage;

            var sparse = storage as SparseCompressedRowMatrixStorage<T>;
            if (sparse != null)
            {
                writer.WriteLine("%%MatrixMarket matrix coordinate {0} general", complex ? "complex" : "real");
                writer.WriteLine("{0} {1} {2}", sparse.RowCount, sparse.ColumnCount, sparse.ValueCount);
                for (int row = 0; row < sparse.RowCount; row++)
                {
                    var endIndex = sparse.RowPointers[row + 1];
                    for (var j = sparse.RowPointers[row]; j < endIndex; j++)
                    {
                        writer.WriteLine("{0} {1} {2}", row + 1, sparse.ColumnIndices[j] + 1, format(sparse.Values[j]));
                    }
                }

                return;
            }

            var diagonal = storage as DiagonalMatrixStorage<T>;
            if (diagonal != null)
            {
                writer.WriteLine("%%MatrixMarket matrix coordinate {0} general", complex ? "complex" : "real");
                writer.WriteLine("{0} {1} {2}", diagonal.RowCount, diagonal.ColumnCount, diagonal.Data.Length);
                for (int k = 0; k < diagonal.Data.Length; k++)
                {
                    writer.WriteLine("{0} {1} {2}", k + 1, k + 1, format(diagonal.Data[k]));
                }

                return;
            }

            var dense = storage as DenseColumnMajorMatrixStorage<T>;
            if (dense != null)
            {
                writer.WriteLine("%%MatrixMarket matrix array {0} general", complex ? "complex" : "real");
                writer.WriteLine("{0} {1}", dense.RowCount, dense.ColumnCount);
                foreach (var value in dense.Data)
                {
                    writer.WriteLine(format(value));
                }

                return;
            }

            writer.WriteLine("%%MatrixMarket matrix array {0} general", complex ? "complex" : "real");
            writer.WriteLine("{0} {1}", storage.RowCount, storage.ColumnCount);
            foreach (var value in storage.ToColumnMajorArray())
            {
                writer.WriteLine(format(value));
            }
        }

        public static void WriteVector<T>(TextWriter writer, LinearAlgebra.Vector<T> vector) where T : struct, IEquatable<T>, IFormattable
        {
            var complex = typeof (T) == typeof (Complex) || typeof (T) == typeof (Complex32);
            var format = CreateValueFormatter<T>();
            var storage = vector.Storage;

            var sparse = storage as SparseVectorStorage<T>;
            if (sparse != null)
            {
                writer.WriteLine("%%MatrixMarket vector coordinate {0}", complex ? "complex" : "real");
                writer.WriteLine("{0} {1}", sparse.Length, sparse.ValueCount);
                for (var k = 0; k < sparse.ValueCount; k++)
                {
                    writer.WriteLine("{0} {1}", k + 1, format(sparse.Values[k]));
                }

                return;
            }

            var dense = storage as DenseVectorStorage<T>;
            if (dense != null)
            {
                writer.WriteLine("%%MatrixMarket vector array {0}", complex ? "complex" : "real");
                writer.WriteLine("{0}", dense.Length);
                foreach (var value in dense.Data)
                {
                    writer.WriteLine(format(value));
                }

                return;
            }

            writer.WriteLine("%%MatrixMarket vector array {0}", complex ? "complex" : "real");
            writer.WriteLine("{0}", storage.Length);
            foreach (var value in storage.Enumerate())
            {
                writer.WriteLine(format(value));
            }
        }

        static Func<T, string> CreateValueFormatter<T>() where T : IFormattable
        {
            if (typeof (T) == typeof (double))
            {
                return value => string.Format(Format, "{0:G14}", value);
            }

            if (typeof (T) == typeof (float))
            {
                return value => string.Format(Format, "{0:G7}", value);
            }

            if (typeof (T) == typeof (Complex))
            {
                return value =>
                {
                    var c = (Complex)(object)value;
                    return string.Format(Format, "{0:G14} {1:G14}", c.Real, c.Imaginary);
                };
            }

            if (typeof (T) == typeof (Complex32))
            {
                return value =>
                {
                    var c = (Complex32)(object)value;
                    return string.Format(Format, "{0:G7} {1:G7}", c.Real, c.Imaginary);
                };
            }

            throw new NotSupportedException();
        }
    }
}
