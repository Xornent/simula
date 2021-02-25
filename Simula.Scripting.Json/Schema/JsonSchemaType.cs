using System;

namespace Simula.Scripting.Json.Schema
{
    /// <summary>
    /// <para>
    /// The value types allowed by the <see cref="JsonSchema"/>.
    /// </para>
    /// <note type="caution">
    /// JSON Schema validation has been moved to its own package. See <see href="http://www.newtonsoft.com/jsonschema">http://www.newtonsoft.com/jsonschema</see> for more details.
    /// </note>
    /// </summary>
    [Flags]
    [Obsolete("JSON Schema validation has been moved to its own package. See http://www.newtonsoft.com/jsonschema for more details.")]
    public enum JsonSchemaType
    {
        /// <summary>
        /// No type specified.
        /// </summary>
        None = 0,

        /// <summary>
        /// String type.
        /// </summary>
        String = 1,

        /// <summary>
        /// Float type.
        /// </summary>
        Float = 2,

        /// <summary>
        /// Integer type.
        /// </summary>
        Integer = 4,

        /// <summary>
        /// Boolean type.
        /// </summary>
        Boolean = 8,

        /// <summary>
        /// Object type.
        /// </summary>
        Object = 16,

        /// <summary>
        /// Array type.
        /// </summary>
        Array = 32,

        /// <summary>
        /// Null type.
        /// </summary>
        Null = 64,

        /// <summary>
        /// Any type.
        /// </summary>
        Any = String | Float | Integer | Boolean | Object | Array | Null
    }
}