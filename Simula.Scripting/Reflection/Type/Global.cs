using Simula.Scripting.Reflection.Markup;
using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Type {

    [Expose("<global>")]
    public static class Global {
        
        [Expose("true")]
        public static readonly Boolean True = Boolean.True;

        [Expose("false")]
        public static readonly Boolean False = Boolean.False;

        [Expose("null")]
        public static readonly _Null Null = new _Null();
    }
}
