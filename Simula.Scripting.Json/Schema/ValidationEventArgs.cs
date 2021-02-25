using System;
using Simula.Scripting.Json.Utilities;

namespace Simula.Scripting.Json.Schema
{
    /// <summary>
    /// <para>
    /// Returns detailed information related to the <see cref="ValidationEventHandler"/>.
    /// </para>
    /// <note type="caution">
    /// JSON Schema validation has been moved to its own package. See <see href="http://www.newtonsoft.com/jsonschema">http://www.newtonsoft.com/jsonschema</see> for more details.
    /// </note>
    /// </summary>
    [Obsolete("JSON Schema validation has been moved to its own package. See http://www.newtonsoft.com/jsonschema for more details.")]
    public class ValidationEventArgs : EventArgs
    {
        private readonly JsonSchemaException _ex;

        internal ValidationEventArgs(JsonSchemaException ex)
        {
            ValidationUtils.ArgumentNotNull(ex, nameof(ex));
            _ex = ex;
        }

        /// <summary>
        /// Gets the <see cref="JsonSchemaException"/> associated with the validation error.
        /// </summary>
        /// <value>The JsonSchemaException associated with the validation error.</value>
        public JsonSchemaException Exception
        {
            get { return _ex; }
        }

        /// <summary>
        /// Gets the path of the JSON location where the validation error occurred.
        /// </summary>
        /// <value>The path of the JSON location where the validation error occurred.</value>
        public string Path
        {
            get { return _ex.Path; }
        }

        /// <summary>
        /// Gets the text description corresponding to the validation error.
        /// </summary>
        /// <value>The text description.</value>
        public string Message
        {
            get { return _ex.Message; }
        }
    }
}