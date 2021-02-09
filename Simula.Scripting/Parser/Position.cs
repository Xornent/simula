using System;

namespace Simula.Scripting.Parser
{
    public class Position
    {
        public Position(int line, int column)
        {
            this.Line = line;
            this.Column = column;
        }

        public int Line { get; set; }
        public int Column { get; set; }
    }
}
