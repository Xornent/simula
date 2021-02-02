using Simula.Scripting.Syntax;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Simula.Scripting.Build
{
    public class GenerationContext : Contexts.DynamicRuntime
    {
        public int Indent = 4;
        public int IndentionLevel = 0;

        // the representation form of object locator:
        // some examples of object locator (Objects.Key) is as follows:

        //     +[0]a

        // regex: [\-\+]\[[0-9]+\] ...

        // the object registry is set to determine when the generator chooses to initialize a 
        // object and when to call a object. it is a named list for object existed. and the priority
        // precedence is shown as follows: (if we want to find a named 'a')

        public HashSet<string> Objects = new HashSet<string>();
        public string DefinerName = "";

        public void PushScope(string name)
        {
            this.Scopes.Add(new Contexts.ScopeContext());
        }

        public void PopScope()
        {
            this.Scopes.RemoveAt(this.Scopes.Count - 1);
            this.Objects.RemoveWhere((str) => {
                return str.StartsWith("+[" + this.Scopes.Count + "]") || str.StartsWith("-[" + this.Scopes.Count + "]");
            });
        }

        public bool ContainsObject(string name)
        {
            Regex reg = new Regex(@"[\-\+]\[[0 - 9]+\]");
            foreach (var item in this.Objects) {
                string identifer = reg.Replace(item, "");
                if (identifer == name) return true;
            }

            return false;
        }
    }

    public class Global
    {
        public class _undefined { }
        public static _undefined undef = new _undefined();

        public abstract class _class { public virtual dynamic _create() { return undef; } }

        public dynamic global = new System.Dynamic.ExpandoObject();
        public List<dynamic> scopes = new List<dynamic>();

        public void pushscope()
        {
            dynamic exp = new System.Dynamic.ExpandoObject();
            exp.fullName = new List<string> { "" };
            scopes.Add(exp);
        }

        public void popscope()
        {
            scopes.RemoveAt(scopes.Count - 1);
        }

        public UInt32 uint32(dynamic self, dynamic[] args)
        {
            return Convert.ToUInt32(args[0]);
        }

        public void alert(dynamic self, dynamic[] args)
        {
            System.Windows.MessageBox.Show(args[0].ToString());
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
