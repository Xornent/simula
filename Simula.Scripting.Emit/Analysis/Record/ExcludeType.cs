using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Analysis.Record
{
    public class ExcludeType : TypeRecord
    {
        public ExcludeType() : base() { }
        public ExcludeType(TypeRecord target, TypeRecord exclusion) : this()
        {
            this.Target = target;
            this.Exclusion = exclusion;
        }

        TypeRecord Target { get; set; }
        TypeRecord Exclusion { get; set; }
    }
}
