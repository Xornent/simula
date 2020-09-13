using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Reflection {
    
    public class Function : SourceBase {
        public string Name = "";
        public string FullName = "";
        public List<string> ModuleHirachy = new List<string>();
        public List<(Base type, string name)> Parameters = new List<(Base, string)>();

        public List<Syntax.Statement> Startup = new List<Syntax.Statement>();
        public Documentation DocumentationSource = new Documentation();

        public bool Compiled = false;
        public Function? Conflict;

        public dynamic Invoke(List<Base> pars, Compilation.RuntimeContext ctx) {

            // 执行在 DOM 语句树定义的函数.

            var selection = this;
            bool notMatch = true;

            while (notMatch) {
                notMatch = false;

                if (pars.Count != selection.Parameters.Count) { notMatch = true; continue; }
                int count = 0;
                foreach (var item in pars) {
                    if (item.GetType() != Parameters[count].type.GetType()) notMatch = true;
                }

                if (!notMatch) break;
                if (selection.Conflict == null) break;
                selection = selection.Conflict;
            }

            return new Syntax.BlockStatement() { Children = selection.Startup }.Operate(ctx);
        }
    }
}
