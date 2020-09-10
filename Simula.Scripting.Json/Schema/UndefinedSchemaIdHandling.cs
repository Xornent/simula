
using System;

#nullable disable

namespace Simula.Scripting.Json.Schema
{
    [Obsolete("JSON Schema validation has been moved to its own package. See https://www.newtonsoft.com/jsonschema for more details.")]
    public enum UndefinedSchemaIdHandling
    {
        None = 0,
        UseTypeName = 1,
        UseAssemblyQualifiedName = 2,
    }
}