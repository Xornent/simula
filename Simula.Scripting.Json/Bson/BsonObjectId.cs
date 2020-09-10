
using System;
using Simula.Scripting.Json.Utilities;

#nullable disable

namespace Simula.Scripting.Json.Bson
{
    [Obsolete("BSON reading and writing has been moved to its own package. See https://www.nuget.org/packages/Simula.Scripting.Json.Bson for more details.")]
    public class BsonObjectId
    {
        public byte[] Value { get; }
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