using System;
using System.Collections.Generic;
using System.Text;
using Simula.Scripting.Reflection.Markup;

namespace Simula.Scripting.Type {

    [Expose("float")]
    public class Float : Var {
        private float value;

        public static implicit operator float(Float i) {
            return i.value;
        }

        public static implicit operator Float(float i) {
            return new Float() { value = i };
        }

        public override bool Equals(object? obj) {
            if (obj == null) return false;
            if (obj is float) return this.value == (float)obj;
            if (obj is double) return this.value == (double)obj;
            if (obj is Float) return this.value == ((Float)obj).value;
            return false;
        }

        public override int GetHashCode() {
            return value.GetHashCode();
        }
    }
}
