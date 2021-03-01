using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simula.Scripting.Analysis.Interop
{
    public class PlaceholderRecord : Record.FunctionRecord
    {
        public PlaceholderRecord(string symbol) : base(symbol) { }
        public new bool IsClr { get; set; } = true;
    }
}
