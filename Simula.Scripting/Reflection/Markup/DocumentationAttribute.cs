using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Reflection.Markup {

    [System.AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    sealed class DocumentationAttribute : Attribute {
        readonly string positionalString;

        public DocumentationAttribute(string positionalString) {
            this.positionalString = positionalString;
        }

        public string PositionalString {
            get { return positionalString; }
        }
    }
}
