
using Simula.Scripting.Json.Utilities;
using System;
using System.Globalization;

namespace Simula.Scripting.Json.Converters
{
    public class UnixDateTimeConverter : DateTimeConverterBase
    {
        internal static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            long seconds;

            if (value is DateTime dateTime) {
                seconds = (long)(dateTime.ToUniversalTime() - UnixEpoch).TotalSeconds;
            }
#if HAVE_DATE_TIME_OFFSET
            else if (value is DateTimeOffset dateTimeOffset) {
                seconds = (long)(dateTimeOffset.ToUniversalTime() - UnixEpoch).TotalSeconds;
            }
#endif
            else {
                throw new JsonSerializationException("Expected date object value.");
            }

            if (seconds < 0) {
                throw new JsonSerializationException("Cannot convert date value that is before Unix epoch of 00:00:00 UTC on 1 January 1970.");
            }

            writer.WriteValue(seconds);
        }
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            bool nullable = ReflectionUtils.IsNullable(objectType);
            if (reader.TokenType == JsonToken.Null) {
                if (!nullable) {
                    throw JsonSerializationException.Create(reader, "Cannot convert null value to {0}.".FormatWith(CultureInfo.InvariantCulture, objectType));
                }

                return null;
            }

            long seconds;

            if (reader.TokenType == JsonToken.Integer) {
                seconds = (long)reader.Value!;
            } else if (reader.TokenType == JsonToken.String) {
                if (!long.TryParse((string)reader.Value!, out seconds)) {
                    throw JsonSerializationException.Create(reader, "Cannot convert invalid value to {0}.".FormatWith(CultureInfo.InvariantCulture, objectType));
                }
            } else {
                throw JsonSerializationException.Create(reader, "Unexpected token parsing date. Expected Integer or String, got {0}.".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
            }

            if (seconds >= 0) {
                DateTime d = UnixEpoch.AddSeconds(seconds);

#if HAVE_DATE_TIME_OFFSET
                Type t = (nullable)
                    ? Nullable.GetUnderlyingType(objectType)
                    : objectType;
                if (t == typeof(DateTimeOffset)) {
                    return new DateTimeOffset(d, TimeSpan.Zero);
                }
#endif
                return d;
            } else {
                throw JsonSerializationException.Create(reader, "Cannot convert value that is before Unix epoch of 00:00:00 UTC on 1 January 1970 to {0}.".FormatWith(CultureInfo.InvariantCulture, objectType));
            }
        }
    }
}