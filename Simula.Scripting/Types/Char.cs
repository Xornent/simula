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
            return new Char(self.raw + args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.byte")) }, "sys.byte");

        public static Function _substract = new Function((self, args) => {
            return new Byte(self.raw - args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.byte")) }, "sys.byte");

        public static Function _multiply = new Function((self, args) => {
            return new Byte(self.raw * args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.byte")) }, "sys.byte");

        public static Function _divide = new Function((self, args) => {
            return new Byte((self.raw / args[0].raw) >= 256 ? (byte)255 : (byte)(self.raw / args[0].raw));
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.byte")) }, "sys.byte");

        public static Function _mod = new Function((self, args) => {
            return new Float((byte)(Convert.ToInt32(self) % Convert.ToInt32(args[0])));
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.byte")) }, "sys.byte");

        public static Function _gt = new Function((self, args) => {
            return new Boolean(self.raw > args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.byte")) }, "sys.bool");

        public static Function _gte = new Function((self, args) => {
            return new Boolean(self.raw >= args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.byte")) }, "sys.bool");

        public static Function _lt = new Function((self, args) => {
            return new Boolean(self.raw < args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.byte")) }, "sys.bool");

        public static Function _lte = new Function((self, args) => {
            return new Boolean(self.raw <= args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.byte")) }, "sys.bool");

        public static Function _equals = new Function((self, args) => {
            return new Boolean(self.raw == args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.byte")) }, "sys.bool");

        public static Function _notequals = new Function((self, args) => {
            return new Boolean(self.raw != args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.byte")) }, "sys.bool");

        public static Function _addassign = new Function((self, args) => {
            self.raw = (args[0] + self.raw >= 256) ? (byte)255 : (byte)(args[0] + self.raw);
            return self;
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.byte")) }, "sys.byte");

        public static Function _substractassign = new Function((self, args) => {
            self.raw = (self.raw - args[0] < 0) ? (byte)0 : (byte)(self.raw - args[0]);
            return self;
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.byte")) }, "sys.byte");

        public static Function _multiplyassign = new Function((self, args) => {
            self.raw = (args[0] * self.raw >= 256) ? (byte)255 : (byte)(args[0] * self.raw);
            return self;
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.byte")) }, "sys.byte");

        public static Function _divideassign = new Function((self, args) => {
            self.raw = (args[0] / self.raw >= 256) ? (byte)255 : (byte)(args[0] / self.raw);
            return self;
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.byte")) }, "sys.byte");

        public static Function _modassign = new Function((self, args) => {
            self.raw = (byte)(Convert.ToInt32(self) % Convert.ToInt32(args[0]));
            return self;
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.byte")) }, "sys.byte");

        public static Function _lincrement = new Function((self, args) => {
            self.raw = (byte)(self.raw + 1);
            return self;
        }, new List<Pair>(), "sys.byte");

        public static Function _ldecrement = new Function((self, args) => {
            self.raw = (byte)(self.raw - 1);
            return self;
        }, new List<Pair>(), "sys.byte");

        public static Function _rincrement = new Function((self, args) => {
            self.raw = (byte)(self.raw + 1);
            return new Byte((byte)(self.raw - 1));
        }, new List<Pair>(), "sys.byte");

        public static Function _rdecrement = new Function((self, args) => {
            self.raw = (byte)(self.raw - 1);
            return new Byte((byte)(self.raw + 1));
        }, new List<Pair>(), "sys.byte");

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

        internal new string type = "sys.char";

        public override string ToString()
        {
            return Convert.ToChar(this.raw).ToString();
        }
    }
}
