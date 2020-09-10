
#if HAVE_ENTITY_FRAMEWORK
using System;
using Simula.Scripting.Json.Serialization;
using System.Globalization;
using Simula.Scripting.Json.Utilities;
using System.Diagnostics;

namespace Simula.Scripting.Json.Converters
{
    public class EntityKeyMemberConverter : JsonConverter
    {
        private const string EntityKeyMemberFullTypeName = "System.Data.EntityKeyMember";

        private const string KeyPropertyName = "Key";
        private const string TypePropertyName = "Type";
        private const string ValuePropertyName = "Value";

        private static ReflectionObject? _reflectionObject;
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            EnsureReflectionObject(value.GetType());
            MiscellaneousUtils.Assert(_reflectionObject != null);

            DefaultContractResolver? resolver = serializer.ContractResolver as DefaultContractResolver;

            string keyName = (string)_reflectionObject.GetValue(value, KeyPropertyName)!;
            object? keyValue = _reflectionObject.GetValue(value, ValuePropertyName);

            Type? keyValueType = keyValue?.GetType();

            writer.WriteStartObject();
            writer.WritePropertyName((resolver != null) ? resolver.GetResolvedPropertyName(KeyPropertyName) : KeyPropertyName);
            writer.WriteValue(keyName);
            writer.WritePropertyName((resolver != null) ? resolver.GetResolvedPropertyName(TypePropertyName) : TypePropertyName);
            writer.WriteValue(keyValueType?.FullName);

            writer.WritePropertyName((resolver != null) ? resolver.GetResolvedPropertyName(ValuePropertyName) : ValuePropertyName);

            if (keyValueType != null)
            {
                if (JsonSerializerInternalWriter.TryConvertToString(keyValue!, keyValueType, out string? valueJson))
                {
                    writer.WriteValue(valueJson);
                }
                else
                {
                    writer.WriteValue(keyValue);
                }
            }
            else
            {
                writer.WriteNull();
            }

            writer.WriteEndObject();
        }

        private static void ReadAndAssertProperty(JsonReader reader, string propertyName)
        {
            reader.ReadAndAssert();

            if (reader.TokenType != JsonToken.PropertyName || !string.Equals(reader.Value?.ToString(), propertyName, StringComparison.OrdinalIgnoreCase))
            {
                throw new JsonSerializationException("Expected JSON property '{0}'.".FormatWith(CultureInfo.InvariantCulture, propertyName));
            }
        }
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            EnsureReflectionObject(objectType);
            MiscellaneousUtils.Assert(_reflectionObject != null);

            object entityKeyMember = _reflectionObject.Creator!();

            ReadAndAssertProperty(reader, KeyPropertyName);
            reader.ReadAndAssert();
            _reflectionObject.SetValue(entityKeyMember, KeyPropertyName, reader.Value?.ToString());

            ReadAndAssertProperty(reader, TypePropertyName);
            reader.ReadAndAssert();
            string? type = reader.Value?.ToString();

            Type t = Type.GetType(type);

            ReadAndAssertProperty(reader, ValuePropertyName);
            reader.ReadAndAssert();
            _reflectionObject.SetValue(entityKeyMember, ValuePropertyName, serializer.Deserialize(reader, t));

            reader.ReadAndAssert();

            return entityKeyMember;
        }

        private static void EnsureReflectionObject(Type objectType)
        {
            if (_reflectionObject == null)
            {
                _reflectionObject = ReflectionObject.Create(objectType, KeyPropertyName, ValuePropertyName);
            }
        }
        public override bool CanConvert(Type objectType)
        {
            return objectType.AssignableToTypeName(EntityKeyMemberFullTypeName, false);
        }
    }
}

#endif