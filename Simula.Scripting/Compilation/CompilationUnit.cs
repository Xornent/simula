using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Simula.Scripting.Syntax;
using System.Linq;
using System.Reflection;

namespace Simula.Scripting.Compilation {

    public abstract class CompilationUnit {
        public abstract void Register(RuntimeContext ctx);
        public abstract void Unregister(RuntimeContext ctx);
    }

    public class SourceCompilationUnit : CompilationUnit {

        public string Content { get; private set; } = "";
        public string File { get; private set; } = "";

        private BlockStatement? body = null;
        public BlockStatement Body {
            get { if (body == null) body = Parse(); return body; }
        }

        private Dictionary<string, Reflection.Module> modules = new Dictionary<string, Reflection.Module>();
        public Dictionary<string, Reflection.Module> Modules {
            get { if (modules.Count == 0) modules = Generate(); return modules; }
        }

        public SourceCompilationUnit(string path, FileMode mode) {
            this.File = path;
            FileStream stream = new FileStream(path, mode);
            StreamReader reader = new StreamReader(stream);
            this.Content = reader.ReadToEnd();
            reader.Close();
            stream.Close();
        }

        public SourceCompilationUnit(string content) {
            this.Content = content;
        }

        private BlockStatement Parse() {
            BlockStatement block = new BlockStatement();
            Token.TokenDocument doc = new Token.TokenDocument();
            doc.Tokenize(this.Content);
            block.Parse(doc.Tokens);
            return block;
        }

        private Dictionary<string, Reflection.Module> Generate() {
            Dictionary<string, Reflection.Module> mods = new Dictionary<string, Reflection.Module>();
            return mods;
        }

        public override void Register(RuntimeContext ctx) {
            
        }

        public override void Unregister(RuntimeContext ctx) {

        }
    }

    public class LibraryCompilationUnit : CompilationUnit {
        
        public string File { get; private set; } = "";

        public LibraryCompilationUnit(string path, FileMode mode) {
            this.File = path;
        }

        private Dictionary<string, Reflection.Module> modules = new Dictionary<string, Reflection.Module>();
        public Dictionary<string, Reflection.Module> Modules {
            get { if (modules.Count == 0) modules = Generate(); return modules; }
        }

        private Dictionary<string, Reflection.Module> Generate() {
            Dictionary<string, Reflection.Module> mods = new Dictionary<string, Reflection.Module>();
            Assembly asm = Assembly.LoadFrom(File);
            System.Type[] types = asm.GetTypes();

            foreach (var type in types) {
                if (type.FullName == null) continue;
                
                // 从已编译的程序集中加载所有类, 筛选出所有用 ExposeAttribute 标识的类.

            }

            return mods;
        }

        public override void Register(RuntimeContext ctx) {
            
        }

        public override void Unregister(RuntimeContext ctx) {

        }
    }

    public class ObjectCompilationUnit : CompilationUnit {

        public string File { get; private set; } = "";

        public ObjectCompilationUnit(string path, FileMode mode) {
            this.File = path;
        }

        private List<Reflection.Module> modules = new List<Reflection.Module>();
        public List<Reflection.Module> Modules {
            get { if (modules.Count == 0) modules = Generate(); return modules; }
        }

        private List<Reflection.Module> Generate() {
            List<Reflection.Module> mods = new List<Reflection.Module>();
            return mods;
        }

        public override void Register(RuntimeContext ctx) {

        }

        public override void Unregister(RuntimeContext ctx) {

        }
    }

    public static class Utilities {

        public static string Connect(this List<string> list, string connection) {
            if (list.Count == 0) return "";
            string s = list[0];
            for(int i = 1; i<list.Count;i++) {
                s = s + connection + list[i];
            }
            return s;
        }

        public static List<string> GetModuleHirachy(string? s) {
            if (string.IsNullOrEmpty(s)) return new List<string>() { "" };
            if (s.StartsWith("Simula.Scripting.Type.")) s = s.Replace("Simula.Scripting.Type.", "");
            if (s.StartsWith("Simula.Scripting.Type")) s = s.Replace("Simula.Scripting.Type", "");
            if (s.Contains(".")) {
                return s.Split('.').ToList();
            } else return new List<string>() { s };
        }

        public static List<System.Type> GetAccessableClassType(Assembly asm) {
            List<System.Type> types = new List<System.Type>();
            foreach(var type in asm.GetTypes()) {
                Reflection.Markup.ExposeAttribute? expose = type.GetCustomAttribute<Reflection.Markup.ExposeAttribute>();
                if(expose != null) {
                    if (expose.Alias == "<global>") continue;
                    types.Add(type);
                }
            }
            return types;
        }

        public static List<Reflection.Variable> GetAccessableClassVariable(Assembly asm) {
            List<Reflection.Variable> classes = new List<Reflection.Variable>();
            foreach (var type in asm.GetTypes()) {
                Reflection.Markup.ExposeAttribute? expose = type.GetCustomAttribute<Reflection.Markup.ExposeAttribute>();
                if (expose != null) {
                    if (expose.Alias == "<global>") continue;
                    Reflection.Variable var = new Reflection.Variable();
                    var.FullName = GetModuleHirachy(type.Namespace).Connect(".");
                    if (var.FullName == "")
                        var.FullName += expose.Alias;
                    else var.FullName += ("." + expose.Alias);
                    var.Name = expose.Alias;
                    var.Hidden = expose.ToSystemOnly;
                    var.Object = new Type.Class(type);
                    classes.Add(var);
                }
            }
            return classes;
        }

        public static void SetVariable(List<string> module, Reflection.Variable variable, Dictionary<string, Reflection.Module> destinationArray) {
            Reflection.Module? current = null;
            foreach (var submod in module) {
                if(current == null) {
                    if (destinationArray.ContainsKey(submod))
                        current = destinationArray[submod];
                    else {
                        current = new Reflection.Module();
                        current.Name = submod;
                        current.Compiled = true;
                        destinationArray.Add(submod, current);
                    }
                } else if(current.SubModules.ContainsKey(submod)) {
                    current = current.SubModules[submod];
                } else {
                    var i = new Reflection.Module();
                    i.Name = submod;
                    i.Compiled = true;
                    current.SubModules.Add(submod, i);
                    current = i;
                }
            }

            if (current.Variables.ContainsKey(variable.Name)) {
                variable.Conflict = current.Variables[variable.Name];
                current.Variables[variable.Name] = variable;
            } else
                current.Variables.Add(variable.Name, variable);
        }

        public static void RemoveVariable(List<string> module, Reflection.Variable variable, Dictionary<string, Reflection.Module> destinationArray) {
            Reflection.Module? current = null;
            foreach (var submod in module) {
                if (current == null) {
                    if (destinationArray.ContainsKey(submod))
                        current = destinationArray[submod];
                    else return;
                } else if (current.SubModules.ContainsKey(submod)) {
                    current = current.SubModules[submod];
                } else return;
            }

            if (current.Variables.ContainsKey(variable.Name)) {
                Reflection.Variable varia = current.Variables[variable.Name];
                while(varia != variable) {
                    if (varia.Conflict == null) return;
                    varia = varia.Conflict;

                    if(varia.Conflict == variable) {
                        varia.Conflict = variable.Conflict;
                        return;
                    }
                }
            } else return;
        }

    }
}
