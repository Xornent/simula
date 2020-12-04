#region License

#endregion

using Simula.Scripting.Json.Serialization;
using Simula.Scripting.Json.Utilities;
using System;
using System.Globalization;
#if !HAVE_LINQ
using Simula.Scripting.Json.Utilities.LinqBridge;
#else

#endif

namespace Simula.Scripting.Json.Converters
{
    public class StringEnumConverter : JsonConverter
    {
        [Obsolete("StringEnumConverter.CamelCaseText is obsolete. Set StringEnumConverter.NamingStrategy with CamelCaseNamingStrategy instead.")]
        public bool CamelCaseText {
            get => NamingStrategy is CamelCaseNamingStrategy ? true : false;
            set {
                if (value) {
                    if (NamingStrategy is CamelCaseNamingStrategy) {
                        return;
                    }

                    NamingStrategy = new CamelCaseNamingStrategy();
                } else {
                    if (!(NamingStrategy is CamelCaseNamingStrategy)) {
                        return;
                    }

                    NamingStrategy = null;
                }
            }
        }
        public NamingStrategy? NamingStrategy { get; set; }
        public bool AllowIntegerValues { get; set; } = true;
        public StringEnumConverter()
        {
        }
        [Obsolete("StringEnumConverter(bool) is obsolete. Create a converter with StringEnumConverter(NamingStrategy, bool) instead.")]
        public StringEnumConverter(bool camelCaseText)
        {
            if (camelCaseText) {
                NamingStrategy = new CamelCaseNamingStrategy();
            }
        }
        public StringEnumConverter(NamingStrategy namingStrategy, bool allowIntegerValues = true)
        {
            NamingStrategy = namingStrategy;
            AllowIntegerValues = allowIntegerValues;
        }
        public StringEnumConverter(Type namingStrategyType)
        {
            ValidationUtils.ArgumentNotNull(namingStrategyType, nameof(namingStrategyType));

            NamingStrategy = JsonTypeReflector.CreateNamingStrategyInstance(namingStrategyType, null);
        }
        public StringEnumConverter(Type namingStrategyType, object[] namingStrategyParameters)
        {
            ValidationUtils.ArgumentNotNull(namingStrategyType, nameof(namingStrategyType));

            NamingStrategy = JsonTypeReflector.CreateNamingStrategyInstance(namingStrategyType, namingStrategyParameters);
        }
        public StringEnumConverter(Type namingStrategyType, object[] namingStrategyParameters, bool allowIntegerValues)
        {
            ValidationUtils.ArgumentNotNull(namingStrategyType, nameof(namingStrategyType));

            NamingStrategy = JsonTypeReflector.CreateNamingStrategyInstance(namingStrategyType, namingStrategyParameters);
            AllowIntegerValues = allowIntegerValues;
        }
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value == null) {
                writer.WriteNull();
                return;
            }

            Enum e = (Enum)value;

            if (!EnumUtils.TryToString(e.GetType(), value, NamingStrategy, out string? enumName)) {
                if (!AllowIntegerValues) {
                    throw JsonSerializationException.Create(null, writer.ContainerPath, "Integer value {0} is not allowed.".FormatWith(CultureInfo.InvariantCulture, e.ToString("D")), null);
                }
                writer.WriteValue(value);
            } else {
                writer.WriteValue(enumName);
            }
        }
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) {
                if (!ReflectionUtils.IsNullableType(objectType)) {
                    throw JsonSerializationException.Create(reader, "Cannot convert null value to {0}.".FormatWith(CultureInfo.InvariantCulture, objectType));
                }

                return null;
            }

            bool isNullable = ReflectionUtils.IsNullableType(objectType);
            Type t = isNullable ? Nullable.GetUnderlyingType(objectType) : objectType;

            try {
                if (reader.TokenType == JsonToken.String) {
                    string? enumText = reader.Value?.ToString();

                    if (StringUtils.IsNullOrEmpty(enumText) && isNullable) {
                        return null;
                    }

                    return EnumUtils.ParseEnum(t, NamingStrategy, enumText!, !AllowIntegerValues);
                }

                if (reader.TokenType == JsonToken.Integer) {
                    if (!AllowIntegerValues) {
                        throw JsonSerializationException.Create(reader, "Integer value {0} is not allowed.".FormatWith(CultureInfo.InvariantCulture, reader.Value));
                    }

                    return ConvertUtils.ConvertOrCast(reader.Value, CultureInfo.InvariantCulture, t);
                }
            } catch (Exception ex) {
                throw JsonSerializationException.Create(reader, "Error converting value {0} to type '{1}'.".FormatWith(CultureInfo.InvariantCulture, MiscellaneousUtils.ToString(reader.Value), objectType), ex);
            }
            throw JsonSerializationException.Create(reader, "Unexpected token {0} when parsing enum.".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
        }
        public override bool CanConvert(Type objectType)
        {
            Type t = (ReflectionUtils.IsNullableType(objectType))
                ? Nullable.GetUnderlyingType(objectType)
                : objectType;

            return t.IsEnum();
        }
    }
}
