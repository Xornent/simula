using System;

namespace Simula.Scripting.Json.Serialization
{
    public interface ITraceWriter
    {
        TraceLevel LevelFilter { get; }
        void Trace(TraceLevel level, string message, Exception? ex);
    }
}