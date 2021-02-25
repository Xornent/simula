using Simula.Scripting.Analysis.Record;
using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Analysis.Interop
{
    public class ClrFunctionRecord : Record.FunctionRecord
    {
#pragma warning disable CS8618 
        public ClrFunctionRecord(Delegate clrFunc) : base(clrFunc.Method.Name) 
        {
            this.clrFunction = clrFunc;
        }

        public ClrFunctionRecord(Delegate clrFunc, string symbol) : base(symbol)
        {
            this.clrFunction = clrFunc;
        }
#pragma warning restore CS8618 

        public new bool IsClr { get; } = true;

        private Delegate clrFunction;
    }
}
