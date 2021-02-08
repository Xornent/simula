using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Dom
{
    [System.AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public sealed class ClassExportAttribute : Attribute
    {
        public readonly string Module = "";
        public readonly string FullName = "";
        public readonly string Name = "";
        public readonly string Documentation = "";

        public ClassExportAttribute(string name, string module = "", string doc = "", string fullName = "")
        {
            if (string.IsNullOrEmpty(fullName)) this.FullName =
                     string.IsNullOrEmpty(module) ? name : (module + "." + name);
            this.Module = module;
            this.Name = name;
            this.Documentation = doc;
        }
    }
}
