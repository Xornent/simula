using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Analysis.Record
{
    // * any cannot be one of the combination type elements.
    // ( a & b ) & b     = a & b
    // ( a & b ) & c     = a & b & c

    public class CombinationType : TypeRecord
    {
        public List<TypeRecord> Combination { get; set; } = new List<TypeRecord>();
    }
}
