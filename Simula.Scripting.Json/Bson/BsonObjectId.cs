using System;
using Simula.Scripting.Json.Utilities;

namespace Simula.Scripting.Json.Bson
{
    /// <summary>
    /// Represents a BSON Oid (object id).
    /// </summary>
    [Obsolete("BSON reading and writing has been moved to its own package. See https://www.nuget.org/packages/Simula.Scripting.Json.Bson for more details.")]
    public class BsonObjectId
    {
        /// <summary>
        /// Gets or sets the value of the Oid.
        /// </summary>
        /// <value>The value of the Oid.</value>
        public byte[] Value { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BsonObjectId"/> class.
        /// </summary>
        /// <param name="value">The Oid value.</param>
        public BsonObjectId(byte[] value)
        {
            ValidationUtils.ArgumentNotNull(value, nameof(value));
            if (value.Length != 12)
            {
                throw new ArgumentException("An ObjectId must be 12 bytes", nameof(value));
            }

            Value = value;
        }
    }
}