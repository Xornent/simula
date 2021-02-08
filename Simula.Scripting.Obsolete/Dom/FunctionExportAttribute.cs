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
        public readonly string Description = "";
        public readonly string Returns = "";
        public readonly List<Types.Pair> Pairs = new List<Types.Pair>();

        public FunctionExportAttribute(string name, string pairs, string module = "", string desc = "", string fullName = "")
        {
            if (string.IsNullOrEmpty(fullName)) this.FullName =
                     string.IsNullOrEmpty(module) ? name : (module + "." + name);
            this.Module = module;
            this.Name = name;
            this.Description = desc;
            if (pairs.Contains("@")) {
                string[] paramret = pairs.Split("@");
                this.Returns = paramret[1];
                if (paramret[0] != "") {
                    string[] str = paramret[0].Split('|');
                    foreach (var item in str) {
                        string[] par = item.Split(':');
                        this.Pairs.Add(new Types.Pair(
                            new Types.String(par[0]),
                            new Types.String(par[1])));
                    }
                }
            } else {
                if (pairs != "") {
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
    }
}
