using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
#if !HAVE_LINQ
using Simula.Scripting.Json.Utilities.LinqBridge;
#else
using System.Linq;

#endif

namespace Simula.Scripting.Json.Schema
{
    [Obsolete("JSON Schema validation has been moved to its own package. See http://www.newtonsoft.com/jsonschema for more details.")]
    internal class JsonSchemaNode
    {
        public string Id { get; }
        public ReadOnlyCollection<JsonSchema> Schemas { get; }
        public Dictionary<string, JsonSchemaNode> Properties { get; }
        public Dictionary<string, JsonSchemaNode> PatternProperties { get; }
        public List<JsonSchemaNode> Items { get; }
        public JsonSchemaNode AdditionalProperties { get; set; }
        public JsonSchemaNode AdditionalItems { get; set; }

        public JsonSchemaNode(JsonSchema schema)
        {
            Schemas = new ReadOnlyCollection<JsonSchema>(new[] { schema });
            Properties = new Dictionary<string, JsonSchemaNode>();
            PatternProperties = new Dictionary<string, JsonSchemaNode>();
            Items = new List<JsonSchemaNode>();

            Id = GetId(Schemas);
        }

        private JsonSchemaNode(JsonSchemaNode source, JsonSchema schema)
        {
            Schemas = new ReadOnlyCollection<JsonSchema>(source.Schemas.Union(new[] { schema }).ToList());
            Properties = new Dictionary<string, JsonSchemaNode>(source.Properties);
            PatternProperties = new Dictionary<string, JsonSchemaNode>(source.PatternProperties);
            Items = new List<JsonSchemaNode>(source.Items);
            AdditionalProperties = source.AdditionalProperties;
            AdditionalItems = source.AdditionalItems;

            Id = GetId(Schemas);
        }

        public JsonSchemaNode Combine(JsonSchema schema)
        {
            return new JsonSchemaNode(this, schema);
        }

        public static string GetId(IEnumerable<JsonSchema> schemata)
        {
            return string.Join("-", schemata.Select(s => s.InternalId).OrderBy(id => id, StringComparer.Ordinal)
#if !HAVE_STRING_JOIN_WITH_ENUMERABLE
                    .ToArray()
#endif
                );
        }
    }
}