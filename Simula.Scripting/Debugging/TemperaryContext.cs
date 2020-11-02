using Simula.Scripting.Debugging;
using Simula.Scripting.Reflection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Documents;

namespace Simula.Scripting.Compilation {
   
    public class TemperaryContext {
        public TemperaryContext(RuntimeContext parent) {
            this.Runtime = parent;
            if (this.Runtime.CallStack.Count>0)
                if (this.Runtime.CallStack.Peek() != this)
                    this.BaseTemperaryContent = this.Runtime.CallStack.Peek();
        }

        public string Name { get; set; }
        public RuntimeContext? Runtime { get; internal set; }
        public TemperaryContext? BaseTemperaryContent { get; internal set; }

        private Dictionary<string, Metadata> members = new Dictionary<string, Metadata>();
        public Dictionary<string, Metadata> Members {
            get {
                Dictionary<string, Metadata> results = new Dictionary<string, Metadata>(members);

                if(this.Runtime!=null) {
                    var defaults = new Locator(false);
                    defaults.Add("");
                    this.Usings.Add(defaults);
                    foreach (var usings in this.Usings) {
                        var use = usings.Locate(this.Runtime.PredefinedObjects, this.Runtime);
                        if(use != null) {
                            foreach (var item in ((Module)use).Members) {
                                if(results.ContainsKey(item.Key)) {
                                    var currentObj = this.Runtime.GetMemberByMetadata(item.Value);
                                    var overlapObj = this.Runtime.GetMemberByMetadata(results[item.Key]);
                                    if (overlapObj.Writable) {
                                        currentObj.Conflict = overlapObj;
                                        results[item.Key] = item.Value;
                                    }
                                } else results.Add(item.Key, item.Value);
                            }
                        }
                    }
                    this.Usings.Remove(defaults);
                }

                return results;
            }
        }

        internal bool IsDirty = true;
        public bool Permeable { get; set; } = true;

        private Dictionary<string, Class> classes = new Dictionary<string, Class>();
        private Dictionary<string, Instance> instances = new Dictionary<string, Instance>();
        private Dictionary<string, Function> functions = new Dictionary<string, Function>();
        private Dictionary<string, Module> modules = new Dictionary<string, Module>();

        public List<Locator> Usings = new List<Locator>();

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

        /// <summary>
        /// <para>
        /// 在当前的临时语境中新建或者获取一个模块, 给定一个模块的路径, 它是跟随在 module 语句之后的
        /// 字符串表达式, 如果不存在则传入一个未初始化的 <see cref="Locator"/> 对象. 这时, 它返回一个
        /// 默认的全局模块. 否则, 如果给定路径的任意一个部分不存在, 它都会新建这个路径.
        /// </para>
        /// 本函数返回 null 当且仅当 <see cref="Runtime"/> 参数为空值. 或者传入的参数名是当前域下已
        /// 定义的同名变量. 这意味着一个模块下不能定义与模块同名的变量.
        /// </summary>
        /// <returns></returns>
        public Module? AllocateMpdule(Locator locator) {
            IsDirty = true;
            if (this.Runtime != null) {
                if (this.Members.Count == 0) {
                    if (this.Members.ContainsKey(""))
                        return (Module)(this.Runtime.GetMemberByMetadata(this.Members[""]));
                    else {
                        Module mdl = new Module(this.Runtime);
                        mdl.Name = "";
                        ExecutionResult result = new ExecutionResult(mdl, this.Runtime);
                        this.members.Add("", new Metadata(result.Pointer, MemberType.Module));
                        return mdl;
                    }
                }

                Module? current = null;
                if (members.ContainsKey(locator[0])) {
                    current = (Module)(this.Runtime.GetMemberByMetadata(this.Members[locator[0]]));
                } else {
                    Module mdl = new Module(this.Runtime);
                    mdl.Name = locator[0];
                    ExecutionResult result = new ExecutionResult(mdl, this.Runtime);
                    this.members.Add(locator[0], new Metadata(result.Pointer, MemberType.Module));
                    current = mdl;
                }

                for (int i = 1; i < locator.Count; i++) {
                    string s = locator[i];
                    if (current.Members.ContainsKey(s)) {
                        try {
                            current = (Module)this.Runtime.GetMemberByMetadata(current.Members[s]);
                        } catch (InvalidCastException) { return null; }
                    } else {
                        Module mdl = new Module(this.Runtime);
                        mdl.Name = locator[i];
                        ExecutionResult result = new ExecutionResult(mdl, this.Runtime);
                        this.members.Add(locator[i], new Metadata(result.Pointer, MemberType.Module));
                        current = mdl;
                    }
                }

                return current;
            } else return null;
        }

        public void ClearDirtyState() {
            if (IsDirty && this.Runtime!=null) {
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
            if (this.BaseTemperaryContent != null && this.Permeable)
                return this.BaseTemperaryContent.GetMember(name);
            if(this.BaseTemperaryContent == null && this.Permeable) {
                if (this.Runtime == null) return new ExecutionResult();
                return this.Runtime.GetMember(name);
            }

            return new ExecutionResult();
        }

        public bool SetMember(string name, Member value) {
            if (this.Members.ContainsKey(name)) {
                if (this.GetMember(name).Result.Writable == false) return false;
                if (this.Runtime != null) {
                    value.Name = name;
                    this.Runtime.SetMemberByMetadata(this.Members[name], value);
                    this.IsDirty = true;
                    return true;
                }
            } else {
                if(this.Runtime != null) {
                    ExecutionResult result = new ExecutionResult(value, this.Runtime);
                    result.Result.Name = name;
                    this.members.Add(name, new Metadata(result.Pointer, result.Result.Type));
                    this.IsDirty = true;
                    return true;
                }
            }

            return false;
        }

        public bool SetMember(uint handle, Member value) {
            if (this.Runtime == null) return false;
            if( this.Runtime.Pointers.ContainsKey(handle) ) {
                this.Runtime.Pointers[handle] = value;
            }
            return false;
        }
    }
}
