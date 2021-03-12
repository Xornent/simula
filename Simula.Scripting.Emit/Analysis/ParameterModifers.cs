using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simula.Scripting.Emit.Analysis
{
    [Flags]
    public enum ParameterModifers : byte
    {
        Readonly      = 0b00000001 ,        // readonly flag.
        Extends       = 0b00000010 ,        // extends flag.

        ByValue       = 0b00000100 ,        // byval flag.
        ByReference   = 0b00001000 ,        // byref flag.
        ByName        = 0b00010000 ,        // byname flag.

        // by default the passer attribute is set to byval.

        PreserveDomain= 0b01000000 ,        // presdom flag.    [experimental]
        CurrentDomain = 0b10000000          // cdom flag.       [experimental]
    }
}
