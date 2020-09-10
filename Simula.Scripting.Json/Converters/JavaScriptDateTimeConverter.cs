
using System;
using System.Globalization;
using Simula.Scripting.Json.Utilities;

namespace Simula.Scripting.Json.Converters
{
    public class JavaScriptDateTimeConverter : DateTimeConverterBase
    {
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            long ticks;

            if (value is DateTime dateTime)
            {
                DateTime utcDateTime = dateTime.ToUniversalTime();
                ticks = DateTimeUtils.ConvertDateTimeToJavaScriptTicks(utcDateTime);
            }
#if HAVE_DATE_TIME_OFFSET
            else if (value is DateTimeOffset dateTimeOffset)
            {
                DateTimeOffset utcDateTimeOffset = dateTimeOffset.ToUniversalTime();
                ticks = DateTimeUtils.ConvertDateTimeToJavaScriptTicks(utcDateTimeOffset.UtcDateTime);
            }
#endif
            else
            {
                throw new JsonSerializationException("Expected date object value.");
            }

            writer.WriteStartConstructor("Date");
            writer.WriteValue(ticks);
            writer.WriteEndConstructor();
        }
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                if (!ReflectionUtils.IsNullable(objectType))
                {
                    throw JsonSerializationException.Create(reader, "Cannot convert null value to {0}.".FormatWith(CultureInfo.InvariantCulture, objectType));
                }

                return null;
            }

            if (reader.TokenType != JsonToken.StartConstructor || !string.Equals(reader.Value?.ToString(), "Date", StringComparison.Ordinal))
            {
                throw JsonSerializationException.Create(reader, "Unexpected token or value when parsing date. Token: {0}, Value: {1}".FormatWith(CultureInfo.InvariantCulture, reader.TokenType, reader.Value));
            }

            if (!JavaScriptUtils.TryGetDateFromConstructorJson(reader, out DateTime d, out string? errorMessage))
            {
                throw JsonSerializationException.Create(reader, errorMessage);
            }

#if HAVE_DATE_TIME_OFFSET
            Type t = (ReflectionUtils.IsNullableType(objectType))
                ? Nullable.GetUnderlyingType(objectType)
                : objectType;
            if (t == typeof(DateTimeOffset))
            {
                return new DateTimeOffset(d);
            }
#endif
            return d;
        }
    }
}