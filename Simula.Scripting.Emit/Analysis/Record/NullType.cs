using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Analysis.Record
{
    public class NullType : TypeRecord
    {
        public NullType() : base() { }
        public new string Symbol { get; set; } = "null";
    }
}
