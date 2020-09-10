
using System;
using System.Globalization;
using Simula.Scripting.Json.Utilities;

namespace Simula.Scripting.Json.Converters
{
    public class IsoDateTimeConverter : DateTimeConverterBase
    {
        private const string DefaultDateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFK";

        private DateTimeStyles _dateTimeStyles = DateTimeStyles.RoundtripKind;
        private string? _dateTimeFormat;
        private CultureInfo? _culture;
        public DateTimeStyles DateTimeStyles
        {
            get => _dateTimeStyles;
            set => _dateTimeStyles = value;
        }
        public string? DateTimeFormat
        {
            get => _dateTimeFormat ?? string.Empty;
            set => _dateTimeFormat = (StringUtils.IsNullOrEmpty(value)) ? null : value;
        }
        public CultureInfo Culture
        {
            get => _culture ?? CultureInfo.CurrentCulture;
            set => _culture = value;
        }
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            string text;

            if (value is DateTime dateTime)
            {
                if ((_dateTimeStyles & DateTimeStyles.AdjustToUniversal) == DateTimeStyles.AdjustToUniversal
                    || (_dateTimeStyles & DateTimeStyles.AssumeUniversal) == DateTimeStyles.AssumeUniversal)
                {
                    dateTime = dateTime.ToUniversalTime();
                }

                text = dateTime.ToString(_dateTimeFormat ?? DefaultDateTimeFormat, Culture);
            }
#if HAVE_DATE_TIME_OFFSET
            else if (value is DateTimeOffset dateTimeOffset)
            {
                if ((_dateTimeStyles & DateTimeStyles.AdjustToUniversal) == DateTimeStyles.AdjustToUniversal
                    || (_dateTimeStyles & DateTimeStyles.AssumeUniversal) == DateTimeStyles.AssumeUniversal)
                {
                    dateTimeOffset = dateTimeOffset.ToUniversalTime();
                }

                text = dateTimeOffset.ToString(_dateTimeFormat ?? DefaultDateTimeFormat, Culture);
            }
#endif
            else
            {
                throw new JsonSerializationException("Unexpected value when converting date. Expected DateTime or DateTimeOffset, got {0}.".FormatWith(CultureInfo.InvariantCulture, ReflectionUtils.GetObjectType(value)!));
            }

            writer.WriteValue(text);
        }
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            bool nullable = ReflectionUtils.IsNullableType(objectType);
            if (reader.TokenType == JsonToken.Null)
            {
                if (!nullable)
                {
                    throw JsonSerializationException.Create(reader, "Cannot convert null value to {0}.".FormatWith(CultureInfo.InvariantCulture, objectType));
                }

                return null;
            }

#if HAVE_DATE_TIME_OFFSET
            Type t = (nullable)
                ? Nullable.GetUnderlyingType(objectType)
                : objectType;
#endif

            if (reader.TokenType == JsonToken.Date)
            {
#if HAVE_DATE_TIME_OFFSET
                if (t == typeof(DateTimeOffset))
                {
                    return (reader.Value is DateTimeOffset) ? reader.Value : new DateTimeOffset((DateTime)reader.Value!);
                }
                if (reader.Value is DateTimeOffset offset)
                {
                    return offset.DateTime;
                }
#endif

                return reader.Value;
            }

            if (reader.TokenType != JsonToken.String)
            {
                throw JsonSerializationException.Create(reader, "Unexpected token parsing date. Expected String, got {0}.".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
            }

            string? dateText = reader.Value?.ToString();

            if (StringUtils.IsNullOrEmpty(dateText) && nullable)
            {
                return null;
            }

#if HAVE_DATE_TIME_OFFSET
            if (t == typeof(DateTimeOffset))
            {
                if (!StringUtils.IsNullOrEmpty(_dateTimeFormat))
                {
                    return DateTimeOffset.ParseExact(dateText, _dateTimeFormat, Culture, _dateTimeStyles);
                }
                else
                {
                    return DateTimeOffset.Parse(dateText, Culture, _dateTimeStyles);
                }
            }
#endif

            if (!StringUtils.IsNullOrEmpty(_dateTimeFormat))
            {
                return DateTime.ParseExact(dateText, _dateTimeFormat, Culture, _dateTimeStyles);
            }
            else
            {
                return DateTime.Parse(dateText, Culture, _dateTimeStyles);
            }
        }
    }
}
