
using System;
using System.Collections.Generic;
using System.Globalization;
#if HAVE_BIG_INTEGER
using System.Numerics;
#endif
using System.Text;
using System.IO;
using System.Xml;
using Simula.Scripting.Json.Utilities;
using System.Diagnostics;

namespace Simula.Scripting.Json
{
    public partial class JsonTextWriter : JsonWriter
    {
        private const int IndentCharBufferSize = 12;
        private readonly TextWriter _writer;
        private Base64Encoder? _base64Encoder;
        private char _indentChar;
        private int _indentation;
        private char _quoteChar;
        private bool _quoteName;
        private bool[]? _charEscapeFlags;
        private char[]? _writeBuffer;
        private IArrayPool<char>? _arrayPool;
        private char[]? _indentChars;

        private Base64Encoder Base64Encoder
        {
            get
            {
                if (_base64Encoder == null)
                {
                    _base64Encoder = new Base64Encoder(_writer);
                }

                return _base64Encoder;
            }
        }
        public IArrayPool<char>? ArrayPool
        {
            get => _arrayPool;
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _arrayPool = value;
            }
        }
        public int Indentation
        {
            get => _indentation;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException("Indentation value must be greater than 0.");
                }

                _indentation = value;
            }
        }
        public char QuoteChar
        {
            get => _quoteChar;
            set
            {
                if (value != '"' && value != '\'')
                {
                    throw new ArgumentException(@"Invalid JavaScript string quote character. Valid quote characters are ' and "".");
                }

                _quoteChar = value;
                UpdateCharEscapeFlags();
            }
        }
        public char IndentChar
        {
            get => _indentChar;
            set
            {
                if (value != _indentChar)
                {
                    _indentChar = value;
                    _indentChars = null;
                }
            }
        }
        public bool QuoteName
        {
            get => _quoteName;
            set => _quoteName = value;
        }
        public JsonTextWriter(TextWriter textWriter)
        {
            if (textWriter == null)
            {
                throw new ArgumentNullException(nameof(textWriter));
            }

            _writer = textWriter;
            _quoteChar = '"';
            _quoteName = true;
            _indentChar = ' ';
            _indentation = 2;

            UpdateCharEscapeFlags();

#if HAVE_ASYNC
            _safeAsync = GetType() == typeof(JsonTextWriter);
#endif
        }
        public override void Flush()
        {
            _writer.Flush();
        }
        public override void Close()
        {
            base.Close();

            CloseBufferAndWriter();
        }

        private void CloseBufferAndWriter()
        {
            if (_writeBuffer != null)
            {
                BufferUtils.ReturnBuffer(_arrayPool, _writeBuffer);
                _writeBuffer = null;
            }

            if (CloseOutput)
            {
#if HAVE_STREAM_READER_WRITER_CLOSE
                _writer?.Close();
#else
                _writer?.Dispose();
#endif
            }
        }
        public override void WriteStartObject()
        {
            InternalWriteStart(JsonToken.StartObject, JsonContainerType.Object);

            _writer.Write('{');
        }
        public override void WriteStartArray()
        {
            InternalWriteStart(JsonToken.StartArray, JsonContainerType.Array);

            _writer.Write('[');
        }
        public override void WriteStartConstructor(string name)
        {
            InternalWriteStart(JsonToken.StartConstructor, JsonContainerType.Constructor);

            _writer.Write("new ");
            _writer.Write(name);
            _writer.Write('(');
        }
        protected override void WriteEnd(JsonToken token)
        {
            switch (token)
            {
                case JsonToken.EndObject:
                    _writer.Write('}');
                    break;
                case JsonToken.EndArray:
                    _writer.Write(']');
                    break;
                case JsonToken.EndConstructor:
                    _writer.Write(')');
                    break;
                default:
                    throw JsonWriterException.Create(this, "Invalid JsonToken: " + token, null);
            }
        }
        public override void WritePropertyName(string name)
        {
            InternalWritePropertyName(name);

            WriteEscapedString(name, _quoteName);

            _writer.Write(':');
        }
        public override void WritePropertyName(string name, bool escape)
        {
            InternalWritePropertyName(name);

            if (escape)
            {
                WriteEscapedString(name, _quoteName);
            }
            else
            {
                if (_quoteName)
                {
                    _writer.Write(_quoteChar);
                }

                _writer.Write(name);

                if (_quoteName)
                {
                    _writer.Write(_quoteChar);
                }
            }

            _writer.Write(':');
        }

        internal override void OnStringEscapeHandlingChanged()
        {
            UpdateCharEscapeFlags();
        }

        private void UpdateCharEscapeFlags()
        {
            _charEscapeFlags = JavaScriptUtils.GetCharEscapeFlags(StringEscapeHandling, _quoteChar);
        }
        protected override void WriteIndent()
        {
            int currentIndentCount = Top * _indentation;

            int newLineLen = SetIndentChars();

            _writer.Write(_indentChars, 0, newLineLen + Math.Min(currentIndentCount, IndentCharBufferSize));

            while ((currentIndentCount -= IndentCharBufferSize) > 0)
            {
                _writer.Write(_indentChars, newLineLen, Math.Min(currentIndentCount, IndentCharBufferSize));
            }
        }

        private int SetIndentChars()
        {
            string writerNewLine = _writer.NewLine;
            int newLineLen = writerNewLine.Length;
            bool match = _indentChars != null && _indentChars.Length == IndentCharBufferSize + newLineLen;
            if (match)
            {
                for (int i = 0; i != newLineLen; ++i)
                {
                    if (writerNewLine[i] != _indentChars![i])
                    {
                        match = false;
                        break;
                    }
                }
            }

            if (!match)
            {
                _indentChars = (writerNewLine + new string(_indentChar, IndentCharBufferSize)).ToCharArray();
            }

            return newLineLen;
        }
        protected override void WriteValueDelimiter()
        {
            _writer.Write(',');
        }
        protected override void WriteIndentSpace()
        {
            _writer.Write(' ');
        }

        private void WriteValueInternal(string value, JsonToken token)
        {
            _writer.Write(value);
        }

        #region WriteValue methods
        public override void WriteValue(object? value)
        {
#if HAVE_BIG_INTEGER
            if (value is BigInteger i)
            {
                InternalWriteValue(JsonToken.Integer);
                WriteValueInternal(i.ToString(CultureInfo.InvariantCulture), JsonToken.String);
            }
            else
#endif
            {
                base.WriteValue(value);
            }
        }
        public override void WriteNull()
        {
            InternalWriteValue(JsonToken.Null);
            WriteValueInternal(JsonConvert.Null, JsonToken.Null);
        }
        public override void WriteUndefined()
        {
            InternalWriteValue(JsonToken.Undefined);
            WriteValueInternal(JsonConvert.Undefined, JsonToken.Undefined);
        }
        public override void WriteRaw(string? json)
        {
            InternalWriteRaw();

            _writer.Write(json);
        }
        public override void WriteValue(string? value)
        {
            InternalWriteValue(JsonToken.String);

            if (value == null)
            {
                WriteValueInternal(JsonConvert.Null, JsonToken.Null);
            }
            else
            {
                WriteEscapedString(value, true);
            }
        }

        private void WriteEscapedString(string value, bool quote)
        {
            EnsureWriteBuffer();
            JavaScriptUtils.WriteEscapedJavaScriptString(_writer, value, _quoteChar, quote, _charEscapeFlags!, StringEscapeHandling, _arrayPool, ref _writeBuffer);
        }
        public override void WriteValue(int value)
        {
            InternalWriteValue(JsonToken.Integer);
            WriteIntegerValue(value);
        }
        [CLSCompliant(false)]
        public override void WriteValue(uint value)
        {
            InternalWriteValue(JsonToken.Integer);
            WriteIntegerValue(value);
        }
        public override void WriteValue(long value)
        {
            InternalWriteValue(JsonToken.Integer);
            WriteIntegerValue(value);
        }
        [CLSCompliant(false)]
        public override void WriteValue(ulong value)
        {
            InternalWriteValue(JsonToken.Integer);
            WriteIntegerValue(value, false);
        }
        public override void WriteValue(float value)
        {
            InternalWriteValue(JsonToken.Float);
            WriteValueInternal(JsonConvert.ToString(value, FloatFormatHandling, QuoteChar, false), JsonToken.Float);
        }
        public override void WriteValue(float? value)
        {
            if (value == null)
            {
                WriteNull();
            }
            else
            {
                InternalWriteValue(JsonToken.Float);
                WriteValueInternal(JsonConvert.ToString(value.GetValueOrDefault(), FloatFormatHandling, QuoteChar, true), JsonToken.Float);
            }
        }
        public override void WriteValue(double value)
        {
            InternalWriteValue(JsonToken.Float);
            WriteValueInternal(JsonConvert.ToString(value, FloatFormatHandling, QuoteChar, false), JsonToken.Float);
        }
        public override void WriteValue(double? value)
        {
            if (value == null)
            {
                WriteNull();
            }
            else
            {
                InternalWriteValue(JsonToken.Float);
                WriteValueInternal(JsonConvert.ToString(value.GetValueOrDefault(), FloatFormatHandling, QuoteChar, true), JsonToken.Float);
            }
        }
        public override void WriteValue(bool value)
        {
            InternalWriteValue(JsonToken.Boolean);
            WriteValueInternal(JsonConvert.ToString(value), JsonToken.Boolean);
        }
        public override void WriteValue(short value)
        {
            InternalWriteValue(JsonToken.Integer);
            WriteIntegerValue(value);
        }
        [CLSCompliant(false)]
        public override void WriteValue(ushort value)
        {
            InternalWriteValue(JsonToken.Integer);
            WriteIntegerValue(value);
        }
        public override void WriteValue(char value)
        {
            InternalWriteValue(JsonToken.String);
            WriteValueInternal(JsonConvert.ToString(value), JsonToken.String);
        }
        public override void WriteValue(byte value)
        {
            InternalWriteValue(JsonToken.Integer);
            WriteIntegerValue(value);
        }
        [CLSCompliant(false)]
        public override void WriteValue(sbyte value)
        {
            InternalWriteValue(JsonToken.Integer);
            WriteIntegerValue(value);
        }
        public override void WriteValue(decimal value)
        {
            InternalWriteValue(JsonToken.Float);
            WriteValueInternal(JsonConvert.ToString(value), JsonToken.Float);
        }
        public override void WriteValue(DateTime value)
        {
            InternalWriteValue(JsonToken.Date);
            value = DateTimeUtils.EnsureDateTime(value, DateTimeZoneHandling);

            if (StringUtils.IsNullOrEmpty(DateFormatString))
            {
                int length = WriteValueToBuffer(value);

                _writer.Write(_writeBuffer, 0, length);
            }
            else
            {
                _writer.Write(_quoteChar);
                _writer.Write(value.ToString(DateFormatString, Culture));
                _writer.Write(_quoteChar);
            }
        }

        private int WriteValueToBuffer(DateTime value)
        {
            EnsureWriteBuffer();
            MiscellaneousUtils.Assert(_writeBuffer != null);

            int pos = 0;
            _writeBuffer[pos++] = _quoteChar;
            pos = DateTimeUtils.WriteDateTimeString(_writeBuffer, pos, value, null, value.Kind, DateFormatHandling);
            _writeBuffer[pos++] = _quoteChar;
            return pos;
        }
        public override void WriteValue(byte[]? value)
        {
            if (value == null)
            {
                WriteNull();
            }
            else
            {
                InternalWriteValue(JsonToken.Bytes);
                _writer.Write(_quoteChar);
                Base64Encoder.Encode(value, 0, value.Length);
                Base64Encoder.Flush();
                _writer.Write(_quoteChar);
            }
        }

#if HAVE_DATE_TIME_OFFSET
        public override void WriteValue(DateTimeOffset value)
        {
            InternalWriteValue(JsonToken.Date);

            if (StringUtils.IsNullOrEmpty(DateFormatString))
            {
                int length = WriteValueToBuffer(value);

                _writer.Write(_writeBuffer, 0, length);
            }
            else
            {
                _writer.Write(_quoteChar);
                _writer.Write(value.ToString(DateFormatString, Culture));
                _writer.Write(_quoteChar);
            }
        }

        private int WriteValueToBuffer(DateTimeOffset value)
        {
            EnsureWriteBuffer();
            MiscellaneousUtils.Assert(_writeBuffer != null);

            int pos = 0;
            _writeBuffer[pos++] = _quoteChar;
            pos = DateTimeUtils.WriteDateTimeString(_writeBuffer, pos, (DateFormatHandling == DateFormatHandling.IsoDateFormat) ? value.DateTime : value.UtcDateTime, value.Offset, DateTimeKind.Local, DateFormatHandling);
            _writeBuffer[pos++] = _quoteChar;
            return pos;
        }
#endif
        public override void WriteValue(Guid value)
        {
            InternalWriteValue(JsonToken.String);

            string text;

#if HAVE_CHAR_TO_STRING_WITH_CULTURE
            text = value.ToString("D", CultureInfo.InvariantCulture);
#else
            text = value.ToString("D");
#endif

            _writer.Write(_quoteChar);
            _writer.Write(text);
            _writer.Write(_quoteChar);
        }
        public override void WriteValue(TimeSpan value)
        {
            InternalWriteValue(JsonToken.String);

            string text;
#if !HAVE_TIME_SPAN_TO_STRING_WITH_CULTURE
            text = value.ToString();
#else
            text = value.ToString(null, CultureInfo.InvariantCulture);
#endif

            _writer.Write(_quoteChar);
            _writer.Write(text);
            _writer.Write(_quoteChar);
        }
        public override void WriteValue(Uri? value)
        {
            if (value == null)
            {
                WriteNull();
            }
            else
            {
                InternalWriteValue(JsonToken.String);
                WriteEscapedString(value.OriginalString, true);
            }
        }
        #endregion
        public override void WriteComment(string? text)
        {
            InternalWriteComment();

            _writer.Write("/*");
            _writer.Write(text);
            _writer.Write("*/");
        }
        public override void WriteWhitespace(string ws)
        {
            InternalWriteWhitespace(ws);

            _writer.Write(ws);
        }

        private void EnsureWriteBuffer()
        {
            if (_writeBuffer == null)
            {
                _writeBuffer = BufferUtils.RentBuffer(_arrayPool, 35);
            }
        }

        private void WriteIntegerValue(long value)
        {
            if (value >= 0 && value <= 9)
            {
                _writer.Write((char)('0' + value));
            }
            else
            {
                bool negative = value < 0;
                WriteIntegerValue(negative ? (ulong)-value : (ulong)value, negative);
            }
        }

        private void WriteIntegerValue(ulong value, bool negative)
        {
            if (!negative & value <= 9)
            {
                _writer.Write((char)('0' + value));
            }
            else
            {
                int length = WriteNumberToBuffer(value, negative);
                _writer.Write(_writeBuffer, 0, length);
            }
        }

        private int WriteNumberToBuffer(ulong value, bool negative)
        {
            if (value <= uint.MaxValue)
            {
                return WriteNumberToBuffer((uint)value, negative);
            }

            EnsureWriteBuffer();
            MiscellaneousUtils.Assert(_writeBuffer != null);

            int totalLength = MathUtils.IntLength(value);

            if (negative)
            {
                totalLength++;
                _writeBuffer[0] = '-';
            }

            int index = totalLength;

            do
            {
                ulong quotient = value / 10;
                ulong digit = value - (quotient * 10);
                _writeBuffer[--index] = (char)('0' + digit);
                value = quotient;
            } while (value != 0);

            return totalLength;
        }

        private void WriteIntegerValue(int value)
        {
            if (value >= 0 && value <= 9)
            {
                _writer.Write((char)('0' + value));
            }
            else
            {
                bool negative = value < 0;
                WriteIntegerValue(negative ? (uint)-value : (uint)value, negative);
            }
        }

        private void WriteIntegerValue(uint value, bool negative)
        {
            if (!negative & value <= 9)
            {
                _writer.Write((char)('0' + value));
            }
            else
            {
                int length = WriteNumberToBuffer(value, negative);
                _writer.Write(_writeBuffer, 0, length);
            }
        }

        private int WriteNumberToBuffer(uint value, bool negative)
        {
            EnsureWriteBuffer();
            MiscellaneousUtils.Assert(_writeBuffer != null);

            int totalLength = MathUtils.IntLength(value);

            if (negative)
            {
                totalLength++;
                _writeBuffer[0] = '-';
            }

            int index = totalLength;

            do
            {
                uint quotient = value / 10;
                uint digit = value - (quotient * 10);
                _writeBuffer[--index] = (char)('0' + digit);
                value = quotient;
            } while (value != 0);

            return totalLength;
        }
    }
}