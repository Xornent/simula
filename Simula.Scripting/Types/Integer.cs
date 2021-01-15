using System.Runtime.CompilerServices;
using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace Simula.Scripting.Types
{
    public class Integer : Var
    {
        public BigInteger raw = 0;
        public Integer() : base() { }
        public Integer(BigInteger bigint) : base()
        {
            this.raw = bigint;
        }

        public Integer(int val) : base()
        {
            this.raw = val;
        }

        public static Function _add = new Function((self, args) => {
            return new Integer(self.raw + args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int")) });

        public static Function _substract = new Function((self, args) => {
            return new Integer(self.raw - args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int")) });

        public static Function _multiply = new Function((self, args) => {
            return new Integer(self.raw * args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int")) });

        public static Function _divide = new Function((self, args) => {
            return new Integer(BigInteger.Divide(self.raw, args[0].raw));
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int")) });

        public static Function _mod = new Function((self, args) => {
            return new Integer(BigInteger.Remainder(self.raw, args[0].raw));
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int")) });

        public static Function _gt = new Function((self, args) => {
            return new Boolean(self.raw > args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int")) });

        public static Function _gte  = new Function((self, args) => {
            return new Boolean(self.raw >= args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int")) });

        public static Function _lt = new Function((self, args) => {
            return new Boolean(self.raw < args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int")) });

        public static Function _lte = new Function((self, args) => {
            return new Boolean(self.raw <= args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int")) });

        public static Function _equals = new Function((self, args) => {
            return new Boolean(self.raw == args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int")) });

        public static Function _notequals = new Function((self, args) => {
            return new Boolean(self.raw != args[0].raw);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int")) });

        public static Function _addassign = new Function((self, args) => {
            self.raw += args[0];
            return self;
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int")) });

        public Function _lshift;
        public Function _rshift;
        public Function _lincrement;
        public Function _rincrement;
        public Function _ldecrement;
        public Function _rdecrement;

        public static implicit operator BigInteger(Integer i)
        {
            return i.raw;
        }

        public static implicit operator Integer(BigInteger b)
        {
            return new Integer(b);
        }

        public static implicit operator int(Integer i)
        {
            return int.Parse(i.raw.ToString() ?? "0");
        }

        public static implicit operator Integer(int i) 
        {
            return new Integer(i);
        }

        internal new string type = "sys.int";

        public override string ToString()
        {
            return raw.ToString();
        }
    }
}
