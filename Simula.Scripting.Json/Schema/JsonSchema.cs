
using Simula.Scripting.Json.Linq;
using Simula.Scripting.Json.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

#nullable disable

namespace Simula.Scripting.Json.Schema
{
    [Obsolete("JSON Schema validation has been moved to its own package. See https://www.newtonsoft.com/jsonschema for more details.")]
    public class JsonSchema
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public bool? Required { get; set; }
        public bool? ReadOnly { get; set; }
        public bool? Hidden { get; set; }
        public bool? Transient { get; set; }
        public string Description { get; set; }
        public JsonSchemaType? Type { get; set; }
        public string Pattern { get; set; }
        public int? MinimumLength { get; set; }
        public int? MaximumLength { get; set; }
        public double? DivisibleBy { get; set; }
        public double? Minimum { get; set; }
        public double? Maximum { get; set; }
        public bool? ExclusiveMinimum { get; set; }
        public bool? ExclusiveMaximum { get; set; }
        public int? MinimumItems { get; set; }
        public int? MaximumItems { get; set; }
        public IList<JsonSchema> Items { get; set; }
        public bool PositionalItemsValidation { get; set; }
        public JsonSchema AdditionalItems { get; set; }
        public bool AllowAdditionalItems { get; set; }
        public bool UniqueItems { get; set; }
        public IDictionary<string, JsonSchema> Properties { get; set; }
        public JsonSchema AdditionalProperties { get; set; }
        public IDictionary<string, JsonSchema> PatternProperties { get; set; }
        public bool AllowAdditionalProperties { get; set; }
        public string Requires { get; set; }
        public IList<JToken> Enum { get; set; }
        public JsonSchemaType? Disallow { get; set; }
        public JToken Default { get; set; }
        public IList<JsonSchema> Extends { get; set; }
        public string Format { get; set; }

        internal string Location { get; set; }

#pragma warning disable CA1305 // Specify IFormatProvider
        private readonly string _internalId = Guid.NewGuid().ToString("N");
#pragma warning restore CA1305 // Specify IFormatProvider

        internal string InternalId => _internalId;
        internal string DeferredReference { get; set; }
        internal bool ReferencesResolved { get; set; }
        public JsonSchema()
        {
            AllowAdditionalProperties = true;
            AllowAdditionalItems = true;
        }
        public static JsonSchema Read(JsonReader reader)
        {
            return Read(reader, new JsonSchemaResolver());
        }
        public static JsonSchema Read(JsonReader reader, JsonSchemaResolver resolver)
        {
            ValidationUtils.ArgumentNotNull(reader, nameof(reader));
            ValidationUtils.ArgumentNotNull(resolver, nameof(resolver));

            JsonSchemaBuilder builder = new JsonSchemaBuilder(resolver);
            return builder.Read(reader);
        }
        public static JsonSchema Parse(string json)
        {
            return Parse(json, new JsonSchemaResolver());
        }
        public static JsonSchema Parse(string json, JsonSchemaResolver resolver)
        {
            ValidationUtils.ArgumentNotNull(json, nameof(json));

            using (JsonReader reader = new JsonTextReader(new StringReader(json))) {
                return Read(reader, resolver);
            }
        }
        public void WriteTo(JsonWriter writer)
        {
            WriteTo(writer, new JsonSchemaResolver());
        }
        public void WriteTo(JsonWriter writer, JsonSchemaResolver resolver)
        {
            ValidationUtils.ArgumentNotNull(writer, nameof(writer));
            ValidationUtils.ArgumentNotNull(resolver, nameof(resolver));

            JsonSchemaWriter schemaWriter = new JsonSchemaWriter(writer, resolver);
            schemaWriter.WriteSchema(this);
        }
        public override string ToString()
        {
            StringWriter writer = new StringWriter(CultureInfo.InvariantCulture);
            JsonTextWriter jsonWriter = new JsonTextWriter(writer);
            jsonWriter.Formatting = Formatting.Indented;

            WriteTo(jsonWriter);

            return writer.ToString();
        }
    }
}