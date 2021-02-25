using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Analysis.Record
{
    public class ExcludeType : TypeRecord
    {
        TypeRecord Target { get; set; }
        TypeRecord Exclusion { get; set; }
    }
}
