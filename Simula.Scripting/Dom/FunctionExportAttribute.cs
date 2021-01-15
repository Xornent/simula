using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Dom
{
    [System.AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public sealed class FunctionExportAttribute : Attribute
    {
        public readonly string Module = "";
        public readonly string FullName = "";
        public readonly string Name = "";
        public readonly List<Types.Pair> Pairs = new List<Types.Pair>();

        public FunctionExportAttribute(string name, string pairs, string module = "", string fullName = "")
        {
            if (string.IsNullOrEmpty(fullName)) this.FullName =
                     string.IsNullOrEmpty(module) ? name : (module + "." + name);
            this.Module = module;
            this.Name = name;
            string[] str = pairs.Split('|');
            foreach (var item in str) {
                string[] par = item.Split(':');
                this.Pairs.Add(new Types.Pair(
                    new Types.String(par[0]),
                    new Types.String(par[1])));
            }
        }
    }
}
