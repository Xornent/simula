
using System;
using System.Collections.Generic;
using Simula.Scripting.Json.Linq;
using Simula.Scripting.Json.Utilities;

#nullable disable

namespace Simula.Scripting.Json.Schema
{
    [Obsolete("JSON Schema validation has been moved to its own package. See https://www.newtonsoft.com/jsonschema for more details.")]
    public static class Extensions
    {
        [Obsolete("JSON Schema validation has been moved to its own package. See https://www.newtonsoft.com/jsonschema for more details.")]
        public static bool IsValid(this JToken source, JsonSchema schema)
        {
            bool valid = true;
            source.Validate(schema, (sender, args) => { valid = false; });
            return valid;
        }
        [Obsolete("JSON Schema validation has been moved to its own package. See https://www.newtonsoft.com/jsonschema for more details.")]
        public static bool IsValid(this JToken source, JsonSchema schema, out IList<string> errorMessages)
        {
            IList<string> errors = new List<string>();

            source.Validate(schema, (sender, args) => errors.Add(args.Message));

            errorMessages = errors;
            return (errorMessages.Count == 0);
        }
        [Obsolete("JSON Schema validation has been moved to its own package. See https://www.newtonsoft.com/jsonschema for more details.")]
        public static void Validate(this JToken source, JsonSchema schema)
        {
            source.Validate(schema, null);
        }
        [Obsolete("JSON Schema validation has been moved to its own package. See https://www.newtonsoft.com/jsonschema for more details.")]
        public static void Validate(this JToken source, JsonSchema schema, ValidationEventHandler validationEventHandler)
        {
            ValidationUtils.ArgumentNotNull(source, nameof(source));
            ValidationUtils.ArgumentNotNull(schema, nameof(schema));

            using (JsonValidatingReader reader = new JsonValidatingReader(source.CreateReader()))
            {
                reader.Schema = schema;
                if (validationEventHandler != null)
                {
                    reader.ValidationEventHandler += validationEventHandler;
                }

                while (reader.Read())
                {
                }
            }
        }
    }
}