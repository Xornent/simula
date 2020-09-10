#if HAVE_TRACE_WRITER
using System;
using System.Diagnostics;
using DiagnosticsTrace = System.Diagnostics.Trace;

namespace Simula.Scripting.Json.Serialization
{
    public class DiagnosticsTraceWriter : ITraceWriter
    {
        public TraceLevel LevelFilter { get; set; }

        private TraceEventType GetTraceEventType(TraceLevel level)
        {
            switch (level)
            {
                case TraceLevel.Error:
                    return TraceEventType.Error;
                case TraceLevel.Warning:
                    return TraceEventType.Warning;
                case TraceLevel.Info:
                    return TraceEventType.Information;
                case TraceLevel.Verbose:
                    return TraceEventType.Verbose;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level));
            }
        }
        public void Trace(TraceLevel level, string message, Exception? ex)
        {
            if (level == TraceLevel.Off)
            {
                return;
            }

            TraceEventCache eventCache = new TraceEventCache();
            TraceEventType traceEventType = GetTraceEventType(level);

            foreach (TraceListener listener in DiagnosticsTrace.Listeners)
            {
                if (!listener.IsThreadSafe)
                {
                    lock (listener)
                    {
                        listener.TraceEvent(eventCache, "Simula.Scripting.Json", traceEventType, 0, message);
                    }
                }
                else
                {
                    listener.TraceEvent(eventCache, "Simula.Scripting.Json", traceEventType, 0, message);
                }

                if (DiagnosticsTrace.AutoFlush)
                {
                    listener.Flush();
                }
            }
        }
    }
}

#endif