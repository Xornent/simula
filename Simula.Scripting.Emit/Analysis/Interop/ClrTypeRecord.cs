using Simula.Scripting.Analysis.Record;
using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Analysis.Interop
{
    public class ClrTypeRecord : Record.FunctionRecord
    {
        public ClrTypeRecord(Type type) : base(type.Name)
        {
            this.clrType = type;
        }

        public ClrTypeRecord(Type type, string symbol) : base(symbol)
        {
            this.clrType = type;
        }

        public new bool IsClr { get; } = true;

        private Type clrType;

        public bool IsValueType { get => clrType.IsValueType; }
        public bool IsAbstract { get => clrType.IsAbstract; }
        public bool IsAnsi { get => clrType.IsAnsiClass; }
    }
}
