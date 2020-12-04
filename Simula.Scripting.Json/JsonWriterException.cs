
using System;

namespace Simula.Scripting.Json
{
#if HAVE_BINARY_EXCEPTION_SERIALIZATION
    [Serializable]
#endif
    public class JsonWriterException : JsonException
    {
        public string? Path { get; }
        public JsonWriterException()
        {
        }
        public JsonWriterException(string message)
            : base(message)
        {
        }
        public JsonWriterException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

#if HAVE_BINARY_EXCEPTION_SERIALIZATION
        public JsonWriterException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif
        public JsonWriterException(string message, string path, Exception? innerException)
            : base(message, innerException)
        {
            Path = path;
        }

        internal static JsonWriterException Create(JsonWriter writer, string message, Exception? ex)
        {
            return Create(writer.ContainerPath, message, ex);
        }

        internal static JsonWriterException Create(string path, string message, Exception? ex)
        {
            message = JsonPosition.FormatMessage(null, path, message);

            return new JsonWriterException(message, path, ex);
        }
    }
}