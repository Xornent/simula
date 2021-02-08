using Simula.Scripting.Syntax;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Simula.Scripting.Build
{
    public class GenerationContext
    {
        public int Indent = 4;
        public int IndentionLevel = 0;

        public HashSet<string> Objects = new HashSet<string>();
        public List<HashSet<string>> Scopes = new List<HashSet<string>>();
        public List<string> ScopeNames = new List<string>();

        internal List<HashSet<string>> classMembers = new List<HashSet<string>>();

        public string DefinerName = "";

        public void PushScope(string name)
        {
            this.Scopes.Add(new HashSet<string>());
            this.ScopeNames.Add(name);
        }

        public void PopScope()
        {
            this.Scopes.RemoveAt(this.Scopes.Count - 1);
            this.ScopeNames.RemoveAt(this.ScopeNames.Count - 1);
            this.Objects.RemoveWhere((str) => {
                return str.StartsWith("+[" + this.Scopes.Count + "]") || str.StartsWith("-[" + this.Scopes.Count + "]");
            });
        }

        public bool ContainsObject(string name)
        {
            if (this.Scopes.Count > 0) {
                bool flag = false;
                for(int i = this.Scopes.Count - 1; i>=0; i--) {
                    flag = this.Scopes[i].Contains(name);
                    if (flag) return true;
                }

                if (!flag) return Objects.Contains(name);
                else return true;

            } else return Objects.Contains(name);
        }

        public void RegisterObject(string name)
        {
            if (this.Scopes.Count > 0) this.Scopes[this.Scopes.Count - 1].Add(name);
            else this.Objects.Add(name);
        }
    }

    public partial class Global
    {
        public class _undefined 
        {
            public override bool Equals(object? obj)
            {
                if (obj == null) return false;
                if (obj is _undefined) return true;
                return false;
            }
        }

        public static partial class GlobalExtension { }

        public static _undefined undef = new _undefined();

        public abstract class _class { public virtual dynamic _create() { return undef; } }

        public static UInt32 uint32(dynamic data)
        {
            return Convert.ToUInt32(data);
        }

        public static void alert(dynamic data)
        {
            System.Windows.MessageBox.Show(data.ToString());
        }
    }

    public static class Generation
    {
        public static string Indention(this GenerationContext ctx)
        {
            if (ctx.Indent == 0) return "";
            string indention = "";
            int count = 0;
            while(count < ctx.Indent * ctx.IndentionLevel) {
                indention += " ";
                count++;
            }

            return indention;
        }
    }
}
