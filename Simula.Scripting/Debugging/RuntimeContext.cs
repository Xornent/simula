using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Simula.Scripting.Reflection;
using Simula.Scripting.Debugging;
using Simula.Scripting.Type;

namespace Simula.Scripting.Compilation {

    public class RuntimeContext {
        public RuntimeContext() {
            Pointers.Add(1, ClrClass.Create(typeof(Type.NullType)));
            Pointers.Add(0, new ClrInstance(Type.Global.Null, this));

            Pointers.Add(2, ClrClass.Create(typeof(Type.Boolean)));
            Pointers.Add(3, new ClrInstance(Type.Global.True, this));
            Pointers.Add(4, new ClrInstance(Type.Global.False, this));

            Pointers.Add(5, ClrClass.Create(typeof(Type.Integer)));
            Pointers.Add(6, ClrClass.Create(typeof(Type.Float)));
            Pointers.Add(7, ClrClass.Create(typeof(Type.String)));
            Pointers.Add(8, ClrClass.Create(typeof(Type.Dimension)));
            Pointers.Add(9, ClrClass.Create(typeof(Type._Class)));
            Pointers.Add(10, new ClrFunction(typeof(Type.Global).GetMethod("Alert")));
        }

        // 对于已编译的对象 (从 LiraryCompilationUnit 注册, 后缀名 .scl Simula 已编译库)
        // 对于动态的对象 (从 ObjectCompilationUnit 注册, 后缀名 .sop Simula 对象包 或 
        // SourceCompilationUnit 注册, 后缀名为 .s Simula 源文件) 

        public List<CompilationUnit> Registry = new List<CompilationUnit>();

        public uint MaximumAllocatedPointer = 1000;
        public Dictionary<uint, Member> Pointers = new Dictionary<uint, Member>();

        public Member AllocateMember(Member member, uint pointer = 0) {
            if (member.Handle != 0) {
                if (Pointers.ContainsKey(member.Handle))
                    Pointers.Remove(member.Handle);
            }

            if (pointer != 0) {
                member.Handle = pointer;
                if (this.Pointers.ContainsKey(pointer))
                    this.Pointers[pointer] = member;
                else {
                    this.Pointers.Add(pointer, member);
                }
            } else {
                this.Pointers.Add(MaximumAllocatedPointer, member);
                member.Handle = MaximumAllocatedPointer;
                MaximumAllocatedPointer++;
            }

            return member;
        }

        public Dictionary<string, Metadata> PredefinedObjects = new System.Collections.Generic.Dictionary<string, Metadata>()
        {
            { "null", new Metadata(0, MemberType.Instance) },
            { "__nulltype__", new Metadata(1, MemberType.Class) },
            { "bool", new Metadata(2, MemberType.Class) },
            { "true", new Metadata(3, MemberType.Instance) },
            { "false", new Metadata(4, MemberType.Instance) },
            { "int", new Metadata(5, MemberType.Class) },
            { "float", new Metadata(6, MemberType.Class) },
            { "string", new Metadata(7, MemberType.Class) },
            { "dimension", new Metadata(8, MemberType.Class) },
            { "class", new Metadata(9, MemberType.Class)},
            { "alert", new Metadata(10, MemberType.Function) }
        };

        public List<RuntimeError> Errors = new List<RuntimeError>();
        public Stack<TemperaryContext> CallStack = new Stack<TemperaryContext>();

        // 在此处, 或者是任意一个返回值的 Statement 中, 返回的值是 ExecutionResult . 即如果遇到所有的异常, 
        // 统一返回 Null. 反之, 则返回的如下类型中的一种:

        // 1.  Reflection.Class 如果对象是一个类
        // 3.  Reflection.Instance 如果对象是一个未编译的实例
        // 4.  Reflection.Module 如果对象是一个模块或子模块
        // 5.  Reflection.Clr* 如果对象是一个已编译的命名语言内对象(包括抽象类, 函数, 和基础类型)

        public Member GetMemberByMetadata(Metadata meta) {
            if (this.Pointers.ContainsKey(meta.Pointer))
                return this.Pointers[meta.Pointer];
            else return Pointers[0];
        }

        public bool SetMemberByMetadata(Metadata meta, Member value) {
            if (this.Pointers.ContainsKey(meta.Pointer)) {
                var overlap = GetMemberByMetadata(meta);
                if (overlap.Writable) {
                    value.Conflict = overlap;
                    value.Name = overlap.Name;
                    this.Pointers[meta.Pointer] = value;
                }
                return true;
            } else {
                this.Pointers.Add(meta.Pointer, value);
                return true;
            }
        }

        /// <summary>
        /// 本函数返回 null 当且仅当 <see cref="Runtime"/> 参数为空值. 或者传入的参数名是当前域下已
        /// 定义的同名变量. 这意味着一个模块下不能定义与模块同名的变量.
        /// </summary>
        /// <returns></returns>
        public Module? AllocateMpdule(Locator locator) {
            if (this != null) {
                if (this.PredefinedObjects.Count == 0) {
                    if (this.PredefinedObjects.ContainsKey(""))
                        return (Module)(this.GetMemberByMetadata(this.PredefinedObjects[""]));
                    else {
                        Module mdl = new Module(this);
                        mdl.Name = "";
                        ExecutionResult result = new ExecutionResult(mdl, this);
                        this.PredefinedObjects.Add("", new Metadata(result.Pointer, MemberType.Module));
                        return mdl;
                    }
                }

                Module? current = null;
                if (PredefinedObjects.ContainsKey(locator[0])) {
                    current = (Module)(this.GetMemberByMetadata(this.PredefinedObjects[locator[0]]));
                } else {
                    Module mdl = new Module(this);
                    mdl.Name = locator[0];
                    ExecutionResult result = new ExecutionResult(mdl, this);
                    this.PredefinedObjects.Add(locator[0], new Metadata(result.Pointer, MemberType.Module));
                    current = mdl;
                }

                for (int i = 1; i < locator.Count; i++) {
                    string s = locator[i];
                    if (current.Members.ContainsKey(s)) {
                        try {
                            current = (Module)this.GetMemberByMetadata(current.Members[s]);
                        } catch (InvalidCastException) { return null; }
                    } else {
                        Module mdl = new Module(this);
                        mdl.Name = locator[i];
                        ExecutionResult result = new ExecutionResult(mdl, this);
                        this.PredefinedObjects.Add(locator[i], new Metadata(result.Pointer, MemberType.Module));
                        current = mdl;
                    }
                }

                return current;
            } else return null;
        }

        public ExecutionResult GetMember(string name) {
            if(this.PredefinedObjects.ContainsKey(name)) 
                return new ExecutionResult(this.PredefinedObjects[name].Pointer, this);
            if(this.PredefinedObjects.ContainsKey("")) {
                var member = this.GetMemberByMetadata(this.PredefinedObjects[""]);
                if(member is Module module) {
                    if (module.Members.ContainsKey(name))
                        return new ExecutionResult(module.Members[name].Pointer, this);
                }
            }

            return new ExecutionResult();
        }
    }
}
