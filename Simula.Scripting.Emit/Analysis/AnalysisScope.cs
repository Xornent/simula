using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Analysis
{
    public class AnalysisScope
    {
        public Dictionary<string, Record.IRecord> Registry = new Dictionary<string, Record.IRecord>();
    }
}
