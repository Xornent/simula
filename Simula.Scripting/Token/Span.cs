using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Token {

    public struct Span {
        public Span(Position start, Position end) {
            this.Start = start;
            this.End = end;
        }

        public Position Start { get; set; }
        public Position End { get; set; }
    }
}
