using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Types
{
    public class Int32 : Var
    {
        public int raw = 0;
        public Int32() { }

        public Int32(int systemShort) : this()
        {
            this.raw = systemShort;
        }

        public static Function _add = new Function((self, args) => {
            return new Int32((self.raw + args[0].raw) < int.MaxValue ? int.MaxValue : (int)(self.raw + args[0].raw));
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int32")) }, "sys.int32");

        public static Function _substract = new Function((self, args) => {
            return new Int32((self.raw - args[0].raw) < int.MinValue ? int.MinValue : (int)(self.raw - args[0].raw));
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int32")) }, "sys.int32");

        public static Function _multiply = new Function((self, args) => {
            return new Int32((self.raw * args[0].raw) > int.MaxValue ? int.MaxValue : (int)(self.raw * args[0].raw));
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int32")) }, "sys.int32");

        public static Function _divide = new Function((self, args) => {
            return new Int32((self.raw / args[0].raw) > int.MaxValue ? (int)int.MaxValue : (int)(self.raw / args[0].raw));
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int32")) }, "sys.int32");

        public static Function _mod = new Function((self, args) => {
            return new Int32((int)(Convert.ToInt32(self) % Convert.ToInt32(args[0])));
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int32")) }, "sys.int32");

        public static Function _gt = new Function((self, args) => {
            return new Boolean(self.raw > args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int32")) }, "sys.bool");

        public static Function _gte = new Function((self, args) => {
            return new Boolean(self.raw >= args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int32")) }, "sys.bool");

        public static Function _lt = new Function((self, args) => {
            return new Boolean(self.raw < args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int32")) }, "sys.bool");

        public static Function _lte = new Function((self, args) => {
            return new Boolean(self.raw <= args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int32")) }, "sys.bool");

        public static Function _equals = new Function((self, args) => {
            return new Boolean(self.raw == args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int32")) }, "sys.bool");

        public static Function _notequals = new Function((self, args) => {
            return new Boolean(self.raw != args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int32")) }, "sys.bool");

        public static Function _addassign = new Function((self, args) => {
            self.raw = (args[0] + self.raw > int.MaxValue) ? int.MaxValue : (int)(args[0] + self.raw);
            return self;
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int32")) }, "sys.int32");

        public static Function _substractassign = new Function((self, args) => {
            self.raw = (self.raw - args[0] < int.MinValue) ? int.MinValue : (int)(self.raw - args[0]);
            return self;
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int32")) }, "sys.int32");

        public static Function _multiplyassign = new Function((self, args) => {
            self.raw = (args[0] * self.raw > int.MaxValue) ? int.MaxValue : (int)(args[0] * self.raw);
            return self;
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int32")) }, "sys.int32");

        public static Function _divideassign = new Function((self, args) => {
            self.raw = (args[0] / self.raw > int.MaxValue) ? int.MaxValue : (int)(args[0] / self.raw);
            return self;
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int32")) }, "sys.int32");

        public static Function _modassign = new Function((self, args) => {
            self.raw = (int)(Convert.ToInt32(self) % Convert.ToInt32(args[0]));
            return self;
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int32")) }, "sys.int32");

        public static Function _lincrement = new Function((self, args) => {
            self.raw = (int)(self.raw + 1);
            return self;
        }, new List<Pair>(), "sys.int32");

        public static Function _ldecrement = new Function((self, args) => {
            self.raw = (int)(self.raw - 1);
            return self;
        }, new List<Pair>(), "sys.int32");

        public static Function _rincrement = new Function((self, args) => {
            self.raw = (int)(self.raw + 1);
            return new Int32((int)(self.raw - 1));
        }, new List<Pair>(), "sys.int32");

        public static Function _rdecrement = new Function((self, args) => {
            self.raw = (int)(self.raw - 1);
            return new Int32((int)(self.raw + 1));
        }, new List<Pair>(), "sys.int32");

        public static implicit operator int(Int32 f)
        {
            return f.raw;
        }

        public static implicit operator Int32(int d)
        {
            return new Int32(d);
        }

        internal new string type = "sys.int32";

        public override string ToString()
        {
            return Convert.ToInt32(this.raw).ToString();
        }
    }
}
