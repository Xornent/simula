using System;
using System.Collections.Generic;
using System.Text;
using Simula.Scripting.Reflection.Markup;

namespace Simula.Scripting.Type {
   
    [Expose("int")]
    public class Integer : Var {
        private System.Numerics.BigInteger value;

        public static implicit operator System.Numerics.BigInteger(Integer i) {
            return i.value;
        }

        public static implicit operator Integer(System.Numerics.BigInteger i) {
            return new Integer() { value = i };
        }

        public static implicit operator Float(Integer i) {
            try {
                float f;
                float.TryParse(i.ToString(), out f);
                Float fl = f;
                return fl;
            } catch(OverflowException) {
                if(i.value.Sign>0) {
                    Float fl = float.MaxValue;
                    return fl;
                } else if(i.value.Sign<0) {
                    Float fl = float.MinValue;
                    return fl;
                } else {
                    Float fl = float.NaN;
                    return fl;
                }
            }
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

        public override string ToString() {
            return value.ToString();
        }

        [Expose("_create", true)]
        public Function _create() {
            return new Function(this.GetType().GetMethod("_init"), this);
        }

        [Expose("_init", true)]
        public Integer _init() {
            return this;
        }


        [Expose("_add")]
        public Integer _add(Integer f) {
            return this.value + f;
        }

        [Expose("_minus")]
        public Integer _minus(Integer f) {
            return this.value - f;
        }

        [Expose("_multiply")]
        public Integer _multiply(Integer f) {
            return this.value * f;
        }

        [Expose("_divide")]
        public Integer _divide(Integer f) {
            return this.value / f;
        }

        [Expose("_pow")]
        public Integer _pow(Integer f) {
            return System.Numerics.BigInteger.Pow(this.value, int.Parse(f.ToString()));
        }

        [Expose("_equal")]
        public Boolean _equal(Var f) {
            return Equals(f);
        }

        [Expose("_notequal")]
        public Boolean _notequal(Var f) {
            return _equal(f)._not();
        }

        [Expose("_morethan")]
        public Boolean _morethan(Integer f) {
            return this.value > f;
        }

        [Expose("_lessthan")]
        public Boolean _lessthan(Integer f) {
            return this.value < f;
        }

        [Expose("_nomorethan")]
        public Boolean _nomorethan(Integer f) {
            return this.value <= f;
        }

        [Expose("_nolessthan")]
        public Boolean _nolessthan(Integer f) {
            return this.value >= f;
        }

        [Expose("_mod")]
        public Integer _mod(Integer f) {
            return this.value % f.value;
        }
    }
}
