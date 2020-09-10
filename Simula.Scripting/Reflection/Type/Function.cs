using System;
using System.Collections.Generic;
using System.Text;
using Simula.Scripting.Reflection.Markup;

namespace Simula.Scripting.Type {
    
    [Expose("func")]
    public class Function {

        // 这个 dynamic 对象是 Func<..., ...> 的一种子类

        private dynamic? function = null;
        public Function(dynamic f) {
            this.function = f;
        }

        
    }
}
