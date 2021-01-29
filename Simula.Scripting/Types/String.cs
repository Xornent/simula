using System;
using System.Dynamic;
using System.Collections.Generic;

namespace Simula.Scripting.Types
{
    public class String : Var
    {
        internal string raw = "";
        public String() : base() { }
        public String(string systemString) : base()
        {
            this.raw = systemString;
        }

        public static Function toString = new Function((self, args) => {
            return new String(self.raw);
        }, new List<Pair>() { }, "sys.string");

        public static Function toUpper = new Function((self, args) => {
            return new String(self.raw.ToUpper());
        }, new List<Pair>() { }, "sys.string");

        public static Function toLower = new Function((self, args) => {
            return new String(self.raw.ToLower());
        }, new List<Pair>() { }, "sys.string");

        public static Function toUpperInvariant = new Function((self, args) => {
            return new String(self.raw.ToUpperInvariant());
        }, new List<Pair>() { }, "sys.string");

        public static Function toLowerInvariant = new Function((self, args) => {
            return new String(self.raw.ToLowerInvariant());
        }, new List<Pair>() { }, "sys.string");

        public static Function length = new Function((self, args) => {
            return new Double(self.raw.Length);
        }, new List<Pair>() { }, "sys.double");

        public static Function trim = new Function((self, args) => {
            return new String(self.raw.Trim());
        }, new List<Pair>() { }, "sys.string");

        public static Function trimStart = new Function((self, args) => {
            return new String(self.raw.TrimStart());
        }, new List<Pair>() { }, "sys.string");

        public static Function trimEnd = new Function((self, args) => {
            return new String(self.raw.TrimEnd());
        }, new List<Pair>() { }, "sys.string");

        public static Function replace = new Function((self, args) => {
            return new String(self.raw.Replace(args[0], args[1]));
        }, new List<Pair>() { new Pair(new String("old"), new String("sys.string")), new Pair(new String("new"), new String("sys.string")) }, "sys.string");

        public static Function _add = new Function((self, args) => {
            return new String(self.raw + args[0].ToString());
        }, new List<Pair>() { new Pair(new String("right"), new String("any")) }, "sys.string");

        public static Function _multiply = new Function((self, args) => {
            string systemStr = "";
            for(int i = 0; i < args[0].raw; i++) {
                systemStr += self.raw;
            }

            return new String(systemStr);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int")) }, "sys.string");

        internal new string type = "sys.string";

        public static implicit operator string(String str)
        {
            return str.raw;
        }

        public override string ToString()
        {
            return raw;
        }
    }
}