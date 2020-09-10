namespace Simula.Scripting {
    using System;
    using static statics;

    public static partial class statics {

    }

    public static partial class info {

    }

    public partial class @string : obj {
        public new string[] type_set = new string[] { "obj<>", "string<>" };
        internal string eval = "";
        public @string(dynamic[] init) : base(init) {
            _init();
        }

        internal @string(string val) : base(new dynamic[] { }) {
            this.eval = val;
        }

        public new void _init() {

        }

        public new @bool _eval() {
            return @true;
        }

        public new obj[] _value() {
            return new obj[] { new @string(eval) };
        }

        public new @bool _equal(obj o) {
            if(o is @string) {
                if((o as @string) != null) {
                    if(this.eval == (o as @string).eval)return @true;
                }
            }
            return @false;
        }

        public new @string _tostring() {
            return this;
        }
    }
}