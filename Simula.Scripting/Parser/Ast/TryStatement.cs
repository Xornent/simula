using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Parser.Ast
{
    public class TryStatement : BlockStatement
    {
        public TryStatement(CatchStatement? catches)
        {
            this.Catch = catches;
            if (catches!=null) this.HasCatchBlock = true;
        }
        public bool HasCatchBlock { get; set; } = false;
        public CatchStatement? Catch { get; set; }
    }
}
