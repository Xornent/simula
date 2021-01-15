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
        }, new List<Pair>() { });

        public static Function toUpper = new Function((self, args) => {
            return new String(self.raw.ToUpper());
        }, new List<Pair>() { });

        public static Function _add = new Function((self, args) => {
            return new String(self.raw + args[0].ToString());
        }, new List<Pair>() { new Pair(new String("right"), new string("any")) });

        public static Function _multiply = new Function((self, args) => {
            string systemStr = "";
            for(int i = 0; i < args[0].raw; i++) {
                systemStr += self.raw;
            }

            return new String(systemStr);
        }, new List<Pair>() { new Pair(new String("right"), new string("sys.int")) });

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