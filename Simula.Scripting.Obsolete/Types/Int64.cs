using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Types
{
    public class Int64 : Var
    {
        public long raw = 0;
        public Int64() { }

        public Int64(long systemShort) : this()
        {
            this.raw = systemShort;
        }

        public static Function _add = new Function((self, args) => {
            return new Int64((self.raw + args[0].raw) < long.MaxValue ? long.MaxValue : (long)(self.raw + args[0].raw));
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int64")) }, "sys.int64");

        public static Function _substract = new Function((self, args) => {
            return new Int64((self.raw - args[0].raw) < long.MinValue ? long.MinValue : (long)(self.raw - args[0].raw));
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int64")) }, "sys.int64");

        public static Function _multiply = new Function((self, args) => {
            return new Int64((self.raw * args[0].raw) > long.MaxValue ? long.MaxValue : (long)(self.raw * args[0].raw));
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int64")) }, "sys.int64");

        public static Function _divide = new Function((self, args) => {
            return new Int64((self.raw / args[0].raw) > long.MaxValue ? (long)long.MaxValue : (long)(self.raw / args[0].raw));
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int64")) }, "sys.int64");

        public static Function _mod = new Function((self, args) => {
            return new Int64((long)(Convert.ToInt32(self) % Convert.ToInt32(args[0])));
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int64")) }, "sys.int64");

        public static Function _gt = new Function((self, args) => {
            return new Boolean(self.raw > args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int64")) }, "sys.bool");

        public static Function _gte = new Function((self, args) => {
            return new Boolean(self.raw >= args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int64")) }, "sys.bool");

        public static Function _lt = new Function((self, args) => {
            return new Boolean(self.raw < args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int64")) }, "sys.bool");

        public static Function _lte = new Function((self, args) => {
            return new Boolean(self.raw <= args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int64")) }, "sys.bool");

        public static Function _equals = new Function((self, args) => {
            return new Boolean(self.raw == args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int64")) }, "sys.bool");

        public static Function _notequals = new Function((self, args) => {
            return new Boolean(self.raw != args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int64")) }, "sys.bool");

        public static Function _addassign = new Function((self, args) => {
            self.raw = (args[0] + self.raw > long.MaxValue) ? long.MaxValue : (long)(args[0] + self.raw);
            return self;
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int64")) }, "sys.int64");

        public static Function _substractassign = new Function((self, args) => {
            self.raw = (self.raw - args[0] < long.MinValue) ? long.MinValue : (long)(self.raw - args[0]);
            return self;
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int64")) }, "sys.int64");

        public static Function _multiplyassign = new Function((self, args) => {
            self.raw = (args[0] * self.raw > long.MaxValue) ? long.MaxValue : (long)(args[0] * self.raw);
            return self;
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int64")) }, "sys.int64");

        public static Function _divideassign = new Function((self, args) => {
            self.raw = (args[0] / self.raw > long.MaxValue) ? long.MaxValue : (long)(args[0] / self.raw);
            return self;
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int64")) }, "sys.int64");

        public static Function _modassign = new Function((self, args) => {
            self.raw = (long)(Convert.ToInt32(self) % Convert.ToInt32(args[0]));
            return self;
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int64")) }, "sys.int64");

        public static Function _lincrement = new Function((self, args) => {
            self.raw = (long)(self.raw + 1);
            return self;
        }, new List<Pair>(), "sys.int64");

        public static Function _ldecrement = new Function((self, args) => {
            self.raw = (long)(self.raw - 1);
            return self;
        }, new List<Pair>(), "sys.int64");

        public static Function _rincrement = new Function((self, args) => {
            self.raw = (long)(self.raw + 1);
            return new Int64((long)(self.raw - 1));
        }, new List<Pair>(), "sys.int64");

        public static Function _rdecrement = new Function((self, args) => {
            self.raw = (long)(self.raw - 1);
            return new Int64((long)(self.raw + 1));
        }, new List<Pair>(), "sys.int64");

        public static implicit operator long(Int64 f)
        {
            return f.raw;
        }

        public static implicit operator Int64(long d)
        {
            return new Int64(d);
        }

        internal new string type = "sys.int64";

        public override string ToString()
        {
            return Convert.ToInt64(this.raw).ToString();
        }
    }
}
