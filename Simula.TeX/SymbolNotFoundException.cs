using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simula.TeX
{
    public class SymbolNotFoundException : Exception
    {
        internal SymbolNotFoundException(string symbolName)
            : base(string.Format("Cannot find symbol with the name '{0}'.", symbolName))
        {
        }
    }
}
