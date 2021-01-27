using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Types
{
    public class Byte : Var
    {
        public byte raw = 0;
        public Byte() { }
        public Byte(byte systemByte) : this()
        {
            this.raw = systemByte;
        }

        public static Function _add = new Function((self, args) => {
            return new Byte((self.raw + args[0].raw) >= 256 ? (byte)255 : (byte)(self.raw + args[0].raw));
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.uint8")) }, "sys.uint8");

        public static Function _substract = new Function((self, args) => {
            return new Byte((self.raw - args[0].raw) < 0 ? (byte)0 : (byte)(self.raw - args[0].raw));
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.uint8")) }, "sys.uint8");

        public static Function _multiply = new Function((self, args) => {
            return new Byte((self.raw * args[0].raw) >= 256 ? (byte)255 : (byte)(self.raw * args[0].raw));
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.uint8")) }, "sys.uint8");

        public static Function _divide = new Function((self, args) => {
            return new Byte((self.raw / args[0].raw) >= 256 ? (byte)255 : (byte)(self.raw / args[0].raw));
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.uint8")) }, "sys.uint8");

        public static Function _mod = new Function((self, args) => {
            return new Byte((byte)(Convert.ToInt32(self) % Convert.ToInt32(args[0])));
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.uint8")) }, "sys.uint8");

        public static Function _gt = new Function((self, args) => {
            return new Boolean(self.raw > args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.uint8")) }, "sys.bool");

        public static Function _gte = new Function((self, args) => {
            return new Boolean(self.raw >= args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.uint8")) }, "sys.bool");

        public static Function _lt = new Function((self, args) => {
            return new Boolean(self.raw < args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.uint8")) }, "sys.bool");

        public static Function _lte = new Function((self, args) => {
            return new Boolean(self.raw <= args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.uint8")) }, "sys.bool");

        public static Function _equals = new Function((self, args) => {
            return new Boolean(self.raw == args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.uint8")) }, "sys.bool");

        public static Function _notequals = new Function((self, args) => {
            return new Boolean(self.raw != args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.uint8")) }, "sys.bool");

        public static Function _addassign = new Function((self, args) => {
            self.raw = (args[0] + self.raw >= 256) ? (byte)255 : (byte)(args[0] + self.raw);
            return self;
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.uint8")) }, "sys.uint8");

        public static Function _substractassign = new Function((self, args) => {
            self.raw = (self.raw - args[0] < 0) ? (byte)0 : (byte)(self.raw - args[0]);
            return self;
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.uint8")) }, "sys.uint8");

        public static Function _multiplyassign = new Function((self, args) => {
            self.raw = (args[0] * self.raw >= 256) ? (byte)255 : (byte)(args[0] * self.raw);
            return self;
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.uint8")) }, "sys.uint8");

        public static Function _divideassign = new Function((self, args) => {
            self.raw = (args[0] / self.raw >= 256) ? (byte)255 : (byte)(args[0] / self.raw);
            return self;
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.uint8")) }, "sys.uint8");

        public static Function _modassign = new Function((self, args) => {
            self.raw = (byte)(Convert.ToInt32(self) % Convert.ToInt32(args[0]));
            return self;
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.uint8")) }, "sys.uint8");

        public static Function _lincrement = new Function((self, args) => {
            self.raw = (byte)(self.raw + 1);
            return self;
        }, new List<Pair>(), "sys.uint8");

        public static Function _ldecrement = new Function((self, args) => {
            self.raw = (byte)(self.raw - 1);
            return self;
        }, new List<Pair>(), "sys.uint8");

        public static Function _rincrement = new Function((self, args) => {
            self.raw = (byte)(self.raw + 1);
            return new Byte((byte)(self.raw - 1));
        }, new List<Pair>(), "sys.uint8");

        public static Function _rdecrement = new Function((self, args) => {
            self.raw = (byte)(self.raw - 1);
            return new Byte((byte)(self.raw + 1));
        }, new List<Pair>(), "sys.uint8");

        public static implicit operator byte(Byte f)
        {
            return f.raw;
        }

        public static implicit operator int(Byte f)
        {
            return f.raw;
        }

        public static implicit operator Byte(byte d)
        {
            return new Byte(d);
        }

        internal new string type = "sys.uint8";

        public override string ToString()
        {
            return this.raw.ToString();
        }
    }
}
