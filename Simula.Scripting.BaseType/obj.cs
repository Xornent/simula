namespace Simula.Scripting {
    using System;
    using static statics;

    public static partial class statics {

    }

    public static partial class info {

    }

    public partial class obj {
        public string[] type_set = new string[] { "obj<>" };
        public obj(dynamic[] init) {
            _init();
        }

        public void _init() {

        }

        public @string _type() {
            return new @string(type_set[type_set.Length - 1]);
        }
         
        public @bool _eval() {
            return @true;
        }

        public @string _value() {
            return new @string("");
        }

        public @bool _equal(obj o) {
            
        }

        public @string _tostring() {
            return _type();
        }
    }
}
