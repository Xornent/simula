
#if HAVE_LINQ || HAVE_ADO_NET
using Simula.Scripting.Json.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
#if HAVE_ADO_NET
using System.Data.SqlTypes;
#endif

namespace Simula.Scripting.Json.Converters
{
    public class BinaryConverter : JsonConverter
    {
#if HAVE_LINQ
        private const string BinaryTypeName = "System.Data.Linq.Binary";
        private const string BinaryToArrayName = "ToArray";
        private static ReflectionObject? _reflectionObject;
#endif
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value == null) {
                writer.WriteNull();
                return;
            }

            byte[] data = GetByteArray(value);

            writer.WriteValue(data);
        }

        private byte[] GetByteArray(object value)
        {
#if HAVE_LINQ
            if (value.GetType().FullName == BinaryTypeName) {
                EnsureReflectionObject(value.GetType());
                MiscellaneousUtils.Assert(_reflectionObject != null);

                return (byte[])_reflectionObject.GetValue(value, BinaryToArrayName)!;
            }
#endif
#if HAVE_ADO_NET
            if (value is SqlBinary binary)
            {
                return binary.Value;
            }
#endif

            throw new JsonSerializationException("Unexpected value type when writing binary: {0}".FormatWith(CultureInfo.InvariantCulture, value.GetType()));
        }

#if HAVE_LINQ
        private static void EnsureReflectionObject(Type t)
        {
            if (_reflectionObject == null) {
                _reflectionObject = ReflectionObject.Create(t, t.GetConstructor(new[] { typeof(byte[]) }), BinaryToArrayName);
            }
        }
#endif
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) {
                if (!ReflectionUtils.IsNullable(objectType)) {
                    throw JsonSerializationException.Create(reader, "Cannot convert null value to {0}.".FormatWith(CultureInfo.InvariantCulture, objectType));
                }

                return null;
            }

            byte[] data;

            if (reader.TokenType == JsonToken.StartArray) {
                data = ReadByteArray(reader);
            } else if (reader.TokenType == JsonToken.String) {
                string encodedData = reader.Value!.ToString();
                data = Convert.FromBase64String(encodedData);
            } else {
                throw JsonSerializationException.Create(reader, "Unexpected token parsing binary. Expected String or StartArray, got {0}.".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
            }

            Type t = (ReflectionUtils.IsNullableType(objectType))
                ? Nullable.GetUnderlyingType(objectType)
                : objectType;

#if HAVE_LINQ
            if (t.FullName == BinaryTypeName) {
                EnsureReflectionObject(t);
                MiscellaneousUtils.Assert(_reflectionObject != null);

                return _reflectionObject.Creator!(data);
            }
#endif

#if HAVE_ADO_NET
            if (t == typeof(SqlBinary))
            {
                return new SqlBinary(data);
            }
#endif

            throw JsonSerializationException.Create(reader, "Unexpected object type when writing binary: {0}".FormatWith(CultureInfo.InvariantCulture, objectType));
        }

        private byte[] ReadByteArray(JsonReader reader)
        {
            List<byte> byteList = new List<byte>();

            while (reader.Read()) {
                switch (reader.TokenType) {
                    case JsonToken.Integer:
                        byteList.Add(Convert.ToByte(reader.Value, CultureInfo.InvariantCulture));
                        break;
                    case JsonToken.EndArray:
                        return byteList.ToArray();
                    case JsonToken.Comment:
                        break;
                    default:
                        throw JsonSerializationException.Create(reader, "Unexpected token when reading bytes: {0}".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
                }
            }

            throw JsonSerializationException.Create(reader, "Unexpected end when reading bytes.");
        }
        public override bool CanConvert(Type objectType)
        {
#if HAVE_LINQ
            if (objectType.FullName == BinaryTypeName) {
                return true;
            }
#endif
#if HAVE_ADO_NET
            if (objectType == typeof(SqlBinary) || objectType == typeof(SqlBinary?))
            {
                return true;
            }
#endif

            return false;
        }
    }
}

#endif