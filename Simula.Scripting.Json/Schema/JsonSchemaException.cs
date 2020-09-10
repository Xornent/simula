
using System;
using System.Runtime.Serialization;

#nullable disable

namespace Simula.Scripting.Json.Schema
{
#if HAVE_BINARY_EXCEPTION_SERIALIZATION
    [Serializable]
#endif
    [Obsolete("JSON Schema validation has been moved to its own package. See https://www.newtonsoft.com/jsonschema for more details.")]
    public class JsonSchemaException : JsonException
    {
        public int LineNumber { get; }
        public int LinePosition { get; }
        public string Path { get; }
        public JsonSchemaException()
        {
        }
        public JsonSchemaException(string message)
            : base(message)
        {
        }
        public JsonSchemaException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

#if HAVE_BINARY_EXCEPTION_SERIALIZATION
        public JsonSchemaException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif

        internal JsonSchemaException(string message, Exception innerException, string path, int lineNumber, int linePosition)
            : base(message, innerException)
        {
            Path = path;
            LineNumber = lineNumber;
            LinePosition = linePosition;
        }
    }
}