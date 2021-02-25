using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Analysis.Record
{
    public class DataRecord : IRecord
    {
        public string Symbol { get; set; } = "_";
        public bool IsUnion { get; } = false;
        public RecordType RecordType { get { return RecordType.Data; } }
        public bool IsClr { get; } = false;

        public StackType Type { get; set; }
    }

    public enum StackType
    {
        Int8,
        Int16,
        Int32,
        Int64,
        UInt8,
        UInt16,
        UInt32,
        UInt64,
        Logical,
        Float,
        Double
    }
}
