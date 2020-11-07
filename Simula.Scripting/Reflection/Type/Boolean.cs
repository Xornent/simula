using System;
using System.Collections.Generic;
using System.Text;
using Simula.Scripting.Reflection.Markup;

namespace Simula.Scripting.Type {

    [Expose("bool")]
    public class Boolean : Var{
        public Boolean(string eval) {
            if(eval.ToLower().Trim() == "true")
                this.value = true;
            else this.value = false;
        }

        private bool value = false;

        public static implicit operator bool(Boolean b) {
            return b.value;
        }

        public static implicit operator Boolean(bool b) {
            if (b) return new Boolean("true");
            else return new Boolean("false");
        }

        public override bool Equals(object? obj) {
            if (obj == null) return false;
            if (obj is char) return this.value == (bool)obj;
            if (obj is Boolean) return this.value == (obj as Boolean)?.value;
            return false;
        }

        public override int GetHashCode() {
            return value.GetHashCode();
        }

        public override string ToString() {
            if (this.value == true) return "true";
            else return "false";
        }

        [Expose("_init", true)]
        public Boolean _init(String evaluation) {
            if (evaluation.to_lower() == "true")
                return new Boolean("true");
            else return new Boolean("false");
        }

        [Expose("_bitlshift", true)]
        public Boolean _bitleft(Integer evaluation) {
            return new Boolean("false");
        }

        [Expose("_bitrshift", true)]
        public Boolean _bitright(Integer evaluation) {
            return new Boolean("false");
        }

        [Expose("_equal", true)]
        public Boolean _equal(Boolean evaluation) {
            return Equals(evaluation);
        }

        [Expose("_notequal", true)]
        public Boolean _notequal(Boolean evaluation) {
            return _equal(evaluation)._not();
        }

        [Expose("_or", true)]
        public Boolean _or(Boolean evaluation) {
            return this.value || evaluation.value;
        }

        [Expose("_and", true)]
        public Boolean _and(Boolean evaluation) {
            return this.value && evaluation.value;
        }

        public Boolean _not() {
            return !this.value;
        }
    }
}
