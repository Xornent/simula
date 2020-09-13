using System;
using System.Collections.Generic;
using System.Text;
using Simula.Scripting.Reflection.Markup;

namespace Simula.Scripting.Type {
    
    [Expose("func")]
    public class Function : Var {

        // 这个 dynamic 对象是 Func<..., ...> 的一种子类

        public System.Reflection.MethodInfo? function = null;
        public Function(System.Reflection.MethodInfo? f) {
            this.function = f;
        }

    }
}
