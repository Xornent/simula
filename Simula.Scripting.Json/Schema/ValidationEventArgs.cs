
using Simula.Scripting.Json.Utilities;
using System;

#nullable disable

namespace Simula.Scripting.Json.Schema
{
    [Obsolete("JSON Schema validation has been moved to its own package. See https://www.newtonsoft.com/jsonschema for more details.")]
    public class ValidationEventArgs : EventArgs
    {
        private readonly JsonSchemaException _ex;

        internal ValidationEventArgs(JsonSchemaException ex)
        {
            ValidationUtils.ArgumentNotNull(ex, nameof(ex));
            _ex = ex;
        }
        public JsonSchemaException Exception => _ex;
        public string Path => _ex.Path;
        public string Message => _ex.Message;
    }
}