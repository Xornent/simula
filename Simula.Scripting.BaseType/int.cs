namespace Simula.Scripting {
    using System;
    using static statics;

    public static partial class statics {

    }

    public static partial class info {

    }

    public partial class @int : obj {
        public new string[] type_set = new string[] { "obj<>", "int<>" };
        internal System.Numerics.BigInteger eval = 0;
        public @int(dynamic[] init) : base(init) {
            _init();
        }

        internal @int(System.Numerics.BigInteger val) : base(new dynamic[] { }) {
            this.eval = val;
        }

        public new void _init() {

        }

        public new @bool _eval() {
            if (this.eval != 0) return @true;
            else return @false;
        }

        public new obj[] _value() {
            return new obj[] { new @int(eval) };
        }

        public new @bool _equal(obj o) {
            if (o is @int) {
                if ((o as @int) != null) {
                    if (this.eval == (o as @int).eval) return @true;
                }
            }
            return @false;
        }

        public new @string _tostring() {
            return new @string(eval.ToString());
        }
    }
}