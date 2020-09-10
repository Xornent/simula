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

        private List<Reflection.Module> modules = new List<Reflection.Module>();
        public List<Reflection.Module> Modules {
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

        private List<Reflection.Module> Generate() {
            List<Reflection.Module> mods = new List<Reflection.Module>();
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

        private List<Reflection.Module> modules = new List<Reflection.Module>();
        public List<Reflection.Module> Modules {
            get { if (modules.Count == 0) modules = Generate(); return modules; }
        }

        private List<Reflection.Module> Generate() {
            Dictionary<string, Reflection.Module> mods = new Dictionary<string, Reflection.Module>();
            Assembly asm = Assembly.LoadFrom(File);
            System.Type[] types = asm.GetTypes();

            foreach (var type in types) {
                if (type.FullName == null) continue;
                if (!type.FullName.StartsWith("Simula.Scripting.Type.")) continue;
                Reflection.Markup.ExposeAttribute? exposed = type.GetCustomAttribute<Reflection.Markup.ExposeAttribute>();
                if (exposed == null) continue;

                Type.Class classVar = new Type.Class();
                Reflection.AbstractClass classDef = new Reflection.AbstractClass();

                if (type.Namespace == null) continue;
                string moduleName = type.Namespace.Replace("Simula.Scripting.Type", "");
                if (moduleName.StartsWith(".")) moduleName = moduleName.Remove(0, 1);

                classDef.Compiled = true;
                if (moduleName == "") classDef.FullName = exposed.Alias;
                else classDef.FullName = moduleName + "." + exposed.Alias;
                
                // 在 Simula.Scripting.Type 命名空间下定义的类是不属于任何包的.
                // 它所对应的 Module 名为 "".

                if (moduleName == "") {
                    if (!mods.ContainsKey("")) mods.Add("", new Reflection.Module());

                }
            }

            return mods.Values.ToList();
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
        public static List<string> GetModuleHirachy(string s) {
            if (s == "") return new List<string>();
            if (s.StartsWith("Simula.Scripting.Type.")) s = s.Replace("Simula.Scripting.Type.", "");
            if (s.StartsWith("Simula.Scripting.Type")) s = s.Replace("Simula.Scripting.Type", "");
            if (s.Contains(".")) {
                return s.Split('.').ToList();
            } else return new List<string>() { s };
        }
    }
}
