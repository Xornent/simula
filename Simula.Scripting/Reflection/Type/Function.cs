using System;
using System.Collections.Generic;
using System.Text;
using Simula.Scripting.Reflection.Markup;

namespace Simula.Scripting.Type {

    [Expose("func")]
    public class _Function : Var{
        public _Function(Reflection.Function t) {
            this.value = t;
        }

        public Reflection.Function? value;
        public override string ToString() {
            List<string> parameters = new List<string>();
            foreach(var param in value?.Parameters ?? new List<Reflection.NamedType>()) {
                parameters.Add(param.Type.Name + " " + param.Name);
            }

            string expr = "()";
            if(parameters.Count > 0) expr = parameters.JoinString(", ");
            return value != null ? ((value is Reflection.ClrMember) ? "native " : "" )
                + "func: " + value.Name.ToString() + "(" + expr + ")" : "func: <empty>";
        }

        [Expose("_self")]
        public object? _self() {
            return value ?? null;
        }
    }
}
