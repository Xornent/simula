using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Simula.Scripting.Types
{
    public class Addr : Var
    {
        internal GCHandle handle = new GCHandle();
        public Addr() : base() { }

        public Addr(GCHandle h) : base()
        {
            this.handle = h;
        }

        public Addr(dynamic obj) : base()
        {
            this.handle = GCHandle.Alloc(obj, GCHandleType.Normal);
        }

        ~Addr()
        {
            if (this.handle.IsAllocated)
                this.handle.Free();
        }

        public static Function _deref = new Function((self, args) => {
            return self.handle.Target;
        }, new List<Pair>() { }, "any");

        internal new string type = "sys.addr";

        public override string ToString()
        {
            if (this.handle.IsAllocated)
                return "<handle>";
            else return "<empty handle>";
        }
    }
}
