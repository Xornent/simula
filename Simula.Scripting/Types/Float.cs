﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace Simula.Scripting.Types
{
    public class Float : Var
    {
        public double raw = 0;
        public Float() { }
        public Float(double systemDouble) : this()
        {
            this.raw = systemDouble;
        }

        public static Function _add = new Function((self, args) => {
            return new Float(self.raw + args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.float")) }, "sys.float");

        public static Function _substract = new Function((self, args) => {
            return new Float(self.raw - args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.float")) }, "sys.float");

        public static Function _multiply = new Function((self, args) => {
            return new Float(self.raw * args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.float")) }, "sys.float");

        public static Function _divide = new Function((self, args) => {
            return new Float(self.raw / args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.float")) }, "sys.float");

        public static Function _pow = new Function((self, args) => {
            return new Float(Math.Pow(self, args[0]));
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.float")) }, "sys.float");

        public static Function _mod = new Function((self, args) => {
            return new Float((double)(Convert.ToInt32(self) % Convert.ToInt32(args[0])));
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.float")) }, "sys.float");

        public static Function _gt = new Function((self, args) => {
            return new Boolean(self.raw > args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.float")) }, "sys.bool");

        public static Function _gte = new Function((self, args) => {
            return new Boolean(self.raw >= args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.float")) }, "sys.bool");

        public static Function _lt = new Function((self, args) => {
            return new Boolean(self.raw < args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.float")) }, "sys.bool");

        public static Function _lte = new Function((self, args) => {
            return new Boolean(self.raw <= args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.float")) }, "sys.bool");

        public static Function _equals = new Function((self, args) => {
            return new Boolean(self.raw == args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.float")) }, "sys.bool");

        public static Function _notequals = new Function((self, args) => {
            return new Boolean(self.raw != args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.float")) }, "sys.bool");

        public static Function _addassign = new Function((self, args) => {
            self.raw += args[0]; // implicit cast to double or int.
            return self;
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.float")) }, "sys.float");

        public static Function _substractassign = new Function((self, args) => {
            self.raw -= args[0]; // implicit cast to double or int.
            return self;
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.float")) }, "sys.float");

        public static Function _powassign = new Function((self, args) => {
            self.raw = Math.Pow(self.raw, args[0]);
            return self;
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.float")) }, "sys.float");

        public static Function _multiplyassign = new Function((self, args) => {
            self.raw *= args[0]; // implicit cast to double or int.
            return self;
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.float")) }, "sys.float");

        public static Function _divideassign = new Function((self, args) => {
            self.raw /= args[0]; // implicit cast to double or int.
            return self;
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.float")) }, "sys.float");

        public static Function _modassign = new Function((self, args) => {
            self.raw = (double)(Convert.ToInt32(self) % Convert.ToInt32(args[0]));
            return self;
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.float")) }, "sys.float");

        public static Function _lincrement = new Function((self, args) => {
            self.raw = self.raw + 1;
            return self;
        }, new List<Pair>(), "sys.float");

        public static Function _ldecrement = new Function((self, args) => {
            self.raw = self.raw - 1;
            return self;
        }, new List<Pair>(), "sys.float");

        public static Function _rincrement = new Function((self, args) => {
            self.raw = self.raw + 1;
            return new Float(self.raw - 1);
        }, new List<Pair>(), "sys.float");

        public static Function _rdecrement = new Function((self, args) => {
            self.raw = self.raw - 1;
            return new Float(self.raw + 1);
        }, new List<Pair>(), "sys.float");

        public static Function _inverse = new Function((self, args) => {
            return new Float(-self.raw);
        }, new List<Pair>(), "sys.float");

        public static implicit operator double(Float f)
        {
            return f.raw;
        }

        public static implicit operator int(Float f)
        {
            return int.Parse(string.Format("{0:0}", f.raw));
        }

        public static implicit operator Float(double d)
        {
            return new Float(d);
        }

        internal new string type = "sys.float";

        public override string ToString()
        {
            double abs = Math.Abs(raw);
            if ((abs < 1e5 && abs > 1e-4) || abs == 0) {
                return string.Format("{0:0.0000}", raw);
            } else {

                // display as scientific notation
                int magnitude = 0;
                if (raw > 1 || raw < -1) {
                    while( raw >= 10 || raw <= -10) {
                        magnitude++;
                        raw /= 10;
                    }
                    return string.Format("{0:0.0000}", raw) + " E" + magnitude + "";

                } else {
                    while (raw < 1 && raw > -1 ) {
                        magnitude--;
                        raw *= 10;
                    }
                    return string.Format("{0:0.0000}", raw) + " E" + magnitude + "";

                }
            }
        }
    }
}
