using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Types
{
    public class UInt32 : Var
    {
        public uint raw = 0;
        public UInt32() { }

        public UInt32(uint systemShort) : this()
        {
            this.raw = systemShort;
        }

        public static Function _add = new Function((self, args) => {
            return new UInt32((self.raw + args[0].raw) < uint.MaxValue ? uint.MaxValue : (uint)(self.raw + args[0].raw));
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.uint32")) }, "sys.uint32");

        public static Function _substract = new Function((self, args) => {
            return new UInt32((self.raw - args[0].raw) < uint.MinValue ? uint.MinValue : (uint)(self.raw - args[0].raw));
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.uint32")) }, "sys.uint32");

        public static Function _multiply = new Function((self, args) => {
            return new UInt32((self.raw * args[0].raw) > uint.MaxValue ? uint.MaxValue : (uint)(self.raw * args[0].raw));
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.uint32")) }, "sys.uint32");

        public static Function _divide = new Function((self, args) => {
            return new UInt32((self.raw / args[0].raw) > uint.MaxValue ? (uint)uint.MaxValue : (uint)(self.raw / args[0].raw));
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.uint32")) }, "sys.uint32");

        public static Function _mod = new Function((self, args) => {
            return new UInt32((uint)(Convert.ToInt32(self) % Convert.ToInt32(args[0])));
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.uint32")) }, "sys.uint32");

        public static Function _gt = new Function((self, args) => {
            return new Boolean(self.raw > args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.uint32")) }, "sys.bool");

        public static Function _gte = new Function((self, args) => {
            return new Boolean(self.raw >= args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.uint32")) }, "sys.bool");

        public static Function _lt = new Function((self, args) => {
            return new Boolean(self.raw < args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.uint32")) }, "sys.bool");

        public static Function _lte = new Function((self, args) => {
            return new Boolean(self.raw <= args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.uint32")) }, "sys.bool");

        public static Function _equals = new Function((self, args) => {
            return new Boolean(self.raw == args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.uint32")) }, "sys.bool");

        public static Function _notequals = new Function((self, args) => {
            return new Boolean(self.raw != args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.uint32")) }, "sys.bool");

        public static Function _addassign = new Function((self, args) => {
            self.raw = (args[0] + self.raw > uint.MaxValue) ? uint.MaxValue : (uint)(args[0] + self.raw);
            return self;
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.uint32")) }, "sys.uint32");

        public static Function _substractassign = new Function((self, args) => {
            self.raw = (self.raw - args[0] < uint.MinValue) ? uint.MinValue : (uint)(self.raw - args[0]);
            return self;
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.uint32")) }, "sys.uint32");

        public static Function _multiplyassign = new Function((self, args) => {
            self.raw = (args[0] * self.raw > uint.MaxValue) ? uint.MaxValue : (uint)(args[0] * self.raw);
            return self;
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.uint32")) }, "sys.uint32");

        public static Function _divideassign = new Function((self, args) => {
            self.raw = (args[0] / self.raw > uint.MaxValue) ? uint.MaxValue : (uint)(args[0] / self.raw);
            return self;
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.uint32")) }, "sys.uint32");

        public static Function _modassign = new Function((self, args) => {
            self.raw = (uint)(Convert.ToInt32(self) % Convert.ToInt32(args[0]));
            return self;
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.uint32")) }, "sys.uint32");

        public static Function _lincrement = new Function((self, args) => {
            self.raw = (uint)(self.raw + 1);
            return self;
        }, new List<Pair>(), "sys.uint32");

        public static Function _ldecrement = new Function((self, args) => {
            self.raw = (uint)(self.raw - 1);
            return self;
        }, new List<Pair>(), "sys.uint32");

        public static Function _rincrement = new Function((self, args) => {
            self.raw = (uint)(self.raw + 1);
            return new UInt32((uint)(self.raw - 1));
        }, new List<Pair>(), "sys.uint32");

        public static Function _rdecrement = new Function((self, args) => {
            self.raw = (uint)(self.raw - 1);
            return new UInt32((uint)(self.raw + 1));
        }, new List<Pair>(), "sys.uint32");

        public static implicit operator uint(UInt32 f)
        {
            return f.raw;
        }

        public static implicit operator UInt32(uint d)
        {
            return new UInt32(d);
        }

        internal new string type = "sys.uint32";

        public override string ToString()
        {
            return Convert.ToUInt32(this.raw).ToString();
        }
    }
}
