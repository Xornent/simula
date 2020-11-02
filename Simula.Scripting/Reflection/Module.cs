using Simula.Scripting.Compilation;
using Simula.Scripting.Debugging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Reflection {

    public class Module : Member {
        public Module(RuntimeContext runtime) {
            this.Type = MemberType.Module;
            this.Runtime = runtime;
        }
            
        public RuntimeContext? Runtime { get; internal set; }
    
        public List<Syntax.Statement> Startup = new List<Syntax.Statement>();
        public Locator Path = new Locator(true);
        public Dictionary<string, Metadata> Members = new Dictionary<string, Metadata>();

        internal bool IsDirty = true;

        private Dictionary<string, Class> classes = new Dictionary<string, Class>();
        private Dictionary<string, Instance> instances = new Dictionary<string, Instance>();
        private Dictionary<string, Function> functions = new Dictionary<string, Function>();
        private Dictionary<string, Module> modules = new Dictionary<string, Module>();

        public Dictionary<string, Class> Classes {
            get {
                if (IsDirty) ClearDirtyState();
                return this.classes;
            }
        }

        public Dictionary<string, Instance> Instances {
            get {
                if (IsDirty) ClearDirtyState();
                return this.instances;
            }
        }

        public Dictionary<string, Function> Functions {
            get {
                if (IsDirty) ClearDirtyState();
                return this.functions;
            }
        }

        public Dictionary<string, Module> Modules {
            get {
                if (IsDirty) ClearDirtyState();
                return this.modules;
            }
        }

        public void ClearDirtyState() {
            if (IsDirty && this.Runtime != null) {
                Dictionary<string, Module> mods = new Dictionary<string, Module>();
                foreach (var item in Members) {
                    if (item.Value.Type == MemberType.Module)
                        mods.Add(item.Key, (Module)this.Runtime.GetMemberByMetadata(this.Members[item.Key]));
                }
                this.modules = mods;

                Dictionary<string, Function> func = new Dictionary<string, Function>();
                foreach (var item in Members) {
                    if (item.Value.Type == MemberType.Function)
                        func.Add(item.Key, (Function)this.Runtime.GetMemberByMetadata(this.Members[item.Key]));
                }
                this.functions = func;

                Dictionary<string, Instance> inst = new Dictionary<string, Instance>();
                foreach (var item in Members) {
                    if (item.Value.Type == MemberType.Instance)
                        inst.Add(item.Key, (Instance)this.Runtime.GetMemberByMetadata(this.Members[item.Key]));
                }
                this.instances = inst;

                Dictionary<string, Class> cls = new Dictionary<string, Class>();
                foreach (var item in Members) {
                    if (item.Value.Type == MemberType.Class)
                        cls.Add(item.Key, (Class)this.Runtime.GetMemberByMetadata(this.Members[item.Key]));
                }
                this.classes = cls;

                this.IsDirty = false;
            }
        }

        public ExecutionResult GetMember(string name) {
            if (this.Members.ContainsKey(name))
                if (this.Runtime != null)
                    return new ExecutionResult(this.Members[name].Pointer, this.Runtime);
            return new ExecutionResult();
        }

        public bool SetMember(string name, Member value) {
            if (this.Members.ContainsKey(name)) {
                if (this.GetMember(name).Result.Writable == false) return false;
                if (this.Runtime != null) {
                    this.Runtime.SetMemberByMetadata(this.Members[name], value);
                    this.IsDirty = true;
                    return true;
                }
            } else {
                if (this.Runtime != null) {
                    ExecutionResult result = new ExecutionResult(value, this.Runtime);
                    this.Members.Add(name, new Metadata(result.Pointer, result.Result.Type));
                    this.IsDirty = true;
                    return true;
                }
            }

            return false;
        }
    }
}
