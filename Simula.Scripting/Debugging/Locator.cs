using Simula.Scripting.Compilation;
using Simula.Scripting.Reflection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Debugging {

    public class Locator : List<string> {
        public Locator(bool initialized): base() {
            if (initialized)
                this.Add("<callstack>");
        }

        public Module? Locate(Dictionary<string, Member> members, RuntimeContext ctx) {
            if (this.Count == 0) {
                if (members.ContainsKey(""))
                    return (Module)members[""];
                else return null;
            } 

            Module? current = null;
            if (members.ContainsKey(this[0])) {
                current = (Module)members[this[0]];
            } else return null;

            for(int i = 1; i < this.Count; i++) {
                string s = this[i];
                if (current.Members.ContainsKey(s)) {
                    try {
                        current = (Module)ctx.GetMemberByMetadata(current.Members[s]);
                    } catch (InvalidCastException) { return null; }
                } else return null;
            }

            return current;
        }

        public Module? Locate(Dictionary<string, Metadata> members, RuntimeContext ctx) {
            if (this.Count == 0) {
                if (members.ContainsKey(""))
                    return (Module)ctx.GetMemberByMetadata( members[""]);
                else return null;
            }

            Module? current = null;
            if (members.ContainsKey(this[0])) {
                current = (Module)ctx.GetMemberByMetadata(members[this[0]]);
            } else return null;

            for (int i = 1; i < this.Count; i++) {
                string s = this[i];
                if (current.Members.ContainsKey(s)) {
                    try {
                        current = (Module)ctx.GetMemberByMetadata(current.Members[s]);
                    } catch (InvalidCastException) { return null; }
                } else return null;
            }

            return current;
        }
    }
}
