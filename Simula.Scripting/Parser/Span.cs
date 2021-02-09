using System;

namespace Simula.Scripting.Parser
{
    public class Span
    {
        public Span(Position start, Position end)
        {
            this.Start = start;
            this.End = end;
        }

        public Span(int lineStart, int columnStart, int lineEnd, int columnEnd)
        {
            this.Start = new Position(lineStart, columnStart);
            this.End = new Position(lineEnd, columnEnd);
        }

        public Position Start { get; set; }
        public Position End { get; set; }
    }
}
