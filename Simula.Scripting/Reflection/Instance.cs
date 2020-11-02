using Simula.Scripting.Compilation;
using Simula.Scripting.Debugging;
using Simula.Scripting.Reflection.Markup;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Simula.Scripting.Reflection {

    public class Instance : Member {
        public Instance(RuntimeContext? runtime) {
            this.Type = MemberType.Instance;
            this.Runtime = runtime;
        }

        public RuntimeContext? Runtime { get; internal set; }
        public Class? Parent { get; set; }
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

        public virtual ExecutionResult GetMember(string name) {
            if (this.Members.ContainsKey(name))
                if (this.Runtime != null)
                    return new ExecutionResult(this.Members[name].Pointer, this.Runtime);
            return new ExecutionResult();
        }

        public virtual bool SetMember(string name, Member value) {
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

    public class ClrInstance : Instance, ClrMember {
        public ClrInstance(RuntimeContext? context = null) : base(context) { }
        public ClrInstance(FieldInfo field, RuntimeContext? context = null) : base(context) {
            this.Reflection = field.GetValue(null);
            if (field.DeclaringType == null)
                this.Parent = null;
            else
                this.Parent = ClrClass.Create(field.DeclaringType.GetType());
        }

        public ClrInstance(object? field, RuntimeContext? context = null) : base(context) {
            this.Reflection = field;
            if (field == null)
                this.Parent = null;
            else
                this.Parent = ClrClass.Create(field.GetType());

            if (field == null) return;
            foreach (var item in field.GetType().GetMethods()) {
                if (item.IsStatic) continue;
                var attribute = item.GetCustomAttribute<ExposeAttribute>();
                if (attribute == null) continue;

                var funcs = new ClrFunction(item);
                funcs.Parent = this;
                funcs.Name = attribute.Alias;
                this.SetMember(attribute.Alias, funcs);
            }

            foreach (var item in field.GetType().GetFields()) {
                if (item.IsStatic) continue;
                var attribute = item.GetCustomAttribute<ExposeAttribute>();
                if (attribute == null) continue;

                var child = new ClrInstance(item);
                child.Name = attribute.Alias;
                this.SetMember(attribute.Alias, child);
            }
        }

        public ClrInstance(object field) : base(null) {
            this.Reflection = field;
        }

        public object? Reflection = null;
        public override ExecutionResult GetMember(string name) {
            return base.GetMember(name);
        }

        public override bool SetMember(string name, Member value) {
            return base.SetMember(name, value);
        }

        public object? GetNative() {
            return this.Reflection;
        }
    }
}
