using System;
using System.Collections.Generic;
using System.Text;
using Simula.Scripting.Reflection.Markup;

namespace Simula.Scripting.Type {

    [Expose("string")]
    public class String : Var {
        private string value = "";

        public static implicit operator string(String s) {
            return s.value;
        }

        public static implicit operator String(string s) {
            return new String() { value = s };
        }

        public override bool Equals(object? obj) {
            if (obj == null) return false;
            if (obj is string) return this.value == (obj as string);
            if (obj is String) return this.value == (obj as String)?.value;
            return false;
        }

        public override int GetHashCode() {
            return value.GetHashCode();
        }

        public override string ToString() {
            return value;
        }

        [Expose("_init", true)]
        public String _init() {
            return this;
        }

        [Expose("lower")]
        public String to_lower() {
            return value.ToLower();
        }
    }
}
