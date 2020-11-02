using Simula.Scripting.Token;
using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Debugging {

    public class RuntimeError {
        public RuntimeError(int index, string msg) {
            this.Index = index;
            this.Message = msg;
        }

        public RuntimeError(int index, string msg, Span? location) {
            this.Index = index;
            this.Message = msg;
            this.Location = location;
        }

        public string Message { get; set; }
        public int Index { get; set; }
        public Span? Location { get; set; }
    }
}
