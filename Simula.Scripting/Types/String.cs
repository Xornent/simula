using System;
using System.Dynamic;

namespace Simula.Scripting.Types
{
    public class String
    {
        private string raw = "";
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
    }
}