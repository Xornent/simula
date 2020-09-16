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

        public override string ToString() {
            return value.ToString();
        }

        [Expose("_create", true)]
        public Function _create() {
            return new Function(this.GetType().GetMethod("_init"), this);
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
            return Equals(f);
        }

        [Expose("_notequal")]
        public Boolean _notequal(Float f) {
            return _equal(f)._not();
        }

        [Expose("_morethan")]
        public Boolean _morethan(Float f) {
            return this.value > f;
        }

        [Expose("_lessthan")]
        public Boolean _lessthan(Float f) {
            return this.value < f;
        }

        [Expose("_nomorethan")]
        public Boolean _nomorethan(Float f) {
            return this.value <= f;
        }

        [Expose("_nolessthan")]
        public Boolean _nolessthan(Float f) {
            return this.value >= f;
        }
    }
}
