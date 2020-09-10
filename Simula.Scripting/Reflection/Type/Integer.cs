using System;
using System.Collections.Generic;
using System.Text;
using Simula.Scripting.Reflection.Markup;

namespace Simula.Scripting.Type {
   
    [Expose("int")]
    public class Integer {
        private System.Numerics.BigInteger value;

        public static implicit operator System.Numerics.BigInteger(Integer i) {
            return i.value;
        }

        public static implicit operator Integer(System.Numerics.BigInteger i) {
            return new Integer() { value = i };
        }

        public override bool Equals(object? obj) {
            if (obj == null) return false;
            if (obj is int) return this.value.Equals((int)obj);
            if (obj is uint) return this.value.Equals((uint)obj);
            if (obj is long) return this.value.Equals((long)obj);
            if (obj is ulong) return this.value.Equals((ulong)obj);
            if (obj is System.Numerics.BigInteger) return this.value == (System.Numerics.BigInteger)obj;
            if (obj is Integer) return this.value == ((Integer)obj).value;
            return false;
        }

        public override int GetHashCode() {
            return value.GetHashCode();
        }
    }
}
