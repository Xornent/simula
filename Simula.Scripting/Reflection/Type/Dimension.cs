using System;
using System.Collections.Generic;
using System.Text;
using Simula.Scripting.Reflection.Markup;

namespace Simula.Scripting.Type {

    [Expose("dimension")]
    public class Dimension : Var {
        int n;
        public Dimension(int _n) {
            this.n = _n;
        }
    }
}
