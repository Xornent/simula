using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Simula.Scripting.Json.Serialization
{
    public class MemoryTraceWriter : ITraceWriter
    {
        private readonly Queue<string> _traceMessages;
        private readonly object _lock;
        public TraceLevel LevelFilter { get; set; }
        public MemoryTraceWriter()
        {
            LevelFilter = TraceLevel.Verbose;
            _traceMessages = new Queue<string>();
            _lock = new object();
        }
        public void Trace(TraceLevel level, string message, Exception? ex)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff", CultureInfo.InvariantCulture));
            sb.Append(" ");
            sb.Append(level.ToString("g"));
            sb.Append(" ");
            sb.Append(message);

            string s = sb.ToString();

            lock (_lock) {
                if (_traceMessages.Count >= 1000) {
                    _traceMessages.Dequeue();
                }

                _traceMessages.Enqueue(s);
            }
        }
        public IEnumerable<string> GetTraceMessages()
        {
            return _traceMessages;
        }
        public override string ToString()
        {
            lock (_lock) {
                StringBuilder sb = new StringBuilder();
                foreach (string traceMessage in _traceMessages) {
                    if (sb.Length > 0) {
                        sb.AppendLine();
                    }

                    sb.Append(traceMessage);
                }

                return sb.ToString();
            }
        }
    }
}