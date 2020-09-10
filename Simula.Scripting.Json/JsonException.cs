
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text;
using Simula.Scripting.Json.Utilities;

namespace Simula.Scripting.Json
{
#if HAVE_BINARY_EXCEPTION_SERIALIZATION
    [Serializable]
#endif
    public class JsonException : Exception
    {
        public JsonException()
        {
        }
        public JsonException(string message)
            : base(message)
        {
        }
        public JsonException(string message, Exception? innerException)
            : base(message, innerException)
        {
        }

#if HAVE_BINARY_EXCEPTION_SERIALIZATION
        public JsonException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif

        internal static JsonException Create(IJsonLineInfo lineInfo, string path, string message)
        {
            message = JsonPosition.FormatMessage(lineInfo, path, message);

            return new JsonException(message);
        }
    }
}