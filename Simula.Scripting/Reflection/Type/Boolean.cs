using System;
using System.Collections.Generic;
using System.Text;
using Simula.Scripting.Reflection.Markup;

namespace Simula.Scripting.Type {

    [Expose("bool")]
    public class Boolean : Var{
        private bool value = false;

        public static readonly Boolean True = new Boolean() { value = true };
        public static readonly Boolean False = new Boolean() { value = false };

        public static implicit operator bool(Boolean b) {
            return b.value;
        }

        public static implicit operator Boolean(bool b) {
            if (b) return True;
            else return False;
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

        [Expose("_create", true)]
        public Function _create() {
            return new Function(this.GetType().GetMethod("_init"), this);
        }

        [Expose("_init", true)]
        public Boolean _init(String evaluation) {
            if (evaluation.to_lower() == "true")
                return True;
            else return False;
        }

        [Expose("_bitleft", true)]
        public Boolean _bitleft(Integer evaluation) {
            return False;
        }

        [Expose("_bitright", true)]
        public Boolean _bitright(Integer evaluation) {
            return False;
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
