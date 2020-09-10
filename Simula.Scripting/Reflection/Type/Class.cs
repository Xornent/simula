using Simula.Scripting.Reflection.Markup;
using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Type {

    [Expose("class")]
    public class Class : Var {
        public String name = "";
        public System.Type? type;

        internal Class(System.Type classType) {
            this.type = classType;
        }
    }
}
