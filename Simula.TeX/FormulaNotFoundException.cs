using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simula.TeX
{
    public class FormulaNotFoundException : Exception
    {
        internal FormulaNotFoundException(string formulaName)
            : base(string.Format("Cannot find predefined formula with name '{0}'.", formulaName))
        {
        }
    }
}
