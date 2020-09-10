using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simula.TeX
{
    public class SymbolMappingNotFoundException : Exception
    {
        internal SymbolMappingNotFoundException(string symbolName)
            : base(string.Format("Cannot find mapping for the symbol with name '{0}'.", symbolName))
        {
        }
    }
}
