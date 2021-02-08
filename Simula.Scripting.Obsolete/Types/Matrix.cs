using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Linq;
using Simula.Scripting.Contexts;

namespace Simula.Scripting.Types
{
    public unsafe abstract class Matrix : Var
    {
        public virtual int dimension { get; set; } = 0;
        public virtual int[] bounds { get; set; }
        public virtual int elementSize { get; set; } = 0;
        public virtual int total { get; set; } = 0;

        public virtual string elementType { get; set; } = "";

        public virtual void Reshape(NumericalMatrix<int> bounds) { }
        public virtual dynamic Get(NumericalMatrix<int> location) { return Null.NULL; }
        public virtual void Set(NumericalMatrix<int> location, dynamic value) { }

        public bool IsVector()
        {
            return dimension == 1;
        }

        public NumericalMatrix<int> GetLocation(int location)
        {
            if (location > this.total || location <= 0) {
                DynamicRuntime.PostExecutionError(StringTableIndex.MatrixOutOfRange);
                return new NumericalMatrix<int>();
            }

            if (this.dimension == 1)
                return new NumericalMatrix<int>(new int[1] { location });
            if (this.dimension == 2)
                return new NumericalMatrix<int>(new int[2] { ((location - 1) / this.bounds[1]) + 1, ((location - 1) % this.bounds[1]) + 1 });
            if(this.dimension > 2) {
                int prevPageCount = (location - 1) / (this.bounds[0] * this.bounds[1]);
                int pageLoc = (location - 1) % (this.bounds[0] * this.bounds[1]) + 1;
                int[] pageCoord = new int[this.dimension - 2];

                for (int i = 0; i < pageCoord.Length; i++)
                    pageCoord[i] = 1;

                for (int i = 1; i < prevPageCount + 1; i ++) {
                    bool levelUp = true;

                    for (int j = pageCoord.Length - 1; j >= 0; j --) {
                        if(levelUp) {
                            pageCoord[j]++;
                            levelUp = false;
                            if(pageCoord[j] > this.bounds[j + 2]) {
                                levelUp = true;
                                pageCoord[j] = 1;
                            }
                        }
                    }
                }

                List<int> l = new List<int>() { ((pageLoc - 1) / this.bounds[1]) + 1, ((pageLoc - 1) % this.bounds[1]) + 1 };
                l.AddRange(pageCoord);
                return new NumericalMatrix<int>(l.ToArray());

            } else {
                DynamicRuntime.PostExecutionError(StringTableIndex.MatrixOutOfRange);
                return new NumericalMatrix<int>();
            }
        }

        public static int SizeMultiplication(int[] dim)
        {
            if (dim.Length == 0) return 0;
            if (dim.Length == 1) return dim[0];
            if (dim.Length == 2) return (dim[0] * dim[1]);
            if (dim.Length == 3) return (dim[0] * dim[1] * dim[2]);
            if (dim.Length == 4) return (dim[0] * dim[1] * dim[2] * dim[3]);

            int mul = dim[0];
            for (int i = 1; i < dim.Length; i++) {
                mul *= dim[i];
            }
            
            return mul;
        }

        public static Function reshape = new Function((obj, args) => {
            obj.Reshape(args[0].ToIntegerMatrix());
            return obj;
        }, new List<Pair>() { new Pair(new String("bounds"), new String("sys.matrix")) }, "sys.matrix");

        public static Function get = new Function((obj, args) => {
            return Serializer.WrapUnmanaged(obj.Get(args[0].ToIntegerMatrix()));
        }, new List<Pair>() { new Pair(new String("position"), new String("sys.matrix")) }, "any");

        public static Function set = new Function((obj, args) => {
            return Serializer.WrapUnmanaged(obj.Set(args[0].ToIntegerMatrix(), args[1]));
        }, new List<Pair>() { new Pair(new String("position"), new String("sys.matrix")), new Pair(new String("value"), new String("any")) }, "any");

        public static Function isVector = new Function((obj, args) => {
            return new Boolean(obj.IsVector());
        }, new List<Pair>(), "sys.bool");

        internal new string type = "sys.matrix";

        public override string ToString()
        {
            return "<undifferentiated matrix>";
        }
    }

    public interface INumericalMatrix
    {
        public NumericalMatrix<int> ToIntegerMatrix();
        public NumericalMatrix<byte> ToByteMatrix();
    }

    // a numerical matrix is a matrix whose data are unmanaged base types, i.e.
    //     bool, int8(sbyte), int16(short), int32(int), int64(long)
    //     uint8(byte), uint16(ushort, char), uint32, uint64, double and float

    // a float type is currently unavailable in the script runtime.

    public unsafe class NumericalMatrix<T> : Matrix, INumericalMatrix
        where T: unmanaged
    {
        public T[] data;

        public override string elementType {
            get {
                var type = typeof(T).Name;
                switch (type) {
                    case "Byte": return "sys.uint8";
                    case "Char": return "sys.uint16";
                    case "UInt16": return "sys.uint16";
                    case "UInt32": return "sys.uint32";
                    case "UInt64": return "sys.uint64";
                    case "SByte": return "sys.int8";
                    case "Int16": return "sys.int16";
                    case "Int32": return "sys.int32";
                    case "Int64": return "sys.int64";
                    case "Double": return "sys.double";
                    default: return "null";
                }
            }
        }

        public NumericalMatrix() { this.data = new T[0]; }
        public NumericalMatrix(T[] elements)
        {
            this.data = elements;
            this.total = elements.Length;
            this.bounds = new int[1] { this.total };
            this.dimension = 1;
            this.elementSize = sizeof(T);
        }

        public NumericalMatrix(dynamic[] elements)
        {
            try {
                List<T> target = new List<T>();
                foreach (var item in elements) {
                    target.Add((T)item);
                }

                this.data = target.ToArray();
                this.total = elements.Length;
                this.bounds = new int[1] { this.total };
                this.dimension = 1;
                this.elementSize = sizeof(T);
            } catch(Exception ex) {
                this.data = new T[0];
                this.total = 0;
                this.bounds = new int[1] { 0 };
                this.dimension = 1;
                this.elementSize = 0;
                System.Windows.MessageBox.Show("at numericalMatrix: (initializer)\n  " + ex.Message);
            }
        }

        public NumericalMatrix<int> ToIntegerMatrix()
        {
            NumericalMatrix<int> matrix;
            try {
                List<int> target = new List<int>();
                foreach (var item in this.data) {
                    target.Add(Convert.ToInt32(item));
                }

                matrix = new NumericalMatrix<int>(target.ToArray());
                return matrix;
            } catch (Exception ex) {
                System.Windows.MessageBox.Show("at numericalMatrix: (toIntegerMatrix)\n  " + ex.Message);
                return new NumericalMatrix<int>();
            }
        }

        public NumericalMatrix<byte> ToByteMatrix()
        {
            NumericalMatrix<byte> matrix;
            try {
                List<byte> target = new List<byte>();
                foreach (var item in this.data) {
                    target.Add(Convert.ToByte(item));
                }

                matrix = new NumericalMatrix<byte>(target.ToArray());
                return matrix;
            } catch (Exception ex) {
                System.Windows.MessageBox.Show("at numericalMatrix: (toByteMatrix)\n  " + ex.Message);
                return new NumericalMatrix<byte>();
            }
        }

        public override void Reshape(NumericalMatrix<int> vector)
        {
            if (vector.IsVector())
                if (Matrix.SizeMultiplication(vector.data) == this.total) {
                    this.dimension = vector.total;
                    if (vector.total == 2)
                        if (vector.data[0] == 1) this.dimension = 1;
                    this.bounds = vector.data;
                }
        }

        public override void Set(NumericalMatrix<int> position, dynamic obj)
        {
            dynamic raw = obj;
            if (Serializer.IsUnmanaged(obj)) raw = obj.raw;
            
            if(raw is T) {
                int[] dims = position.data;
                if (this.dimension == dims.Length) {

                    // get the pages of the matrix, they are the index without the 2
                    // starting elements. if the dims has lengths more than 2.

                    if (dims.Length > 2) {
                        int page = 0;
                        for (int i = 2; i < dims.Length; i++) {
                            int mul = 1;
                            for (int j = i + 1; j < dims.Length; j++) {
                                mul *= this.bounds[j];
                            }

                            page += dims[i] * mul;
                        }

                        int pageCount = 1;
                        for (int j = 2; j < dims.Length; j++) {
                            pageCount *= this.bounds[j];
                        }

                        int loc = pageCount * page + this.bounds[1] * (dims[0] - 1) + (dims[1] - 1);
                        this.data[loc] = raw;

                    } else if (dims.Length == 2) {
                        int loc = this.bounds[1] * (dims[0] - 1) + (dims[1] - 1);
                        this.data[loc] = raw;

                    } else if (dims.Length == 1) {
                        int loc = dims[0] - 1;
                        this.data[loc] = raw;

                    } else {
                        System.Windows.MessageBox.Show("at numericMatrix: (get)\n  zero index vector");
                    }
                }
            }
        }

        public override dynamic Get(NumericalMatrix<int> position)
        {
            int[] dims = position.data;
            if (this.dimension == dims.Length) {

                // get the pages of the matrix, they are the index without the 2
                // starting elements. if the dims has lengths more than 2.

                if (dims.Length > 2) {
                    int page = 0;
                    for (int i = 2; i < dims.Length; i++) {
                        int mul = 1;
                        for (int j = i + 1; j < dims.Length; j++) {
                            mul *= this.bounds[j];
                        }

                        page += dims[i] * mul;
                    }

                    int pageCount = 1;
                    for (int j = 2; j < dims.Length; j++) {
                        pageCount *= this.bounds[j];
                    }

                    int loc = pageCount * page + this.bounds[1] * (dims[0] - 1) + (dims[1] - 1);
                    return Serializer.WrapUnmanaged(this.data[loc]);

                } else if (dims.Length == 2) {
                    int loc = this.bounds[1] * (dims[0] - 1) + (dims[1] - 1);
                    return Serializer.WrapUnmanaged(this.data[loc]);

                } else if (dims.Length == 1) {
                    int loc = dims[0] - 1;
                    return Serializer.WrapUnmanaged(this.data[loc]);

                } else {
                    System.Windows.MessageBox.Show("at numericMatrix: (set)\n  zero index vector");
                    return Null.NULL;
                }

            } else {
                System.Windows.MessageBox.Show("at numericMatrix: (set)\n  index overflow");
                return Null.NULL;
            }
        }

        public override string ToString()
        {
            string eval = "numeric matrix [" + this.bounds.ToList().JoinString("x") + "]\n";
            if (this.bounds.Count() == 1) eval = "numeric matrix [1x" + this.bounds[0] + "]\n";

            if (this.bounds.Count() <= 2) {
                int count = 0;

                for (int y = 1; y <= ((this.bounds.Count() == 1) ? 1 : this.bounds[0]); y++) {
                    string line = "\n";
                    for (int x = 1; x <= ((this.bounds.Count() == 1) ? this.bounds[0] : this.bounds[1]); x++) {
                        line += this.data[count].ToString() + "\t";
                        count++;
                    }
                    eval += line;
                }
            } else return eval + "\n" + Resources.Loc(StringTableIndex.UnsupportMatrixExpression);

            return eval;
        }
    }

    public class ObjectMatrix : Matrix
    {
        public GCHandle[] data;
        public ObjectMatrix(dynamic[] elements)
        {

        }
    }
}
