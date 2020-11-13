using Simula.Scripting.Debugging;
using Simula.Scripting.Reflection.Markup;
using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Reflection {

    public class Reflect {}
    
    [Markup.Expose("object")]
    public class Member : Reflect {
        public string Name { get; internal set; } = "";
        public string FullName { get; internal set; } = "";
        public Locator ModuleHierarchy { get; internal set; } = new Locator(true);

        public Documentation Documentation { get; internal set; } = new Documentation();
        public Member? Conflict { get; set; } = null;
        public bool Compiled { get; internal set; } = false;
        public MemberType Type { get; set; } = MemberType.Unknown;

        public bool Readable { get; set; } = true;
        public bool Writable { get; set; } = true;
        public bool Restricted { get; set; } = false;

        public uint Handle { get; set; } = 0;

        [Expose("isCompiled")]
        public Type.Boolean isCompiled() {
            return this.Compiled;
        }

        [Expose("name")]
        public Type.String name(){
            return this.Name;
        }
        
        [Expose("type")]
        public Type.String type(){
            return this.Type.ToString();
        }

        [Expose("pointer")]
        public Type.Integer pointer(){
            return new Type.Integer(){ value = Convert.ToInt32(this.Handle) } ;
        }
        
        [Expose("moduleHierarchy")]
        public Type.String moduleHierarchy() {
            return this.ModuleHierarchy.JoinString(".");
        }

        [Expose("readable")]
        public Type.Boolean readable(){
            return this.Readable;
        }

        [Expose("writable")]
        public Type.Boolean writable() {
            return this.Writable;
        }
    }

    public interface ClrMember {
        object? GetNative();
    }

    public enum MemberType {
        Class,
        Instance,
        Function,
        Module,
        Unknown
    }
}
