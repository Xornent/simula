using System;

namespace Simula.Scripting.Json.Schema
{
    /// <summary>
    /// <para>
    /// Specifies undefined schema Id handling options for the <see cref="JsonSchemaGenerator"/>.
    /// </para>
    /// <note type="caution">
    /// JSON Schema validation has been moved to its own package. See <see href="http://www.newtonsoft.com/jsonschema">http://www.newtonsoft.com/jsonschema</see> for more details.
    /// </note>
    /// </summary>
    [Obsolete("JSON Schema validation has been moved to its own package. See http://www.newtonsoft.com/jsonschema for more details.")]
    public enum UndefinedSchemaIdHandling
    {
        /// <summary>
        /// Do not infer a schema Id.
        /// </summary>
        None = 0,

        /// <summary>
        /// Use the .NET type name as the schema Id.
        /// </summary>
        UseTypeName = 1,

        /// <summary>
        /// Use the assembly qualified .NET type name as the schema Id.
        /// </summary>
        UseAssemblyQualifiedName = 2,
    }
}