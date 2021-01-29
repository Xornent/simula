using System;

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
