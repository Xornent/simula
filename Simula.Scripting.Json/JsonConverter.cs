
using System;
using Simula.Scripting.Json.Utilities;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;

namespace Simula.Scripting.Json
{
    public abstract class JsonConverter
    {
        public abstract void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer);
        public abstract object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer);
        public abstract bool CanConvert(Type objectType);
        public virtual bool CanRead => true;
        public virtual bool CanWrite => true;
    }
    public abstract class JsonConverter<T> : JsonConverter
    {
        public sealed override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (!(value != null ? value is T : ReflectionUtils.IsNullable(typeof(T))))
            {
                throw new JsonSerializationException("Converter cannot write specified value to JSON. {0} is required.".FormatWith(CultureInfo.InvariantCulture, typeof(T)));
            }
#pragma warning disable CS8601 // Possible null reference assignment.
            WriteJson(writer, (T)value, serializer);
#pragma warning restore CS8601 // Possible null reference assignment.
        }
        public abstract void WriteJson(JsonWriter writer, [AllowNull]T value, JsonSerializer serializer);
        public sealed override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            bool existingIsNull = existingValue == null;
            if (!(existingIsNull || existingValue is T))
            {
                throw new JsonSerializationException("Converter cannot read JSON with the specified existing value. {0} is required.".FormatWith(CultureInfo.InvariantCulture, typeof(T)));
            }
#pragma warning disable CS8601 // Possible null reference assignment.
            return ReadJson(reader, objectType, existingIsNull ? default : (T)existingValue, !existingIsNull, serializer);
#pragma warning restore CS8601 // Possible null reference assignment.
        }
        public abstract T ReadJson(JsonReader reader, Type objectType, [AllowNull]T existingValue, bool hasExistingValue, JsonSerializer serializer);
        public sealed override bool CanConvert(Type objectType)
        {
            return typeof(T).IsAssignableFrom(objectType);
        }
    }
}