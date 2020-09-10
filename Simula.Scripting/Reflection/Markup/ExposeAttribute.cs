using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Reflection.Markup {

    [System.AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    sealed class ExposeAttribute : Attribute {
        readonly string alias;
        readonly bool toSystemOnly;
        public ExposeAttribute(string alias, bool systemOnly = false) {
            this.alias = alias;
            this.toSystemOnly = systemOnly;
        }

        public string Alias {
            get { return alias; }
        }

        public bool ToSystemOnly {
            get { return toSystemOnly; }
        }
    }
}
