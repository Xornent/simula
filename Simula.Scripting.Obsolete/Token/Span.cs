namespace Simula.Scripting.Token
{
    public struct Span
    {
        public Span(Position start, Position end)
        {
            Start = start;
            End = end;
        }

        public Position Start { get; set; }
        public Position End { get; set; }
    }
}
