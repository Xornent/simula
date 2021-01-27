using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Types
{
    public class Int16 : Var
    {
        public short raw = 0;
        public Int16() { }

        public Int16(short systemShort) : this()
        {
            this.raw = systemShort;
        }

        public static Function _add = new Function((self, args) => {
            return new Int16((self.raw + args[0].raw) < short.MaxValue ? short.MaxValue : (short)(self.raw + args[0].raw));
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int16")) }, "sys.int16");

        public static Function _substract = new Function((self, args) => {
            return new Int16((self.raw - args[0].raw) < short.MinValue ? short.MinValue : (short)(self.raw - args[0].raw));
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int16")) }, "sys.int16");

        public static Function _multiply = new Function((self, args) => {
            return new Int16((self.raw * args[0].raw) > short.MaxValue ? short.MaxValue : (short)(self.raw * args[0].raw));
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int16")) }, "sys.int16");

        public static Function _divide = new Function((self, args) => {
            return new Int16((self.raw / args[0].raw) > short.MaxValue ? (short)short.MaxValue : (short)(self.raw / args[0].raw));
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int16")) }, "sys.int16");

        public static Function _mod = new Function((self, args) => {
            return new Int16((short)(Convert.ToInt32(self) % Convert.ToInt32(args[0])));
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int16")) }, "sys.int16");

        public static Function _gt = new Function((self, args) => {
            return new Boolean(self.raw > args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int16")) }, "sys.bool");

        public static Function _gte = new Function((self, args) => {
            return new Boolean(self.raw >= args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int16")) }, "sys.bool");

        public static Function _lt = new Function((self, args) => {
            return new Boolean(self.raw < args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int16")) }, "sys.bool");

        public static Function _lte = new Function((self, args) => {
            return new Boolean(self.raw <= args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int16")) }, "sys.bool");

        public static Function _equals = new Function((self, args) => {
            return new Boolean(self.raw == args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int16")) }, "sys.bool");

        public static Function _notequals = new Function((self, args) => {
            return new Boolean(self.raw != args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int16")) }, "sys.bool");

        public static Function _addassign = new Function((self, args) => {
            self.raw = (args[0] + self.raw > short.MaxValue) ? short.MaxValue : (short)(args[0] + self.raw);
            return self;
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int16")) }, "sys.int16");

        public static Function _substractassign = new Function((self, args) => {
            self.raw = (self.raw - args[0] < short.MinValue) ? short.MinValue : (short)(self.raw - args[0]);
            return self;
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int16")) }, "sys.int16");

        public static Function _multiplyassign = new Function((self, args) => {
            self.raw = (args[0] * self.raw > short.MaxValue) ? short.MaxValue : (short)(args[0] * self.raw);
            return self;
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int16")) }, "sys.int16");

        public static Function _divideassign = new Function((self, args) => {
            self.raw = (args[0] / self.raw > short.MaxValue) ? short.MaxValue : (short)(args[0] / self.raw);
            return self;
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int16")) }, "sys.int16");

        public static Function _modassign = new Function((self, args) => {
            self.raw = (short)(Convert.ToInt32(self) % Convert.ToInt32(args[0]));
            return self;
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int16")) }, "sys.int16");

        public static Function _lincrement = new Function((self, args) => {
            self.raw = (short)(self.raw + 1);
            return self;
        }, new List<Pair>(), "sys.int16");

        public static Function _ldecrement = new Function((self, args) => {
            self.raw = (short)(self.raw - 1);
            return self;
        }, new List<Pair>(), "sys.int16");

        public static Function _rincrement = new Function((self, args) => {
            self.raw = (short)(self.raw + 1);
            return new Int16((short)(self.raw - 1));
        }, new List<Pair>(), "sys.int16");

        public static Function _rdecrement = new Function((self, args) => {
            self.raw = (short)(self.raw - 1);
            return new Int16((short)(self.raw + 1));
        }, new List<Pair>(), "sys.int16");

        public static implicit operator short(Int16 f)
        {
            return f.raw;
        }

        public static implicit operator Int16(short d)
        {
            return new Int16(d);
        }

        internal new string type = "sys.int16";

        public override string ToString()
        {
            return Convert.ToInt16(this.raw).ToString();
        }
    }
}
