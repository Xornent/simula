namespace Simula.Scripting {
    using System;
    using static statics;

    public static partial class statics {

    }

    public static partial class info {

    }

    public partial class @float : obj {
        public new string[] type_set = new string[] { "obj<>", "float<>" };
        internal float eval = 0;
        public @float(dynamic[] init) : base(init) {
            _init();
        }

        internal @float(float val) : base(new dynamic[] { }) {
            this.eval = val;
        }

        public new void _init() {

        }

        public new @bool _eval() {
            if (this.eval != 0) return @true;
            else return @false;
        }

        public new obj[] _value() {
            return new obj[] { new @float(eval) };
        }

        public new @bool _equal(obj o) {
            if (o is @float) {
                if ((o as @float) != null) {
                    if (this.eval == (o as @float).eval) return @true;
                }
            }
            return @false;
        }

        public new @string _tostring() {
            return new @string(eval.ToString());
        }
    }
}