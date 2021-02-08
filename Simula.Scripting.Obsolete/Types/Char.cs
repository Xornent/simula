using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Types
{
    public class Char : Var
    {
        public ushort raw = 0;
        public Char() { }
        public Char(char systemChar) : this()
        {
            this.raw = systemChar;
        }

        public Char(ushort systemShort) : this()
        {
            this.raw = systemShort;
        }

        public static Function _add = new Function((self, args) => {
            return new Char((self.raw + args[0].raw) < ushort.MaxValue ? ushort.MaxValue : (ushort)(self.raw + args[0].raw));
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.uint16")) }, "sys.uint16");

        public static Function _substract = new Function((self, args) => {
            return new Char((self.raw - args[0].raw) < ushort.MinValue ? ushort.MinValue: (ushort)(self.raw - args[0].raw));
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.uint16")) }, "sys.uint16");

        public static Function _multiply = new Function((self, args) => {
            return new Char((self.raw * args[0].raw) > ushort.MaxValue ? ushort.MaxValue : (ushort)(self.raw * args[0].raw));
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.uint16")) }, "sys.uint16");

        public static Function _divide = new Function((self, args) => {
            return new Char((self.raw / args[0].raw) > ushort.MaxValue ? (ushort)ushort.MaxValue : (ushort)(self.raw / args[0].raw));
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.uint16")) }, "sys.uint16");

        public static Function _mod = new Function((self, args) => {
            return new Char((ushort)(Convert.ToInt32(self) % Convert.ToInt32(args[0])));
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.uint16")) }, "sys.uint16");

        public static Function _gt = new Function((self, args) => {
            return new Boolean(self.raw > args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.uint16")) }, "sys.bool");

        public static Function _gte = new Function((self, args) => {
            return new Boolean(self.raw >= args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.uint16")) }, "sys.bool");

        public static Function _lt = new Function((self, args) => {
            return new Boolean(self.raw < args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.uint16")) }, "sys.bool");

        public static Function _lte = new Function((self, args) => {
            return new Boolean(self.raw <= args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.uint16")) }, "sys.bool");

        public static Function _equals = new Function((self, args) => {
            return new Boolean(self.raw == args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.uint16")) }, "sys.bool");

        public static Function _notequals = new Function((self, args) => {
            return new Boolean(self.raw != args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.uint16")) }, "sys.bool");

        public static Function _addassign = new Function((self, args) => {
            self.raw = (args[0] + self.raw > ushort.MaxValue) ? ushort.MaxValue : (ushort)(args[0] + self.raw);
            return self;
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.uint16")) }, "sys.uint16");

        public static Function _substractassign = new Function((self, args) => {
            self.raw = (self.raw - args[0] < ushort.MinValue) ? ushort.MinValue : (ushort)(self.raw - args[0]);
            return self;
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.uint16")) }, "sys.uint16");

        public static Function _multiplyassign = new Function((self, args) => {
            self.raw = (args[0] * self.raw > ushort.MaxValue) ? ushort.MaxValue : (ushort)(args[0] * self.raw);
            return self;
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.uint16")) }, "sys.uint16");

        public static Function _divideassign = new Function((self, args) => {
            self.raw = (args[0] / self.raw > ushort.MaxValue) ? ushort.MaxValue : (ushort)(args[0] / self.raw);
            return self;
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.uint16")) }, "sys.uint16");

        public static Function _modassign = new Function((self, args) => {
            self.raw = (ushort)(Convert.ToInt32(self) % Convert.ToInt32(args[0]));
            return self;
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.uint16")) }, "sys.uint16");

        public static Function _lincrement = new Function((self, args) => {
            self.raw = (ushort)(self.raw + 1);
            return self;
        }, new List<Pair>(), "sys.uint16");

        public static Function _ldecrement = new Function((self, args) => {
            self.raw = (ushort)(self.raw - 1);
            return self;
        }, new List<Pair>(), "sys.uint16");

        public static Function _rincrement = new Function((self, args) => {
            self.raw = (ushort)(self.raw + 1);
            return new Char((ushort)(self.raw - 1));
        }, new List<Pair>(), "sys.uint16");

        public static Function _rdecrement = new Function((self, args) => {
            self.raw = (ushort)(self.raw - 1);
            return new Char((ushort)(self.raw + 1));
        }, new List<Pair>(), "sys.uint16");

        public static implicit operator ushort(Char f)
        {
            return f.raw;
        }

        public static implicit operator char(Char f)
        {
            return Convert.ToChar(f.raw);
        }

        public static implicit operator Char(char d)
        {
            return new Char(d);
        }

        public static implicit operator Char(ushort d)
        {
            return new Char(d);
        }

        internal new string type = "sys.uint16";

        public override string ToString()
        {
            return Convert.ToChar(this.raw).ToString();
        }
    }
}
