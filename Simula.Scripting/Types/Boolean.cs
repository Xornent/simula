using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Types
{
    public class Boolean
    {
        private bool raw = false;
        public Boolean()
        {
        }

        public Boolean(bool val)
        {
            this.raw = val;
        }

        public static Function _equals = new Function((self, args) => {
            return (Boolean)(self.raw == args[0]);
        });

        public Function _notequals = new Function((self, args) => {
            return (Boolean)(self.raw != args[0]);
        });

        public Function _not = new Function((self, args) => {
            return new Boolean(!(self.raw));
        });

        public Function _add;
        public Function _or;

        public Class type = Class.typeBool;

        public static implicit operator bool(Boolean b)
        {
            return b.raw;
        }

        public static implicit operator Boolean(bool b)
        {
            return new Boolean(b);
        }
    }
}
