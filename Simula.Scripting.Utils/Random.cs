using System;
using System.Collections.Generic;
using System.Text;
using Simula.Scripting.Dom;

namespace Simula.Scripting.Utils
{
    public static class Random
    {
        public static System.Random rand;

        [FunctionExport("randomFloat", "min:sys.double|max:sys.double@sys.double", "util",
            "在两个浮点数型的上界 max 和下界 min 之间生成浮点数类型的伪随机数")]
        public static Func<dynamic, dynamic[], dynamic> randomFloat = (self, args) => {
            rand = new System.Random(DateTime.Now.Millisecond);
            return new Types.Double((rand.NextDouble() * (args[1] - args[0])) + args[0]);
        };

        [FunctionExport("random", "min:sys.double|max:sys.double@sys.double", "util",
            "在两个浮点数型的上界 max 和下界 min 之间生成整数伪随机数")]
        public static Func<dynamic, dynamic[], dynamic> random = (self, args) => {
            rand = new System.Random(DateTime.Now.Millisecond);
            return new Types.Double((int)(rand.NextDouble() * (args[1] - args[0])) + (int)args[0]);
        };

        [FunctionExport("randomSequence", "min:sys.int|max:sys.double|length:sys.double@sys.array", "util",
            "在两个浮点数型的上界 max 和下界 min 之间生成一个序列，内含 length 个整数类型的伪随机数")]
        public static Func<dynamic, dynamic[], dynamic> randomSequence = (self, args) => {
            rand = new System.Random(DateTime.Now.Millisecond);
            Types.Double[] ints = new Types.Double[(int)args[2]];
            for (int j = 0; j < (int)args[2]; j++) {
                ints[j] = new Types.Double((int)(rand.NextDouble() * (args[1] - args[0])) + (int)args[0]);
            }
            return new Types.NumericalMatrix<double>(ints);
        };

        [FunctionExport("randomSequenceFloat", "min:sys.double|max:sys.double@sys.array", "util",
            "在两个浮点数型的上界 max 和下界 min 之间生成一个序列，内含 length 个浮点数类型的伪随机数")]
        public static Func<dynamic, dynamic[], dynamic> randomSequenceFloat = (self, args) => {
            rand = new System.Random(DateTime.Now.Millisecond);
            Types.Double[] ints = new Types.Double[(int)args[2]];
            for (int j = 0; j < (int)args[2]; j++) {
                ints[j] = new Types.Double(rand.NextDouble() * (args[1] - args[0]) + args[0]);
            }
            return new Types.NumericalMatrix<double>(ints);
        };

        [FunctionExport("randomVectorSequence", "min:sys.double|max:sys.double|length:sys.double|dimension:sys.double@sys.array", "util",
            "在两个浮点数型的上界 max 和下界 min 之间生成一个序列长为 length，序列的元素是 dimension 长的行，内含 length * dimension 个整数类型的伪随机数")]
        public static Func<dynamic, dynamic[], dynamic> randomVectorSequence = (self, args) => {
            rand = new System.Random(DateTime.Now.Millisecond);
            Types.Array[] seq = new Types.Array[(int)args[2]];
            for (int j = 0; j < (int)args[2]; j++) {
                Types.Double[] vec = new Types.Double[(int)args[3]];
                for (int i = 0; i < (int)args[3]; i++) {
                    vec[i] = new Types.Double((int)(rand.NextDouble() * (args[1] - args[0])) + (int)args[0]);
                }
                seq[j] = new Types.Array(vec);
            }
            return new Types.Array(seq);
        };

        [FunctionExport("randomVectorSequenceFloat", "min:sys.double|max:sys.double|length:sys.double|dimension:sys.double@sys.array", "util",
            "在两个浮点数型的上界 max 和下界 min 之间生成一个序列长为 length，序列的元素是 dimension 长的行，内含 length * dimension 个浮点数类型的伪随机数")]
        public static Func<dynamic, dynamic[], dynamic> randomVectorSequenceFloat = (self, args) => {
            rand = new System.Random(DateTime.Now.Millisecond);
            Types.Array[] seq = new Types.Array[(int)args[2]];
            for (int j = 0; j < (int)args[2]; j++) {
                Types.Double[] vec = new Types.Double[(int)args[3]];
                for (int i = 0; i < (int)args[3]; i++) {
                    vec[i] = new Types.Double((rand.NextDouble() * (args[1] - args[0])) + args[0]);
                }
                seq[j] = new Types.Array(vec);
            }
            return new Types.Array(seq);
        };
    }
}
