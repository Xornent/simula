using System;
using System.Collections.Generic;
using System.Text;
using Simula.Scripting.Reflection.Markup;

namespace Simula.Scripting.Type {

    [Expose("char")]
    public class Character {
        private char value = ' ';

        public static implicit operator char(Character s) {
            return s.value;
        }

        public static implicit operator Character(char s) {
            return new Character() { value = s };
        }

        public override bool Equals(object? obj) {
            if (obj == null) return false;
            if (obj is char) return this.value == (char)obj;
            if (obj is Character) return this.value == (obj as Character)?.value;
            return false;
        }

        public override int GetHashCode() {
            return value.GetHashCode();
        }
    }
}
