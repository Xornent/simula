
using System;

namespace Simula.Scripting.Json.Serialization
{
    public class ErrorContext
    {
        internal ErrorContext(object? originalObject, object? member, string path, Exception error)
        {
            OriginalObject = originalObject;
            Member = member;
            Error = error;
            Path = path;
        }

        internal bool Traced { get; set; }
        public Exception Error { get; }
        public object? OriginalObject { get; }
        public object? Member { get; }
        public string Path { get; }
        public bool Handled { get; set; }
    }
}