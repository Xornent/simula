using System;
using System.Collections.Generic;
using System.Text;
using Simula.Scripting.Reflection.Markup;

namespace Simula.Scripting.Type {

    [Expose("float")]
    public class Float : Var {
        public float value;

        public static implicit operator float(Float i) {
            return i.value;
        }

        public static implicit operator Float(float i) {
            return new Float() { value = i };
        }

        public override string ToString() {
            return value.ToString();
        }

        [Expose("_init", true)]
        public Float _init() {
            return this;
        }

        [Expose("_add")]
        public Float _add(Float f) {
            return this.value + f;
        }

        [Expose("_minus")]
        public Float _minus(Float f) {
            return this.value - f;
        }

        [Expose("_multiply")]
        public Float _multiply(Float f) {
            return this.value * f;
        }

        [Expose("_divide")]
        public Float _divide(Float f) {
            return this.value / f;
        }

        [Expose("_pow")]
        public Float _pow(Float f) {
            return (float)Math.Pow(this.value, f);
        }

        [Expose("_equal")]
        public Boolean _equal(Var f) {
            if (f is Float)
                return this.value == ((Float)f).value;
            if (f is Integer bint) {
                if ((int)this.value == this.value)
                    return bint.Equals((int)this.value);
            }
            return false;
        }

        [Expose("_notequal")]
        public Boolean _notequal(Float f) {
            return _equal(f)._not();
        }

        [Expose("_gt")]
        public Boolean _morethan(Float f) {
            return this.value > f;
        }

        [Expose("_lt")]
        public Boolean _lessthan(Float f) {
            return this.value < f;
        }

        [Expose("_lte")]
        public Boolean _nomorethan(Float f) {
            return this.value <= f;
        }

        [Expose("_gte")]
        public Boolean _nolessthan(Float f) {
            return this.value >= f;
        }
    }
}
