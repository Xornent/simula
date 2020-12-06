using System;
using System.Dynamic;

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
        });

        public static Function toUpper = new Function((self, args) => {
            return new String(self.raw.ToUpper());
        });

        internal new string type = "string";

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