using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Analysis.Record
{
    public class TypeRecord : IRecord
    {
        public TypeRecord() 
        {
            this.Oid = Guid.NewGuid();
        }

        public TypeRecord(string symbol) : this()
        {
            this.Symbol = symbol;
        }

        public string Symbol { get; set; } = "_";
        public bool IsUnion { get; } = false;
        public RecordType RecordType { get { return RecordType.Type; } }
        public bool IsClr { get; } = false;

        internal Parser.Ast.DataDeclaration Declaration;

        public IDictionary<string, Guid> FieldSymbols { get; set; } = new Dictionary<string, Guid>();
        public List<TypeRecord> FieldTypes { get; set; } = new List<TypeRecord>();

        public List<TypeRecord> Assertions { get; set; } = new List<TypeRecord>();
        public List<TypeRecord> Inheritages { get; set; } = new List<TypeRecord>();

        public List<IRecord> Locals { get; set; }
        public List<IRecord> References { get; set; }

        public Guid Oid { get; set; }
    }
}
