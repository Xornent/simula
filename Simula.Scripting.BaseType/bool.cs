namespace Simula.Scripting {
    using System;
    using static statics;

    public static partial class statics {
        public static dynamic @true = new @bool(1);
        public static dynamic @false = new @bool(0);
     }

    public static partial class info {

    }

    public partial class @bool : obj {
        public new string[] type_set = new string[] { "obj<>", "bool<>" };
        internal int eval = 0;
        public @bool(dynamic[] init) : base(init){
            _init();
        }

        internal @bool(int val) : base(new dynamic[] { }) {
            this.eval = val;
        }

        public new void _init() {

        }

        public new @bool _eval() {
            if (eval == 0) return @false;
            else return @true;
        }

        public new obj[] _value() {
            if (eval == 1) return new obj[] { @true };
            else return new obj[] { @false };
        }

        public new @bool _equal(obj o) {
            if (this._eval().eval == o._eval().eval) return @true;
            else return @false;
        }

        public new @string _tostring() {
            if (eval == 0) return new @string("false");
            else return new @string("true");
        }

        internal bool _tobool() {
            if (eval == 0) return false;
            else return true;
        }
    }
}
