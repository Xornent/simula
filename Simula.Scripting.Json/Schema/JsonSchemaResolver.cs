
using System;
using System.Collections.Generic;
#if !HAVE_LINQ
using Simula.Scripting.Json.Utilities.LinqBridge;
#else
using System.Linq;

#endif

#nullable disable

namespace Simula.Scripting.Json.Schema
{
    [Obsolete("JSON Schema validation has been moved to its own package. See https://www.newtonsoft.com/jsonschema for more details.")]
    public class JsonSchemaResolver
    {
        public IList<JsonSchema> LoadedSchemas { get; protected set; }
        public JsonSchemaResolver()
        {
            LoadedSchemas = new List<JsonSchema>();
        }
        public virtual JsonSchema GetSchema(string reference)
        {
            JsonSchema schema = LoadedSchemas.SingleOrDefault(s => string.Equals(s.Id, reference, StringComparison.Ordinal));

            if (schema == null)
            {
                schema = LoadedSchemas.SingleOrDefault(s => string.Equals(s.Location, reference, StringComparison.Ordinal));
            }

            return schema;
        }
    }
}