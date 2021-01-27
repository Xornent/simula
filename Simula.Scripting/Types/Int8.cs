using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Types
{
    public class Int8 : Var
    {
        public sbyte raw = 0;
        public Int8() { }

        public Int8(sbyte systemShort) : this()
        {
            this.raw = systemShort;
        }

        public static Function _add = new Function((self, args) => {
            return new Int8((self.raw + args[0].raw) < sbyte.MaxValue ? sbyte.MaxValue : (sbyte)(self.raw + args[0].raw));
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int8")) }, "sys.int8");

        public static Function _substract = new Function((self, args) => {
            return new Int8((self.raw - args[0].raw) < sbyte.MinValue ? sbyte.MinValue : (sbyte)(self.raw - args[0].raw));
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int8")) }, "sys.int8");

        public static Function _multiply = new Function((self, args) => {
            return new Int8((self.raw * args[0].raw) > sbyte.MaxValue ? sbyte.MaxValue : (sbyte)(self.raw * args[0].raw));
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int8")) }, "sys.int8");

        public static Function _divide = new Function((self, args) => {
            return new Int8((self.raw / args[0].raw) > sbyte.MaxValue ? (sbyte)sbyte.MaxValue : (sbyte)(self.raw / args[0].raw));
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int8")) }, "sys.int8");

        public static Function _mod = new Function((self, args) => {
            return new Int8((sbyte)(Convert.ToInt32(self) % Convert.ToInt32(args[0])));
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int8")) }, "sys.int8");

        public static Function _gt = new Function((self, args) => {
            return new Boolean(self.raw > args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int8")) }, "sys.bool");

        public static Function _gte = new Function((self, args) => {
            return new Boolean(self.raw >= args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int8")) }, "sys.bool");

        public static Function _lt = new Function((self, args) => {
            return new Boolean(self.raw < args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int8")) }, "sys.bool");

        public static Function _lte = new Function((self, args) => {
            return new Boolean(self.raw <= args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int8")) }, "sys.bool");

        public static Function _equals = new Function((self, args) => {
            return new Boolean(self.raw == args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int8")) }, "sys.bool");

        public static Function _notequals = new Function((self, args) => {
            return new Boolean(self.raw != args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int8")) }, "sys.bool");

        public static Function _addassign = new Function((self, args) => {
            self.raw = (args[0] + self.raw > sbyte.MaxValue) ? sbyte.MaxValue : (sbyte)(args[0] + self.raw);
            return self;
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int8")) }, "sys.int8");

        public static Function _substractassign = new Function((self, args) => {
            self.raw = (self.raw - args[0] < sbyte.MinValue) ? sbyte.MinValue : (sbyte)(self.raw - args[0]);
            return self;
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int8")) }, "sys.int8");

        public static Function _multiplyassign = new Function((self, args) => {
            self.raw = (args[0] * self.raw > sbyte.MaxValue) ? sbyte.MaxValue : (sbyte)(args[0] * self.raw);
            return self;
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int8")) }, "sys.int8");

        public static Function _divideassign = new Function((self, args) => {
            self.raw = (args[0] / self.raw > sbyte.MaxValue) ? sbyte.MaxValue : (sbyte)(args[0] / self.raw);
            return self;
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int8")) }, "sys.int8");

        public static Function _modassign = new Function((self, args) => {
            self.raw = (sbyte)(Convert.ToInt32(self) % Convert.ToInt32(args[0]));
            return self;
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int8")) }, "sys.int8");

        public static Function _lincrement = new Function((self, args) => {
            self.raw = (sbyte)(self.raw + 1);
            return self;
        }, new List<Pair>(), "sys.int8");

        public static Function _ldecrement = new Function((self, args) => {
            self.raw = (sbyte)(self.raw - 1);
            return self;
        }, new List<Pair>(), "sys.int8");

        public static Function _rincrement = new Function((self, args) => {
            self.raw = (sbyte)(self.raw + 1);
            return new Int8((sbyte)(self.raw - 1));
        }, new List<Pair>(), "sys.int8");

        public static Function _rdecrement = new Function((self, args) => {
            self.raw = (sbyte)(self.raw - 1);
            return new Int8((sbyte)(self.raw + 1));
        }, new List<Pair>(), "sys.int8");

        public static implicit operator sbyte(Int8 f)
        {
            return f.raw;
        }

        public static implicit operator Int8(sbyte d)
        {
            return new Int8(d);
        }

        internal new string type = "sys.int8";

        public override string ToString()
        {
            return Convert.ToSByte(this.raw).ToString();
        }
    }
}
