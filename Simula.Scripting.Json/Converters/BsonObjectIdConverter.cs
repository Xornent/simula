
using Simula.Scripting.Json.Bson;
using Simula.Scripting.Json.Utilities;
using System;
using System.Globalization;

#nullable disable

namespace Simula.Scripting.Json.Converters
{
    [Obsolete("BSON reading and writing has been moved to its own package. See https://www.nuget.org/packages/Simula.Scripting.Json.Bson for more details.")]
    public class BsonObjectIdConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            BsonObjectId objectId = (BsonObjectId)value;

            if (writer is BsonWriter bsonWriter) {
                bsonWriter.WriteObjectId(objectId.Value);
            } else {
                writer.WriteValue(objectId.Value);
            }
        }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.Bytes) {
                throw new JsonSerializationException("Expected Bytes but got {0}.".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
            }

            byte[] value = (byte[])reader.Value;

            return new BsonObjectId(value);
        }
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(BsonObjectId));
        }
    }
}