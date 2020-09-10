using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Compilation {

    public class GlobalContext {

        public GlobalContext() {
            Global = new RuntimeContext();
            Domain.Push(this.Global);
        }

        RuntimeContext Global;
        public Stack<RuntimeContext> Domain = new Stack<RuntimeContext>();
    }
}
