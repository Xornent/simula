using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace Simula.Scripting.Types
{
    public class Float : Var
    {
        public BigInteger rawdata = 0;
        public BigInteger point = 0;

        internal new string type = "float";

        public override string ToString()
        {
            return "notimplemented: float";
        }
    }
}
