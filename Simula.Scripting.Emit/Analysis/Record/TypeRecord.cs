using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Analysis.Record
{
    public class TypeRecord : IRecord
    {
        public TypeRecord() { }
        public TypeRecord(string symbol)
        {
            this.Symbol = symbol;
        }

        public string Symbol { get; set; } = "_";
        public bool IsUnion { get; } = false;
        public RecordType RecordType { get { return RecordType.Type; } }
        public bool IsClr { get; } = false;

        public List<string> FieldSymbols { get; set; } = new List<string>();
        public List<TypeRecord> FieldTypes { get; set; } = new List<TypeRecord>();

        public List<TypeRecord> Assertions { get; set; } = new List<TypeRecord>();
        public List<TypeRecord> Inheritages { get; set; } = new List<TypeRecord>();

        public List<IRecord> Locals { get; set; }
        public List<IRecord> References { get; set; }
    }
}
