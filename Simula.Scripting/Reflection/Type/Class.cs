using System;
using System.Collections.Generic;
using System.Text;
using Simula.Scripting.Reflection.Markup;

namespace Simula.Scripting.Type {

    [Expose("class")]
    public class _Class : Var{
        public _Class(Reflection.Class t) {
            this.value = t;
        }

        public Reflection.Class? value;
        public override string ToString() {
            return value != null ? "class: " + value.Name.ToString() : "class: <empty>";
        }

        [Expose("_self")]
        public object? _self() {
            return this.value ?? null;
        }
    }
}
