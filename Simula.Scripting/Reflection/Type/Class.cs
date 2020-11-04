using System;
using System.Collections.Generic;
using System.Text;
using Simula.Scripting.Reflection.Markup;

namespace Simula.Scripting.Type {

    [Expose("class")]
    public class _Class : Var{
        public _Class(System.Type t) {
            this.value = t;
        }

        public System.Type? value;
        public override string ToString() {
            return value != null ? "class: " + value.ToString() : "class: <empty>";
        }
    }
}
