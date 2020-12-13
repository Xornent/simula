using System;
using System.Dynamic;
using System.Collections.Generic;

namespace Simula.Scripting.Types
{
    public class String : Var
    {
        private string raw = "";
        public String() { }
        public String(string systemString)
        {
            this.raw = systemString;
        }

        public static Function toString = new Function((self, args) => {
            return new String(self.raw);
        }, new List<Pair>() { });

        public static Function toUpper = new Function((self, args) => {
            return new String(self.raw.ToUpper());
        }, new List<Pair>() { });

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