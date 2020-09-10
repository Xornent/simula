
#if !HAVE_TRACE_WRITER
using Simula.Scripting.Json.Serialization;

namespace Simula.Scripting.Json
{
    public enum TraceLevel
    {
        Off = 0,
        Error = 1,
        Warning = 2,
        Info = 3,
        Verbose = 4
    }
}

#endif