using Simula.Scripting.Debugging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Reflection {
    
    [Markup.Expose("object")]
    public class Member {
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
