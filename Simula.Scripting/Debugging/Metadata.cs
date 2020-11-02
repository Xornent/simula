using Simula.Scripting.Compilation;
using Simula.Scripting.Reflection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Debugging {

    public struct Metadata {
        public Metadata(Member obj, RuntimeContext ctx) {
            ExecutionResult result = new ExecutionResult(obj, ctx);
            this.Pointer = result.Pointer;
            this.Type = result.Result.Type;
        }

        public Metadata(uint pointer, Reflection.MemberType type) {
            this.Pointer = pointer;
            this.Type = type;
        }

        public uint Pointer;
        public Reflection.MemberType Type;
    }
}
