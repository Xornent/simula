using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Token {

    public struct Position {
        public Position(int line, int column) {
            this.Line = line;
            this.Column = column;
        }

        public int Line { get; set; }
        public int Column { get; set; }
    }
}
