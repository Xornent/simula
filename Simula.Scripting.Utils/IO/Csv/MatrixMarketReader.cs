﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Numerics;
using Simula.Maths.LinearAlgebra;

namespace Simula.Maths.IO.Text
{
    internal enum MatrixMarketSymmetry
    {
        General,
        Symmetric,
        SkewSymmetric,
        Hermitian
    }

    /// <summary>
    /// NIST MatrixMarket Format Reader (http://math.nist.gov/MatrixMarket/)
    /// </summary>
    public static class MatrixMarketReader
    {
        static readonly char[] Separators = { ' ' };
        static readonly NumberFormatInfo Format = CultureInfo.InvariantCulture.NumberFormat;

        public static Matrix<T> ReadMatrix<T>(string filePath, Compression compression = Compression.Uncompressed) where T : struct, IEquatable<T>, IFormattable
        {
            using (var stream = File.OpenRead(filePath))
            {
                switch (compression)
                {
                    case Compression.Uncompressed:
                        using (var reader = new StreamReader(stream))
                        {
                            return ReadMatrix<T>(reader);
                        }

                    case Compression.GZip:
                        using (var decompressed = new GZipStream(stream, CompressionMode.Decompress))
                        using (var reader = new StreamReader(decompressed))
                        {
                            return ReadMatrix<T>(reader);
                        }

                    default:
                        throw new NotSupportedException("Compression not supported: " + compression);
                }
            }
        }

        public static LinearAlgebra.Vector<T> ReadVector<T>(string filePath, Compression compression = Compression.Uncompressed) where T : struct, IEquatable<T>, IFormattable
        {
            using (var stream = File.OpenRead(filePath))
            {
                switch (compression)
                {
                    case Compression.Uncompressed:
                        using (var reader = new StreamReader(stream))
                        {
                            return ReadVector<T>(reader);
                        }

                    case Compression.GZip:
                        using (var decompressed = new GZipStream(stream, CompressionMode.Decompress))
                        using (var reader = new StreamReader(decompressed))
                        {
                            return ReadVector<T>(reader);
                        }

                    default:
                        throw new NotSupportedException("Compression not supported: " + compression);
                }
            }
        }

        public static Matrix<T> ReadMatrix<T>(Stream stream) where T : struct, IEquatable<T>, IFormattable
        {
            using (var reader = new StreamReader(stream))
            {
                return ReadMatrix<T>(reader);
            }
        }

        public static LinearAlgebra.Vector<T> ReadVector<T>(Stream stream) where T : struct, IEquatable<T>, IFormattable
        {
            using (var reader = new StreamReader(stream))
            {
                return ReadVector<T>(reader);
            }
        }

        public static Matrix<T> ReadMatrix<T>(TextReader reader) where T : struct, IEquatable<T>, IFormattable
        {
            bool complex, sparse;
            MatrixMarketSymmetry symmetry;
            ExpectHeader(reader, true, out complex, out sparse, out symmetry);

            var parse = CreateValueParser<T>(complex);

            var sizes = ExpectLine(reader).Split(Separators, StringSplitOptions.RemoveEmptyEntries);
            int rows = int.Parse(sizes[0]);
            int cols = int.Parse(sizes[1]);

            if (sparse)
            {
                var indexed = ReadTokenLines(reader).Select(tokens => new Tuple<int, int, T>(int.Parse(tokens[0]) - 1, int.Parse(tokens[1]) - 1, parse(2, tokens)));
                return Matrix<T>.Build.SparseOfIndexed(rows, cols, symmetry == MatrixMarketSymmetry.General ? indexed : ExpandSparse(symmetry, indexed));
            }

            var columnMajor = ReadTokenLines(reader).Select(tokens => parse(0, tokens));
            switch (symmetry)
            {
                case MatrixMarketSymmetry.General:
                {
                    return Matrix<T>.Build.DenseOfColumnMajor(rows, cols, columnMajor);
                }

                case MatrixMarketSymmetry.Symmetric:
                {
                    var m = Matrix<T>.Build.Dense(rows, cols);
                    int k = 0;
                    foreach (var slice in SliceDecreasing(rows, columnMajor))
                    {
                        var vector = LinearAlgebra.Vector<T>.Build.Dense(slice);
                        m.SetColumn(k, k, rows - k, vector);
                        m.SetRow(k, k, cols - k, vector);
                        k++;
                    }

                    return m;
                }

                case MatrixMarketSymmetry.Hermitian:
                {
                    var m = Matrix<T>.Build.Dense(rows, cols);
                    int k = 0;
                    foreach (var slice in SliceDecreasing(rows, columnMajor))
                    {
                        var vector = LinearAlgebra.Vector<T>.Build.Dense(slice);
                        m.SetColumn(k, k, rows - k, vector);
                        m.SetRow(k, k, cols - k, vector.Conjugate());
                        k++;
                    }

                    return m;
                }

                case MatrixMarketSymmetry.SkewSymmetric:
                {
                    var m = Matrix<T>.Build.Dense(rows, cols);
                    int k = 0;
                    foreach (var slice in SliceDecreasing(rows - 1, columnMajor))
                    {
                        var vector = LinearAlgebra.Vector<T>.Build.Dense(slice);
                        m.SetColumn(k, k + 1, rows - 1 - k, vector);
                        m.SetRow(k, k + 1, cols - 1 - k, vector);
                        k++;
                    }

                    return m;
                }

                default:
                    throw new NotSupportedException("Symmetry type not supported.");
            }
        }

        public static LinearAlgebra.Vector<T> ReadVector<T>(TextReader reader)
            where T : struct, IEquatable<T>, IFormattable
        {
            bool complex, sparse;
            MatrixMarketSymmetry symmetry;
            ExpectHeader(reader, false, out complex, out sparse, out symmetry);

            var parse = CreateValueParser<T>(complex);

            var sizes = ExpectLine(reader).Split(Separators, StringSplitOptions.RemoveEmptyEntries);
            int length = int.Parse(sizes[0]);

            if (sparse)
            {
                var indexedSeq = ReadTokenLines(reader).Select(tokens => new Tuple<int, T>(int.Parse(tokens[0]) - 1, parse(1, tokens)));
                return LinearAlgebra.Vector<T>.Build.SparseOfIndexed(length, indexedSeq);
            }

            return LinearAlgebra.Vector<T>.Build.Dense(ReadTokenLines(reader).Select(tokens => parse(0, tokens)).ToArray());
        }

        static void ExpectHeader(TextReader reader, bool matrix, out bool complex, out bool sparse, out MatrixMarketSymmetry symmetry)
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();
                if (line.StartsWith("%%MatrixMarket"))
                {
                    var tokens = line.ToLowerInvariant().Substring(15).Split(Separators, StringSplitOptions.RemoveEmptyEntries);
                    if (tokens.Length < 2)
                    {
                        throw new FormatException(@"Expected MatrixMarket Header with 2-4 attributes: object format [field] [symmetry]; see http://math.nist.gov/MatrixMarket/ for details.");
                    }

                    if (tokens[0] != (matrix ? "matrix" : "vector"))
                    {
                        throw new FormatException("Expected matrix content.");
                    }

                    switch (tokens[1])
                    {
                        case "array":
                            sparse = false;
                            break;
                        case "coordinate":
                            sparse = true;
                            break;
                        default:
                            throw new NotSupportedException("Format type not supported.");
                    }

                    if (tokens.Length < 3)
                    {
                        complex = false;
                    }
                    else
                    {
                        switch (tokens[2])
                        {
                            case "real":
                            case "double":
                            case "integer":
                                complex = false;
                                break;
                            case "complex":
                                complex = true;
                                break;
                            default:
                                throw new NotSupportedException("Field type not supported.");
                        }
                    }

                    if (tokens.Length < 4)
                    {
                        symmetry = MatrixMarketSymmetry.General;
                    }
                    else
                    {
                        switch (tokens[3])
                        {
                            case "general":
                                symmetry = MatrixMarketSymmetry.General;
                                break;
                            case "symmetric":
                                symmetry = MatrixMarketSymmetry.Symmetric;
                                break;
                            case "skew-symmetric":
                                symmetry = MatrixMarketSymmetry.SkewSymmetric;
                                break;
                            case "hermitian":
                                symmetry = MatrixMarketSymmetry.Hermitian;
                                break;
                            default:
                                throw new NotSupportedException("Symmetry type not supported");
                        }
                    }

                    return;
                }
            }

            throw new FormatException(@"Expected MatrixMarket Header, see http://math.nist.gov/MatrixMarket/ for details.");
        }

        static string ExpectLine(TextReader reader)
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                var trim = line.Trim();
                if (trim.Length > 0 && !trim.StartsWith("%"))
                {
                    return trim;
                }
            }

            throw new FormatException(@"End of file reached unexpectedly.");
        }

        static IEnumerable<string[]> ReadTokenLines(TextReader reader)
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                var trim = line.Trim();
                if (trim.Length > 0 && !trim.StartsWith("%"))
                {
                    yield return trim.Split(Separators, StringSplitOptions.RemoveEmptyEntries);
                }
            }
        }

        static IEnumerable<Tuple<int, int, T>> ExpandSparse<T>(MatrixMarketSymmetry symmetry, IEnumerable<Tuple<int, int, T>> indexedValues)
        {
            var map = CreateSymmetryMap<T>(symmetry);
            foreach (var x in indexedValues)
            {
                yield return x;
                if (x.Item1 != x.Item2)
                {
                    yield return new Tuple<int, int, T>(x.Item2, x.Item1, map(x.Item3));
                }
            }
        }

        static IEnumerable<T[]> SliceDecreasing<T>(int initialLength, IEnumerable<T> columnMajor)
        {
            int nextIndex = 0;
            var slice = new T[initialLength];
            foreach (var value in columnMajor)
            {
                if (nextIndex == initialLength)
                {
                    yield return slice;
                    slice = new T[--initialLength];
                    nextIndex = 0;
                }

                slice[nextIndex++] = value;
            }

            yield return slice;
        }

        static Func<int, string[], T> CreateValueParser<T>(bool sourceIsComplex)
        {
            if (typeof (T) == typeof (double))
            {
                // ignore imaginary part if source is complex
                return (offset, tokens) => (T)(object)double.Parse(tokens[offset], NumberStyles.Any, Format);
            }

            if (typeof (T) == typeof (float))
            {
                // ignore imaginary part if source is complex
                return (offset, tokens) => (T)(object)float.Parse(tokens[offset], NumberStyles.Any, Format);
            }

            if (typeof (T) == typeof (Complex))
            {
                return sourceIsComplex
                    ? ((offset, tokens) => (T)(object)new Complex(double.Parse(tokens[offset], NumberStyles.Any, Format), double.Parse(tokens[offset + 1], NumberStyles.Any, Format)))
                    : (Func<int, string[], T>)((offset, tokens) => (T)(object)new Complex(double.Parse(tokens[offset], NumberStyles.Any, Format), 0d));
            }

            if (typeof (T) == typeof (Complex32))
            {
                return sourceIsComplex
                    ? ((offset, tokens) => (T)(object)new Complex32(float.Parse(tokens[offset], NumberStyles.Any, Format), float.Parse(tokens[offset + 1], NumberStyles.Any, Format)))
                    : (Func<int, string[], T>)((offset, tokens) => (T)(object)new Complex32(float.Parse(tokens[offset], NumberStyles.Any, Format), 0f));
            }

            throw new NotSupportedException();
        }

        static Func<T, T> CreateSymmetryMap<T>(MatrixMarketSymmetry symmetry)
        {
            if (symmetry != MatrixMarketSymmetry.Hermitian)
            {
                return x => x;
            }

            if (typeof (T) == typeof (double) || typeof (T) == typeof (float))
            {
                return x => x;
            }

            if (typeof (T) == typeof (Complex))
            {
                return x => (T)(object)((Complex)(object)x).Conjugate();
            }

            if (typeof (T) == typeof (Complex32))
            {
                return x => (T)(object)((Complex32)(object)x).Conjugate();
            }

            throw new NotSupportedException();
        }
    }
}