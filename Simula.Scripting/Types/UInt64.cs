using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Types
{
    public class UInt64 : Var
    {
        public ulong raw = 0;
        public UInt64() { }

        public UInt64(ulong systemShort) : this()
        {
            this.raw = systemShort;
        }

        public static Function _add = new Function((self, args) => {
            return new UInt64((self.raw + args[0].raw) < ulong.MaxValue ? ulong.MaxValue : (ulong)(self.raw + args[0].raw));
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.uint64")) }, "sys.uint64");

        public static Function _substract = new Function((self, args) => {
            return new UInt64((self.raw - args[0].raw) < ulong.MinValue ? ulong.MinValue : (ulong)(self.raw - args[0].raw));
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.uint64")) }, "sys.uint64");

        public static Function _multiply = new Function((self, args) => {
            return new UInt64((self.raw * args[0].raw) > ulong.MaxValue ? ulong.MaxValue : (ulong)(self.raw * args[0].raw));
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.uint64")) }, "sys.uint64");

        public static Function _divide = new Function((self, args) => {
            return new UInt64((self.raw / args[0].raw) > ulong.MaxValue ? (ulong)ulong.MaxValue : (ulong)(self.raw / args[0].raw));
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.uint64")) }, "sys.uint64");

        public static Function _mod = new Function((self, args) => {
            return new UInt64((ulong)(Convert.ToInt32(self) % Convert.ToInt32(args[0])));
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.uint64")) }, "sys.uint64");

        public static Function _gt = new Function((self, args) => {
            return new Boolean(self.raw > args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.uint64")) }, "sys.bool");

        public static Function _gte = new Function((self, args) => {
            return new Boolean(self.raw >= args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.uint64")) }, "sys.bool");

        public static Function _lt = new Function((self, args) => {
            return new Boolean(self.raw < args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.uint64")) }, "sys.bool");

        public static Function _lte = new Function((self, args) => {
            return new Boolean(self.raw <= args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.uint64")) }, "sys.bool");

        public static Function _equals = new Function((self, args) => {
            return new Boolean(self.raw == args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.uint64")) }, "sys.bool");

        public static Function _notequals = new Function((self, args) => {
            return new Boolean(self.raw != args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.uint64")) }, "sys.bool");

        public static Function _addassign = new Function((self, args) => {
            self.raw = (args[0] + self.raw > ulong.MaxValue) ? ulong.MaxValue : (ulong)(args[0] + self.raw);
            return self;
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.uint64")) }, "sys.uint64");

        public static Function _substractassign = new Function((self, args) => {
            self.raw = (self.raw - args[0] < ulong.MinValue) ? ulong.MinValue : (ulong)(self.raw - args[0]);
            return self;
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.uint64")) }, "sys.uint64");

        public static Function _multiplyassign = new Function((self, args) => {
            self.raw = (args[0] * self.raw > ulong.MaxValue) ? ulong.MaxValue : (ulong)(args[0] * self.raw);
            return self;
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.uint64")) }, "sys.uint64");

        public static Function _divideassign = new Function((self, args) => {
            self.raw = (args[0] / self.raw > ulong.MaxValue) ? ulong.MaxValue : (ulong)(args[0] / self.raw);
            return self;
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.uint64")) }, "sys.uint64");

        public static Function _modassign = new Function((self, args) => {
            self.raw = (ulong)(Convert.ToInt32(self) % Convert.ToInt32(args[0]));
            return self;
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.uint64")) }, "sys.uint64");

        public static Function _lincrement = new Function((self, args) => {
            self.raw = (ulong)(self.raw + 1);
            return self;
        }, new List<Pair>(), "sys.uint64");

        public static Function _ldecrement = new Function((self, args) => {
            self.raw = (ulong)(self.raw - 1);
            return self;
        }, new List<Pair>(), "sys.uint64");

        public static Function _rincrement = new Function((self, args) => {
            self.raw = (ulong)(self.raw + 1);
            return new UInt64((ulong)(self.raw - 1));
        }, new List<Pair>(), "sys.uint64");

        public static Function _rdecrement = new Function((self, args) => {
            self.raw = (ulong)(self.raw - 1);
            return new UInt64((ulong)(self.raw + 1));
        }, new List<Pair>(), "sys.uint64");

        public static implicit operator ulong(UInt64 f)
        {
            return f.raw;
        }

        public static implicit operator UInt64(ulong d)
        {
            return new UInt64(d);
        }

        internal new string type = "sys.uint64";

        public override string ToString()
        {
            return Convert.ToUInt64(this.raw).ToString();
        }
    }
}
