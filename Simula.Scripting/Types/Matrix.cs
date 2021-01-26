using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace Simula.Scripting.Types
{
    public unsafe class Matrix : Var
    {
        public int dimension = 0;
        public int[] bounds;
        public int elementSize = 0;
        public int total = 0;

        // several common basic value type is created for optimization. however,
        // matrixes can also contains classes, and that is achieved by locking all
        // the elements in memory, and take its GCHandle.

        private bool[] boolData;
        private double[] doubleData;
        private byte[] byteData;
        private char[] charData;
        private GCHandle[] handles;

        private string elementType;
        private MatrixType matrixType;

        enum MatrixType
        {
            Boolean,
            Double,
            Byte,
            Char,
            Object
        }

        // the four fields is declared here, but in the runtime, only one of them will
        // be available according to the elementType. whether it is 'sys.float', 'sys.char'
        // 'sys.byte', 'sys.bool' or 'object'

        // default matrix is a double matrix 1x0.

        public Matrix()
        {
            this.matrixType = MatrixType.Double;
            this.doubleData = new double[0];
            this.elementSize = 8;
            this.dimension = 1;
            this.bounds = new int[] { 0 };
        }

        public Matrix(double[] dbarray)
        {
            this.matrixType = MatrixType.Double;
            this.doubleData = dbarray;
            this.elementSize = 8;
            this.dimension = 1;
            this.bounds = new int[] { dbarray.Length };
            this.total = dbarray.Length;
        }

        public Matrix(bool[] barray)
        {
            this.matrixType = MatrixType.Boolean;
            this.boolData = barray;
            this.elementSize = 1;
            this.dimension = 1;
            this.bounds = new int[] { barray.Length };
            this.total = barray.Length;
        }

        public Matrix(byte[] byarray)
        {
            this.matrixType = MatrixType.Byte;
            this.byteData = byarray;
            this.elementSize = 1;
            this.dimension = 1;
            this.bounds = new int[] { byarray.Length };
            this.total = byarray.Length;
        }

        public Matrix(char[] carray)
        {
            this.matrixType = MatrixType.Char;
            this.charData = carray;
            this.elementSize = 2;
            this.dimension = 1;
            this.bounds = new int[] { carray.Length };
            this.total = carray.Length;
        }

        public Matrix(string str) : this(str.ToCharArray()) { }

        public Matrix(dynamic[] objs)
        {
            if (objs.Length > 0) {
                bool uniform = true;
                Type t = objs[0].GetType();
                foreach (var item in objs) {
                    if (item.GetType() != t) uniform = false;
                }

                if (!uniform) {
                    this.matrixType = MatrixType.Object;
                    this.handles = new GCHandle[objs.Length];
                    for (int i = 0; i < objs.Length; i++) {
                        handles[i] = GCHandle.Alloc(objs[i], GCHandleType.Normal);
                    }

                    this.elementSize = 4;
                    this.dimension = 1;
                    this.bounds = new int[] { objs.Length };
                    this.total = objs.Length;

                } else {

                    // the uniform test assures that all of the matrix's element is of the same type,
                    // and because the storage is different for every type, we had to do these works separately.
                    // this significantly increases the code length.

                    if (objs[0] is Boolean) {
                        bool[] arr = new bool[objs.Length];
                        int i = 0;
                        foreach (var item in objs) {
                            arr[i] = item;
                            i++;
                        }

                        this.matrixType = MatrixType.Boolean;
                        this.boolData = arr;
                        this.elementSize = 1;
                        this.dimension = 1;
                        this.bounds = new int[] { arr.Length };
                        this.total = arr.Length;

                    } else if (objs[0] is Float) {
                        double[] arr = new double[objs.Length];
                        int i = 0;
                        foreach (var item in objs) {
                            arr[i] = item;
                            i++;
                        }

                        this.matrixType = MatrixType.Double;
                        this.doubleData = arr;
                        this.elementSize = 1;
                        this.dimension = 1;
                        this.bounds = new int[] { arr.Length };
                        this.total = arr.Length;

                    } else if (objs[0] is Byte) {
                        byte[] arr = new byte[objs.Length];
                        int i = 0;
                        foreach (var item in objs) {
                            arr[i] = item;
                            i++;
                        }

                        this.matrixType = MatrixType.Byte;
                        this.byteData = arr;
                        this.elementSize = 1;
                        this.dimension = 1;
                        this.bounds = new int[] { arr.Length };
                        this.total = arr.Length;

                    } else if (objs[0] is Char) {
                        char[] arr = new char[objs.Length];
                        int i = 0;
                        foreach (var item in objs) {
                            arr[i] = item;
                            i++;
                        }

                        this.matrixType = MatrixType.Char;
                        this.charData = arr;
                        this.elementSize = 1;
                        this.dimension = 1;
                        this.bounds = new int[] { arr.Length };
                        this.total = arr.Length;
                    }
                }
            }
        }

        public static dynamic Reshape(dynamic matrix, dynamic[] args)
        {
            if(IsNumeral(args[0], new dynamic[] { }) && IsVector(args[0], new dynamic[] { })) {
                int[] dims = IntegralVector(args[0]);
                if(matrix.total == SizeMultiplication(dims)) {
                    matrix.dimension = dims.Length;
                    matrix.bounds = dims;
                }
            }

            return matrix;
        }

        public static dynamic Get(dynamic matrix, dynamic[] args)
        {
            if (IsNumeral(args[0], new dynamic[] { }) && IsVector(args[0], new dynamic[] { })) {
                int[] dims = IntegralVector(args[0]);
                if (matrix.dimension == dims.Length) {

                    // get the pages of the matrix, they are the index without the 2
                    // starting elements. if the dims has lengths more than 2.

                    if (dims.Length > 2) {
                        int page = 0;
                        for (int i = 2; i < dims.Length; i++) {
                            int mul = 1;
                            for (int j = i + 1; j < dims.Length; j++) {
                                mul *= matrix.bounds[j];
                            }

                            page += dims[i] * mul;
                        }

                        int pageCount = 1;
                        for (int j = 2; j < dims.Length; j++) {
                            pageCount *= matrix.bounds[j];
                        }

                        int loc = pageCount * page + matrix.bounds[1] * (dims[0] - 1) + (dims[1] - 1);
                        switch (matrix.matrixType) {
                            case MatrixType.Boolean:
                                return new Boolean(matrix.boolData[loc]);
                            case MatrixType.Double:
                                return new Float(matrix.doubleData[loc]);
                            case MatrixType.Byte:
                                return new Byte(matrix.byteData[loc]);
                            case MatrixType.Char:
                                return new Char(matrix.charData[loc]);
                            case MatrixType.Object:
                                return matrix.handles[loc].Target;
                            default:
                                return Null.NULL;
                        }

                    } else if (dims.Length == 2) {
                        int loc = matrix.bounds[1] * (dims[0] - 1) + (dims[1] - 1);
                        switch (matrix.matrixType) {
                            case MatrixType.Boolean:
                                return new Boolean(matrix.boolData[loc]);
                            case MatrixType.Double:
                                return new Float(matrix.doubleData[loc]);
                            case MatrixType.Byte:
                                return new Byte(matrix.byteData[loc]);
                            case MatrixType.Char:
                                return new Char(matrix.charData[loc]);
                            case MatrixType.Object:
                                return matrix.handles[loc].Target;
                            default:
                                return Null.NULL;
                        }
                    } else if (dims.Length == 1) {
                        int loc = dims[0] - 1;
                        switch (matrix.matrixType) {
                            case MatrixType.Boolean:
                                return new Boolean(matrix.boolData[loc]);
                            case MatrixType.Double:
                                return new Float(matrix.doubleData[loc]);
                            case MatrixType.Byte:
                                return new Byte(matrix.byteData[loc]);
                            case MatrixType.Char:
                                return new Char(matrix.charData[loc]);
                            case MatrixType.Object:
                                return matrix.handles[loc].Target;
                            default:
                                return Null.NULL;
                        }
                    } else return Null.NULL;
                }
            }

            return matrix;
        }

        public static dynamic Set(dynamic matrix, dynamic[] args)
        {
            if (IsNumeral(args[0], new dynamic[] { }) && IsVector(args[0], new dynamic[] { })) {
                int[] dims = IntegralVector(args[0]);
                if (matrix.dimension == dims.Length) {

                    // get the pages of the matrix, they are the index without the 2
                    // starting elements. if the dims has lengths more than 2.

                    if (dims.Length > 2) {
                        int page = 0;
                        for (int i = 2; i < dims.Length; i++) {
                            int mul = 1;
                            for (int j = i + 1; j < dims.Length; j++) {
                                mul *= matrix.bounds[j];
                            }

                            page += dims[i] * mul;
                        }

                        int pageCount = 1;
                        for (int j = 2; j < dims.Length; j++) {
                            pageCount *= matrix.bounds[j];
                        }

                        int loc = pageCount * page + matrix.bounds[1] * (dims[0] - 1) + (dims[1] - 1);
                        switch (matrix.matrixType) {
                            case MatrixType.Boolean:
                                matrix.boolData[loc] = args[0];
                                return args[0];
                            case MatrixType.Double:
                                matrix.doubleData[loc] = args[0];
                                return args[0];
                            case MatrixType.Byte:
                                matrix.byteData[loc] = args[0];
                                return args[0];
                            case MatrixType.Char:
                                matrix.charData[loc] = args[0];
                                return args[0];
                            case MatrixType.Object:
                                matrix.handles[loc].Free();
                                matrix.handles[loc] = GCHandle.Alloc(args[0]);
                                return args[0];
                            default:
                                return Null.NULL;
                        }

                    } else {
                        int loc = matrix.bounds[1] * (dims[0] - 1) + (dims[1] - 1);
                        switch (matrix.matrixType) {
                            case MatrixType.Boolean:
                                matrix.boolData[loc] = args[0];
                                return args[0];
                            case MatrixType.Double:
                                matrix.doubleData[loc] = args[0];
                                return args[0];
                            case MatrixType.Byte:
                                matrix.byteData[loc] = args[0];
                                return args[0];
                            case MatrixType.Char:
                                matrix.charData[loc] = args[0];
                                return args[0];
                            case MatrixType.Object:
                                matrix.handles[loc].Free();
                                matrix.handles[loc] = GCHandle.Alloc(args[0]);
                                return args[0];
                            default:
                                return Null.NULL;
                        }
                    }
                }
            }

            return matrix;
        }

        public static dynamic IsNumeral(dynamic matrix, dynamic[] args)
        {
            switch (matrix.matrixType) {
                case MatrixType.Boolean:
                    return true;
                case MatrixType.Double:
                    return true;
                case MatrixType.Byte:
                    return true;
                case MatrixType.Char:
                    return true;
                case MatrixType.Object:
                    return false;
                default:
                    return false;
            }
        }

        public static dynamic IsVector(dynamic matrix, dynamic[] args)
        {
            return matrix.dimension == 1;
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

        public static int[] IntegralVector(Matrix mtx)
        {
            int[] result = new int[mtx.total];

            int i = 0;
            switch (mtx.matrixType) {
                case MatrixType.Boolean:
                    foreach (var item in mtx.boolData) {
                        result[i] = item ? 1 : 0;
                        i++;
                    }
                    break;
                case MatrixType.Double:
                    foreach (var item in mtx.doubleData) {
                        result[i] = Convert.ToInt32(item);
                        i++;
                    }
                    break;
                case MatrixType.Byte:
                    foreach (var item in mtx.byteData) {
                        result[i] = item;
                        i++;
                    }
                    break;
                case MatrixType.Char:
                    foreach (var item in mtx.charData) {
                        result[i] = item;
                        i++;
                    }
                    break;
                case MatrixType.Object:
                    result = new int[0];
                    break;
                default:
                    result = new int[0];
                    break;
            }

            return result;
        }

        public static Function reshape = new Function(Reshape, new List<Pair>() { 
            new Pair( new String("bounds"), new String("sys.matrix")) }, "sys.matrix");
        public static Function get = new Function(Get, new List<Pair>() {
            new Pair( new String("bounds"), new String("sys.matrix")) }, "sys.matrix");
        public static Function set = new Function(Set, new List<Pair>() {
            new Pair( new String("bounds"), new String("sys.matrix")) }, "sys.matrix");
        public static Function isVector = new Function((self, args) => {
            return new Boolean(IsVector(self, args));
        }, new List<Pair>(), "sys.bool");
        public static Function isNumeral = new Function((self, args) => {
            return new Boolean(IsNumeral(self, args));
        }, new List<Pair>(), "sys.bool");

        internal new string type = "sys.matrix";

        public override string ToString()
        {
            if (this.total > 10000) return "数据量过多";
            return "<sys.matrix>";
        }
    }
}
