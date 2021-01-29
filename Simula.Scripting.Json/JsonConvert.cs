
using System;
using System.IO;
using System.Globalization;
#if HAVE_BIG_INTEGER
using System.Numerics;
#endif
using Simula.Scripting.Json.Utilities;
using Simula.Scripting.Json.Converters;
using System.Text;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
#if HAVE_XLINQ
using System.Xml.Linq;
#endif

namespace Simula.Scripting.Json
{
    public static class JsonConvert
    {
        public static Func<JsonSerializerSettings>? DefaultSettings { get; set; }
        public static readonly string True = "true";
        public static readonly string False = "false";
        public static readonly string Null = "null";
        public static readonly string Undefined = "undefined";
        public static readonly string PositiveInfinity = "Infinity";
        public static readonly string NegativeInfinity = "-Infinity";
        public static readonly string NaN = "NaN";
        public static string ToString(DateTime value)
        {
            return ToString(value, DateFormatHandling.IsoDateFormat, DateTimeZoneHandling.RoundtripKind);
        }
        public static string ToString(DateTime value, DateFormatHandling format, DateTimeZoneHandling timeZoneHandling)
        {
            DateTime updatedDateTime = DateTimeUtils.EnsureDateTime(value, timeZoneHandling);

            using (StringWriter writer = StringUtils.CreateStringWriter(64)) {
                writer.Write('"');
                DateTimeUtils.WriteDateTimeString(writer, updatedDateTime, format, null, CultureInfo.InvariantCulture);
                writer.Write('"');
                return writer.ToString();
            }
        }

#if HAVE_DATE_TIME_OFFSET
        public static string ToString(DateTimeOffset value)
        {
            return ToString(value, DateFormatHandling.IsoDateFormat);
        }
        public static string ToString(DateTimeOffset value, DateFormatHandling format)
        {
            using (StringWriter writer = StringUtils.CreateStringWriter(64)) {
                writer.Write('"');
                DateTimeUtils.WriteDateTimeOffsetString(writer, value, format, null, CultureInfo.InvariantCulture);
                writer.Write('"');
                return writer.ToString();
            }
        }
#endif
        public static string ToString(bool value)
        {
            return (value) ? True : False;
        }
        public static string ToString(char value)
        {
            return ToString(char.ToString(value));
        }
        public static string ToString(Enum value)
        {
            return value.ToString("D");
        }
        public static string ToString(int value)
        {
            return value.ToString(null, CultureInfo.InvariantCulture);
        }
        public static string ToString(short value)
        {
            return value.ToString(null, CultureInfo.InvariantCulture);
        }
        [CLSCompliant(false)]
        public static string ToString(ushort value)
        {
            return value.ToString(null, CultureInfo.InvariantCulture);
        }
        [CLSCompliant(false)]
        public static string ToString(uint value)
        {
            return value.ToString(null, CultureInfo.InvariantCulture);
        }
        public static string ToString(long value)
        {
            return value.ToString(null, CultureInfo.InvariantCulture);
        }

#if HAVE_BIG_INTEGER
        private static string ToStringInternal(BigInteger value)
        {
            return value.ToString(null, CultureInfo.InvariantCulture);
        }
#endif
        [CLSCompliant(false)]
        public static string ToString(ulong value)
        {
            return value.ToString(null, CultureInfo.InvariantCulture);
        }
        public static string ToString(float value)
        {
            return EnsureDecimalPlace(value, value.ToString("R", CultureInfo.InvariantCulture));
        }

        internal static string ToString(float value, FloatFormatHandling floatFormatHandling, char quoteChar, bool nullable)
        {
            return EnsureFloatFormat(value, EnsureDecimalPlace(value, value.ToString("R", CultureInfo.InvariantCulture)), floatFormatHandling, quoteChar, nullable);
        }

        private static string EnsureFloatFormat(double value, string text, FloatFormatHandling floatFormatHandling, char quoteChar, bool nullable)
        {
            if (floatFormatHandling == FloatFormatHandling.Symbol || !(double.IsInfinity(value) || double.IsNaN(value))) {
                return text;
            }

            if (floatFormatHandling == FloatFormatHandling.DefaultValue) {
                return (!nullable) ? "0.0" : Null;
            }

            return quoteChar + text + quoteChar;
        }
        public static string ToString(double value)
        {
            return EnsureDecimalPlace(value, value.ToString("R", CultureInfo.InvariantCulture));
        }

        internal static string ToString(double value, FloatFormatHandling floatFormatHandling, char quoteChar, bool nullable)
        {
            return EnsureFloatFormat(value, EnsureDecimalPlace(value, value.ToString("R", CultureInfo.InvariantCulture)), floatFormatHandling, quoteChar, nullable);
        }

        private static string EnsureDecimalPlace(double value, string text)
        {
            if (double.IsNaN(value) || double.IsInfinity(value) || text.IndexOf('.') != -1 || text.IndexOf('E') != -1 || text.IndexOf('e') != -1) {
                return text;
            }

            return text + ".0";
        }

        private static string EnsureDecimalPlace(string text)
        {
            if (text.IndexOf('.') != -1) {
                return text;
            }

            return text + ".0";
        }
        public static string ToString(byte value)
        {
            return value.ToString(null, CultureInfo.InvariantCulture);
        }
        [CLSCompliant(false)]
        public static string ToString(sbyte value)
        {
            return value.ToString(null, CultureInfo.InvariantCulture);
        }
        public static string ToString(decimal value)
        {
            return EnsureDecimalPlace(value.ToString(null, CultureInfo.InvariantCulture));
        }
        public static string ToString(Guid value)
        {
            return ToString(value, '"');
        }

        internal static string ToString(Guid value, char quoteChar)
        {
            string text;
            string qc;
#if HAVE_CHAR_TO_STRING_WITH_CULTURE
            text = value.ToString("D", CultureInfo.InvariantCulture);
            qc = quoteChar.ToString(CultureInfo.InvariantCulture);
#else
            text = value.ToString("D");
            qc = quoteChar.ToString();
#endif

            return qc + text + qc;
        }
        public static string ToString(TimeSpan value)
        {
            return ToString(value, '"');
        }

        internal static string ToString(TimeSpan value, char quoteChar)
        {
            return ToString(value.ToString(), quoteChar);
        }
        public static string ToString(Uri? value)
        {
            if (value == null) {
                return Null;
            }

            return ToString(value, '"');
        }

        internal static string ToString(Uri value, char quoteChar)
        {
            return ToString(value.OriginalString, quoteChar);
        }
        public static string ToString(string? value)
        {
            return ToString(value, '"');
        }
        public static string ToString(string? value, char delimiter)
        {
            return ToString(value, delimiter, StringEscapeHandling.Default);
        }
        public static string ToString(string? value, char delimiter, StringEscapeHandling stringEscapeHandling)
        {
            if (delimiter != '"' && delimiter != '\'') {
                throw new ArgumentException("Delimiter must be a single or double quote.", nameof(delimiter));
            }

            return JavaScriptUtils.ToEscapedJavaScriptString(value, delimiter, true, stringEscapeHandling);
        }
        public static string ToString(object? value)
        {
            if (value == null) {
                return Null;
            }

            PrimitiveTypeCode typeCode = ConvertUtils.GetTypeCode(value.GetType());

            switch (typeCode) {
                case PrimitiveTypeCode.String:
                    return ToString((string)value);
                case PrimitiveTypeCode.Char:
                    return ToString((char)value);
                case PrimitiveTypeCode.Boolean:
                    return ToString((bool)value);
                case PrimitiveTypeCode.SByte:
                    return ToString((sbyte)value);
                case PrimitiveTypeCode.Int16:
                    return ToString((short)value);
                case PrimitiveTypeCode.UInt16:
                    return ToString((ushort)value);
                case PrimitiveTypeCode.Int32:
                    return ToString((int)value);
                case PrimitiveTypeCode.Byte:
                    return ToString((byte)value);
                case PrimitiveTypeCode.UInt32:
                    return ToString((uint)value);
                case PrimitiveTypeCode.Int64:
                    return ToString((long)value);
                case PrimitiveTypeCode.UInt64:
                    return ToString((ulong)value);
                case PrimitiveTypeCode.Single:
                    return ToString((float)value);
                case PrimitiveTypeCode.Double:
                    return ToString((double)value);
                case PrimitiveTypeCode.DateTime:
                    return ToString((DateTime)value);
                case PrimitiveTypeCode.Decimal:
                    return ToString((decimal)value);
#if HAVE_DB_NULL_TYPE_CODE
                case PrimitiveTypeCode.DBNull:
                    return Null;
#endif
#if HAVE_DATE_TIME_OFFSET
                case PrimitiveTypeCode.DateTimeOffset:
                    return ToString((DateTimeOffset)value);
#endif
                case PrimitiveTypeCode.Guid:
                    return ToString((Guid)value);
                case PrimitiveTypeCode.Uri:
                    return ToString((Uri)value);
                case PrimitiveTypeCode.TimeSpan:
                    return ToString((TimeSpan)value);
#if HAVE_BIG_INTEGER
                case PrimitiveTypeCode.BigInteger:
                    return ToStringInternal((BigInteger)value);
#endif
            }

            throw new ArgumentException("Unsupported type: {0}. Use the JsonSerializer class to get the object's JSON representation.".FormatWith(CultureInfo.InvariantCulture, value.GetType()));
        }

        #region Serialize
        [DebuggerStepThrough]
        public static string SerializeObject(object? value)
        {
            return SerializeObject(value, null, (JsonSerializerSettings?)null);
        }
        [DebuggerStepThrough]
        public static string SerializeObject(object? value, Formatting formatting)
        {
            return SerializeObject(value, formatting, (JsonSerializerSettings?)null);
        }
        [DebuggerStepThrough]
        public static string SerializeObject(object? value, params JsonConverter[] converters)
        {
            JsonSerializerSettings? settings = (converters != null && converters.Length > 0)
                ? new JsonSerializerSettings { Converters = converters }
                : null;

            return SerializeObject(value, null, settings);
        }
        [DebuggerStepThrough]
        public static string SerializeObject(object? value, Formatting formatting, params JsonConverter[] converters)
        {
            JsonSerializerSettings? settings = (converters != null && converters.Length > 0)
                ? new JsonSerializerSettings { Converters = converters }
                : null;

            return SerializeObject(value, null, formatting, settings);
        }
        [DebuggerStepThrough]
        public static string SerializeObject(object? value, JsonSerializerSettings settings)
        {
            return SerializeObject(value, null, settings);
        }
        [DebuggerStepThrough]
        public static string SerializeObject(object? value, Type? type, JsonSerializerSettings? settings)
        {
            JsonSerializer jsonSerializer = JsonSerializer.CreateDefault(settings);

            return SerializeObjectInternal(value, type, jsonSerializer);
        }
        [DebuggerStepThrough]
        public static string SerializeObject(object? value, Formatting formatting, JsonSerializerSettings? settings)
        {
            return SerializeObject(value, null, formatting, settings);
        }
        [DebuggerStepThrough]
        public static string SerializeObject(object? value, Type? type, Formatting formatting, JsonSerializerSettings? settings)
        {
            JsonSerializer jsonSerializer = JsonSerializer.CreateDefault(settings);
            jsonSerializer.Formatting = formatting;

            return SerializeObjectInternal(value, type, jsonSerializer);
        }

        private static string SerializeObjectInternal(object? value, Type? type, JsonSerializer jsonSerializer)
        {
            StringBuilder sb = new StringBuilder(256);
            StringWriter sw = new StringWriter(sb, CultureInfo.InvariantCulture);
            using (JsonTextWriter jsonWriter = new JsonTextWriter(sw)) {
                jsonWriter.Formatting = jsonSerializer.Formatting;

                jsonSerializer.Serialize(jsonWriter, value, type);
            }

            return sw.ToString();
        }
        #endregion

        #region Deserialize
        [DebuggerStepThrough]
        public static object? DeserializeObject(string value)
        {
            return DeserializeObject(value, null, (JsonSerializerSettings?)null);
        }
        [DebuggerStepThrough]
        public static object? DeserializeObject(string value, JsonSerializerSettings settings)
        {
            return DeserializeObject(value, null, settings);
        }
        [DebuggerStepThrough]
        public static object? DeserializeObject(string value, Type type)
        {
            return DeserializeObject(value, type, (JsonSerializerSettings?)null);
        }
        [DebuggerStepThrough]
        public static T DeserializeObject<T>(string value)
        {
            return DeserializeObject<T>(value, (JsonSerializerSettings?)null);
        }
        [DebuggerStepThrough]
        public static T DeserializeAnonymousType<T>(string value, T anonymousTypeObject)
        {
            return DeserializeObject<T>(value);
        }
        [DebuggerStepThrough]
        public static T DeserializeAnonymousType<T>(string value, T anonymousTypeObject, JsonSerializerSettings settings)
        {
            return DeserializeObject<T>(value, settings);
        }
        [DebuggerStepThrough]
        [return: MaybeNull]
        public static T DeserializeObject<T>(string value, params JsonConverter[] converters)
        {
#pragma warning disable CS8601 // Possible null reference assignment.
            return (T)DeserializeObject(value, typeof(T), converters);
#pragma warning restore CS8601 // Possible null reference assignment.
        }
        [DebuggerStepThrough]
        [return: MaybeNull]
        public static T DeserializeObject<T>(string value, JsonSerializerSettings? settings)
        {
#pragma warning disable CS8601 // Possible null reference assignment.
            return (T)DeserializeObject(value, typeof(T), settings);
#pragma warning restore CS8601 // Possible null reference assignment.
        }
        [DebuggerStepThrough]
        public static object? DeserializeObject(string value, Type type, params JsonConverter[] converters)
        {
            JsonSerializerSettings? settings = (converters != null && converters.Length > 0)
                ? new JsonSerializerSettings { Converters = converters }
                : null;

            return DeserializeObject(value, type, settings);
        }
        public static object? DeserializeObject(string value, Type? type, JsonSerializerSettings? settings)
        {
            ValidationUtils.ArgumentNotNull(value, nameof(value));

            JsonSerializer jsonSerializer = JsonSerializer.CreateDefault(settings);
            if (!jsonSerializer.IsCheckAdditionalContentSet()) {
                jsonSerializer.CheckAdditionalContent = true;
            }

            using (JsonTextReader reader = new JsonTextReader(new StringReader(value))) {
                return jsonSerializer.Deserialize(reader, type);
            }
        }
        #endregion

        #region Populate
        [DebuggerStepThrough]
        public static void PopulateObject(string value, object target)
        {
            PopulateObject(value, target, null);
        }
        public static void PopulateObject(string value, object target, JsonSerializerSettings? settings)
        {
            JsonSerializer jsonSerializer = JsonSerializer.CreateDefault(settings);

            using (JsonReader jsonReader = new JsonTextReader(new StringReader(value))) {
                jsonSerializer.Populate(jsonReader, target);

                if (settings != null && settings.CheckAdditionalContent) {
                    while (jsonReader.Read()) {
                        if (jsonReader.TokenType != JsonToken.Comment) {
                            throw JsonSerializationException.Create(jsonReader, "Additional text found in JSON string after finishing deserializing object.");
                        }
                    }
                }
            }
        }
        #endregion

        #region Xml
#if HAVE_XML_DOCUMENT
        public static string SerializeXmlNode(XmlNode? node)
        {
            return SerializeXmlNode(node, Formatting.None);
        }
        public static string SerializeXmlNode(XmlNode? node, Formatting formatting)
        {
            XmlNodeConverter converter = new XmlNodeConverter();

            return SerializeObject(node, formatting, converter);
        }
        public static string SerializeXmlNode(XmlNode? node, Formatting formatting, bool omitRootObject)
        {
            XmlNodeConverter converter = new XmlNodeConverter { OmitRootObject = omitRootObject };

            return SerializeObject(node, formatting, converter);
        }
        public static XmlDocument? DeserializeXmlNode(string value)
        {
            return DeserializeXmlNode(value, null);
        }
        public static XmlDocument? DeserializeXmlNode(string value, string? deserializeRootElementName)
        {
            return DeserializeXmlNode(value, deserializeRootElementName, false);
        }
        public static XmlDocument? DeserializeXmlNode(string value, string? deserializeRootElementName, bool writeArrayAttribute)
        {
            return DeserializeXmlNode(value, deserializeRootElementName, writeArrayAttribute, false);
        }
        public static XmlDocument? DeserializeXmlNode(string value, string? deserializeRootElementName, bool writeArrayAttribute, bool encodeSpecialCharacters)
        {
            XmlNodeConverter converter = new XmlNodeConverter();
            converter.DeserializeRootElementName = deserializeRootElementName;
            converter.WriteArrayAttribute = writeArrayAttribute;
            converter.EncodeSpecialCharacters = encodeSpecialCharacters;

            return (XmlDocument?)DeserializeObject(value, typeof(XmlDocument), converter);
        }
#endif

#if HAVE_XLINQ
        public static string SerializeXNode(XObject? node)
        {
            return SerializeXNode(node, Formatting.None);
        }
        public static string SerializeXNode(XObject? node, Formatting formatting)
        {
            return SerializeXNode(node, formatting, false);
        }
        public static string SerializeXNode(XObject? node, Formatting formatting, bool omitRootObject)
        {
            XmlNodeConverter converter = new XmlNodeConverter { OmitRootObject = omitRootObject };

            return SerializeObject(node, formatting, converter);
        }
        public static XDocument? DeserializeXNode(string value)
        {
            return DeserializeXNode(value, null);
        }
        public static XDocument? DeserializeXNode(string value, string? deserializeRootElementName)
        {
            return DeserializeXNode(value, deserializeRootElementName, false);
        }
        public static XDocument? DeserializeXNode(string value, string? deserializeRootElementName, bool writeArrayAttribute)
        {
            return DeserializeXNode(value, deserializeRootElementName, writeArrayAttribute, false);
        }
        public static XDocument? DeserializeXNode(string value, string? deserializeRootElementName, bool writeArrayAttribute, bool encodeSpecialCharacters)
        {
            XmlNodeConverter converter = new XmlNodeConverter();
            converter.DeserializeRootElementName = deserializeRootElementName;
            converter.WriteArrayAttribute = writeArrayAttribute;
            converter.EncodeSpecialCharacters = encodeSpecialCharacters;

            return (XDocument?)DeserializeObject(value, typeof(XDocument), converter);
        }
#endif
        #endregion
    }
}
