using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Types
{
    public class Boolean : Var
    {
        private bool raw = false;
        public Boolean() { }
        public Boolean(bool val)
        {
            this.raw = val;
        }

        public static Function _equals = new Function((self, args) => {
            return (Boolean)(self.raw == args[0]);
        }, new List<Pair>() { new Pair(new String("right"), new String("bool")) });

        public static Function _notequals = new Function((self, args) => {
            return (Boolean)(self.raw != args[0]);
        }, new List<Pair>() { new Pair(new String("right"), new String("bool")) });

        public static Function _not = new Function((self, args) => {
            return new Boolean(!(self.raw));
        }, new List<Pair>() { });

        public static Function _and;
        public static Function _or;

        internal new string type = "sys.bool";

        public static implicit operator bool(Boolean b)
        {
            return b.raw;
        }

        public static implicit operator Boolean(bool b)
        {
            return new Boolean(b);
        }

        public override string ToString()
        {
            return raw.ToString();
        }
    }
}
