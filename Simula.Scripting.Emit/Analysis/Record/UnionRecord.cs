using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Analysis.Record
{
    public class UnionRecord : IRecord
    {
        public string Symbol { get; set; } = "_";
        public bool IsUnion { get; } = true;
        public RecordType RecordType { get { return RecordType.Union; } }
        public bool IsClr { get; } = false;

        public TypeRecord Definition { get; set; }

        public List<UnionRecord> Assertions { get; set; } = new List<UnionRecord>();
        public List<UnionRecord> Inheritages { get; set; } = new List<UnionRecord>();
    }
}
