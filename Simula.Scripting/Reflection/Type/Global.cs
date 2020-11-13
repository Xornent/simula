using Simula.Scripting.Reflection;
using Simula.Scripting.Reflection.Markup;
using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Type {

    [Expose("<global>")]
    public static class Global {

        [Expose("null")]
        public static readonly NullType Null = NullType.Null;

        [Expose("alert")]
        public static void Alert(Member str) {
            Compilation.RuntimeContext.RequestStandardOutput(str.ToString());
        }
    }
}
