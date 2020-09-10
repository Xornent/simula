
#if HAVE_ASYNC

using System;
using System.Globalization;
using System.Threading;
#if HAVE_BIG_INTEGER
using System.Numerics;
#endif
using System.Threading.Tasks;
using Simula.Scripting.Json.Utilities;
using System.Diagnostics;

namespace Simula.Scripting.Json
{
    public partial class JsonTextWriter
    {
#if HAVE_ASYNC // Double-check this isn't included inappropriately.
        private readonly bool _safeAsync;
#endif
        public override Task FlushAsync(CancellationToken cancellationToken = default)
        {
            return _safeAsync ? DoFlushAsync(cancellationToken) : base.FlushAsync(cancellationToken);
        }

        internal Task DoFlushAsync(CancellationToken cancellationToken)
        {
            return cancellationToken.CancelIfRequestedAsync() ?? _writer.FlushAsync();
        }
        protected override Task WriteValueDelimiterAsync(CancellationToken cancellationToken)
        {
            return _safeAsync ? DoWriteValueDelimiterAsync(cancellationToken) : base.WriteValueDelimiterAsync(cancellationToken);
        }

        internal Task DoWriteValueDelimiterAsync(CancellationToken cancellationToken)
        {
            return _writer.WriteAsync(',', cancellationToken);
        }
        protected override Task WriteEndAsync(JsonToken token, CancellationToken cancellationToken)
        {
            return _safeAsync ? DoWriteEndAsync(token, cancellationToken) : base.WriteEndAsync(token, cancellationToken);
        }

        internal Task DoWriteEndAsync(JsonToken token, CancellationToken cancellationToken)
        {
            switch (token)
            {
                case JsonToken.EndObject:
                    return _writer.WriteAsync('}', cancellationToken);
                case JsonToken.EndArray:
                    return _writer.WriteAsync(']', cancellationToken);
                case JsonToken.EndConstructor:
                    return _writer.WriteAsync(')', cancellationToken);
                default:
                    throw JsonWriterException.Create(this, "Invalid JsonToken: " + token, null);
            }
        }
        public override Task CloseAsync(CancellationToken cancellationToken = default)
        {
            return _safeAsync ? DoCloseAsync(cancellationToken) : base.CloseAsync(cancellationToken);
        }

        internal async Task DoCloseAsync(CancellationToken cancellationToken)
        {
            if (Top == 0) // otherwise will happen in calls to WriteEndAsync
            {
                cancellationToken.ThrowIfCancellationRequested();
            }

            while (Top > 0)
            {
                await WriteEndAsync(cancellationToken).ConfigureAwait(false);
            }

            CloseBufferAndWriter();
        }
        public override Task WriteEndAsync(CancellationToken cancellationToken = default)
        {
            return _safeAsync ? WriteEndInternalAsync(cancellationToken) : base.WriteEndAsync(cancellationToken);
        }
        protected override Task WriteIndentAsync(CancellationToken cancellationToken)
        {
            return _safeAsync ? DoWriteIndentAsync(cancellationToken) : base.WriteIndentAsync(cancellationToken);
        }

        internal Task DoWriteIndentAsync(CancellationToken cancellationToken)
        {
            int currentIndentCount = Top * _indentation;

            int newLineLen = SetIndentChars();
            MiscellaneousUtils.Assert(_indentChars != null);

            if (currentIndentCount <= IndentCharBufferSize)
            {
                return _writer.WriteAsync(_indentChars, 0, newLineLen + currentIndentCount, cancellationToken);
            }

            return WriteIndentAsync(currentIndentCount, newLineLen, cancellationToken);
        }

        private async Task WriteIndentAsync(int currentIndentCount, int newLineLen, CancellationToken cancellationToken)
        {
            MiscellaneousUtils.Assert(_indentChars != null);

            await _writer.WriteAsync(_indentChars, 0, newLineLen + Math.Min(currentIndentCount, IndentCharBufferSize), cancellationToken).ConfigureAwait(false);

            while ((currentIndentCount -= IndentCharBufferSize) > 0)
            {
                await _writer.WriteAsync(_indentChars, newLineLen, Math.Min(currentIndentCount, IndentCharBufferSize), cancellationToken).ConfigureAwait(false);
            }
        }

        private Task WriteValueInternalAsync(JsonToken token, string value, CancellationToken cancellationToken)
        {
            Task task = InternalWriteValueAsync(token, cancellationToken);
            if (task.IsCompletedSucessfully())
            {
                return _writer.WriteAsync(value, cancellationToken);
            }

            return WriteValueInternalAsync(task, value, cancellationToken);
        }

        private async Task WriteValueInternalAsync(Task task, string value, CancellationToken cancellationToken)
        {
            await task.ConfigureAwait(false);
            await _writer.WriteAsync(value, cancellationToken).ConfigureAwait(false);
        }
        protected override Task WriteIndentSpaceAsync(CancellationToken cancellationToken)
        {
            return _safeAsync ? DoWriteIndentSpaceAsync(cancellationToken) : base.WriteIndentSpaceAsync(cancellationToken);
        }

        internal Task DoWriteIndentSpaceAsync(CancellationToken cancellationToken)
        {
            return _writer.WriteAsync(' ', cancellationToken);
        }
        public override Task WriteRawAsync(string? json, CancellationToken cancellationToken = default)
        {
            return _safeAsync ? DoWriteRawAsync(json, cancellationToken) : base.WriteRawAsync(json, cancellationToken);
        }

        internal Task DoWriteRawAsync(string? json, CancellationToken cancellationToken)
        {
            return _writer.WriteAsync(json, cancellationToken);
        }
        public override Task WriteNullAsync(CancellationToken cancellationToken = default)
        {
            return _safeAsync ? DoWriteNullAsync(cancellationToken) : base.WriteNullAsync(cancellationToken);
        }

        internal Task DoWriteNullAsync(CancellationToken cancellationToken)
        {
            return WriteValueInternalAsync(JsonToken.Null, JsonConvert.Null, cancellationToken);
        }

        private Task WriteDigitsAsync(ulong uvalue, bool negative, CancellationToken cancellationToken)
        {
            if (uvalue <= 9 & !negative)
            {
                return _writer.WriteAsync((char)('0' + uvalue), cancellationToken);
            }

            int length = WriteNumberToBuffer(uvalue, negative);
            return _writer.WriteAsync(_writeBuffer!, 0, length, cancellationToken);
        }

        private Task WriteIntegerValueAsync(ulong uvalue, bool negative, CancellationToken cancellationToken)
        {
            Task task = InternalWriteValueAsync(JsonToken.Integer, cancellationToken);
            if (task.IsCompletedSucessfully())
            {
                return WriteDigitsAsync(uvalue, negative, cancellationToken);
            }

            return WriteIntegerValueAsync(task, uvalue, negative, cancellationToken);
        }

        private async Task WriteIntegerValueAsync(Task task, ulong uvalue, bool negative, CancellationToken cancellationToken)
        {
            await task.ConfigureAwait(false);
            await WriteDigitsAsync(uvalue, negative, cancellationToken).ConfigureAwait(false);
        }

        internal Task WriteIntegerValueAsync(long value, CancellationToken cancellationToken)
        {
            bool negative = value < 0;
            if (negative)
            {
                value = -value;
            }

            return WriteIntegerValueAsync((ulong)value, negative, cancellationToken);
        }

        internal Task WriteIntegerValueAsync(ulong uvalue, CancellationToken cancellationToken)
        {
            return WriteIntegerValueAsync(uvalue, false, cancellationToken);
        }

        private Task WriteEscapedStringAsync(string value, bool quote, CancellationToken cancellationToken)
        {
            return JavaScriptUtils.WriteEscapedJavaScriptStringAsync(_writer, value, _quoteChar, quote, _charEscapeFlags!, StringEscapeHandling, this, _writeBuffer!, cancellationToken);
        }
        public override Task WritePropertyNameAsync(string name, CancellationToken cancellationToken = default)
        {
            return _safeAsync ? DoWritePropertyNameAsync(name, cancellationToken) : base.WritePropertyNameAsync(name, cancellationToken);
        }

        internal Task DoWritePropertyNameAsync(string name, CancellationToken cancellationToken)
        {
            Task task = InternalWritePropertyNameAsync(name, cancellationToken);
            if (!task.IsCompletedSucessfully())
            {
                return DoWritePropertyNameAsync(task, name, cancellationToken);
            }

            task = WriteEscapedStringAsync(name, _quoteName, cancellationToken);
            if (task.IsCompletedSucessfully())
            {
                return _writer.WriteAsync(':', cancellationToken);
            }

            return JavaScriptUtils.WriteCharAsync(task, _writer, ':', cancellationToken);
        }

        private async Task DoWritePropertyNameAsync(Task task, string name, CancellationToken cancellationToken)
        {
            await task.ConfigureAwait(false);

            await WriteEscapedStringAsync(name, _quoteName, cancellationToken).ConfigureAwait(false);

            await _writer.WriteAsync(':').ConfigureAwait(false);
        }
        public override Task WritePropertyNameAsync(string name, bool escape, CancellationToken cancellationToken = default)
        {
            return _safeAsync ? DoWritePropertyNameAsync(name, escape, cancellationToken) : base.WritePropertyNameAsync(name, escape, cancellationToken);
        }

        internal async Task DoWritePropertyNameAsync(string name, bool escape, CancellationToken cancellationToken)
        {
            await InternalWritePropertyNameAsync(name, cancellationToken).ConfigureAwait(false);

            if (escape)
            {
                await WriteEscapedStringAsync(name, _quoteName, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                if (_quoteName)
                {
                    await _writer.WriteAsync(_quoteChar).ConfigureAwait(false);
                }

                await _writer.WriteAsync(name, cancellationToken).ConfigureAwait(false);

                if (_quoteName)
                {
                    await _writer.WriteAsync(_quoteChar).ConfigureAwait(false);
                }
            }

            await _writer.WriteAsync(':').ConfigureAwait(false);
        }
        public override Task WriteStartArrayAsync(CancellationToken cancellationToken = default)
        {
            return _safeAsync ? DoWriteStartArrayAsync(cancellationToken) : base.WriteStartArrayAsync(cancellationToken);
        }

        internal Task DoWriteStartArrayAsync(CancellationToken cancellationToken)
        {
            Task task = InternalWriteStartAsync(JsonToken.StartArray, JsonContainerType.Array, cancellationToken);
            if (task.IsCompletedSucessfully())
            {
                return _writer.WriteAsync('[', cancellationToken);
            }

            return DoWriteStartArrayAsync(task, cancellationToken);
        }

        internal async Task DoWriteStartArrayAsync(Task task, CancellationToken cancellationToken)
        {
            await task.ConfigureAwait(false);

            await _writer.WriteAsync('[', cancellationToken).ConfigureAwait(false);
        }
        public override Task WriteStartObjectAsync(CancellationToken cancellationToken = default)
        {
            return _safeAsync ? DoWriteStartObjectAsync(cancellationToken) : base.WriteStartObjectAsync(cancellationToken);
        }

        internal Task DoWriteStartObjectAsync(CancellationToken cancellationToken)
        {
            Task task = InternalWriteStartAsync(JsonToken.StartObject, JsonContainerType.Object, cancellationToken);
            if (task.IsCompletedSucessfully())
            {
                return _writer.WriteAsync('{', cancellationToken);
            }

            return DoWriteStartObjectAsync(task, cancellationToken);
        }

        internal async Task DoWriteStartObjectAsync(Task task, CancellationToken cancellationToken)
        {
            await task.ConfigureAwait(false);

            await _writer.WriteAsync('{', cancellationToken).ConfigureAwait(false);
        }
        public override Task WriteStartConstructorAsync(string name, CancellationToken cancellationToken = default)
        {
            return _safeAsync ? DoWriteStartConstructorAsync(name, cancellationToken) : base.WriteStartConstructorAsync(name, cancellationToken);
        }

        internal async Task DoWriteStartConstructorAsync(string name, CancellationToken cancellationToken)
        {
            await InternalWriteStartAsync(JsonToken.StartConstructor, JsonContainerType.Constructor, cancellationToken).ConfigureAwait(false);

            await _writer.WriteAsync("new ", cancellationToken).ConfigureAwait(false);
            await _writer.WriteAsync(name, cancellationToken).ConfigureAwait(false);
            await _writer.WriteAsync('(').ConfigureAwait(false);
        }
        public override Task WriteUndefinedAsync(CancellationToken cancellationToken = default)
        {
            return _safeAsync ? DoWriteUndefinedAsync(cancellationToken) : base.WriteUndefinedAsync(cancellationToken);
        }

        internal Task DoWriteUndefinedAsync(CancellationToken cancellationToken)
        {
            Task task = InternalWriteValueAsync(JsonToken.Undefined, cancellationToken);
            if (task.IsCompletedSucessfully())
            {
                return _writer.WriteAsync(JsonConvert.Undefined, cancellationToken);
            }

            return DoWriteUndefinedAsync(task, cancellationToken);
        }

        private async Task DoWriteUndefinedAsync(Task task, CancellationToken cancellationToken)
        {
            await task.ConfigureAwait(false);
            await _writer.WriteAsync(JsonConvert.Undefined, cancellationToken).ConfigureAwait(false);
        }
        public override Task WriteWhitespaceAsync(string ws, CancellationToken cancellationToken = default)
        {
            return _safeAsync ? DoWriteWhitespaceAsync(ws, cancellationToken) : base.WriteWhitespaceAsync(ws, cancellationToken);
        }

        internal Task DoWriteWhitespaceAsync(string ws, CancellationToken cancellationToken)
        {
            InternalWriteWhitespace(ws);
            return _writer.WriteAsync(ws, cancellationToken);
        }
        public override Task WriteValueAsync(bool value, CancellationToken cancellationToken = default)
        {
            return _safeAsync ? DoWriteValueAsync(value, cancellationToken) : base.WriteValueAsync(value, cancellationToken);
        }

        internal Task DoWriteValueAsync(bool value, CancellationToken cancellationToken)
        {
            return WriteValueInternalAsync(JsonToken.Boolean, JsonConvert.ToString(value), cancellationToken);
        }
        public override Task WriteValueAsync(bool? value, CancellationToken cancellationToken = default)
        {
            return _safeAsync ? DoWriteValueAsync(value, cancellationToken) : base.WriteValueAsync(value, cancellationToken);
        }

        internal Task DoWriteValueAsync(bool? value, CancellationToken cancellationToken)
        {
            return value == null ? DoWriteNullAsync(cancellationToken) : DoWriteValueAsync(value.GetValueOrDefault(), cancellationToken);
        }
        public override Task WriteValueAsync(byte value, CancellationToken cancellationToken = default)
        {
            return _safeAsync ? WriteIntegerValueAsync(value, cancellationToken) : base.WriteValueAsync(value, cancellationToken);
        }
        public override Task WriteValueAsync(byte? value, CancellationToken cancellationToken = default)
        {
            return _safeAsync ? DoWriteValueAsync(value, cancellationToken) : base.WriteValueAsync(value, cancellationToken);
        }

        internal Task DoWriteValueAsync(byte? value, CancellationToken cancellationToken)
        {
            return value == null ? DoWriteNullAsync(cancellationToken) : WriteIntegerValueAsync(value.GetValueOrDefault(), cancellationToken);
        }
        public override Task WriteValueAsync(byte[]? value, CancellationToken cancellationToken = default)
        {
            return _safeAsync ? (value == null ? WriteNullAsync(cancellationToken) : WriteValueNonNullAsync(value, cancellationToken)) : base.WriteValueAsync(value, cancellationToken);
        }

        internal async Task WriteValueNonNullAsync(byte[] value, CancellationToken cancellationToken)
        {
            await InternalWriteValueAsync(JsonToken.Bytes, cancellationToken).ConfigureAwait(false);
            await _writer.WriteAsync(_quoteChar).ConfigureAwait(false);
            await Base64Encoder.EncodeAsync(value, 0, value.Length, cancellationToken).ConfigureAwait(false);
            await Base64Encoder.FlushAsync(cancellationToken).ConfigureAwait(false);
            await _writer.WriteAsync(_quoteChar).ConfigureAwait(false);
        }
        public override Task WriteValueAsync(char value, CancellationToken cancellationToken = default)
        {
            return _safeAsync ? DoWriteValueAsync(value, cancellationToken) : base.WriteValueAsync(value, cancellationToken);
        }

        internal Task DoWriteValueAsync(char value, CancellationToken cancellationToken)
        {
            return WriteValueInternalAsync(JsonToken.String, JsonConvert.ToString(value), cancellationToken);
        }
        public override Task WriteValueAsync(char? value, CancellationToken cancellationToken = default)
        {
            return _safeAsync ? DoWriteValueAsync(value, cancellationToken) : base.WriteValueAsync(value, cancellationToken);
        }

        internal Task DoWriteValueAsync(char? value, CancellationToken cancellationToken)
        {
            return value == null ? DoWriteNullAsync(cancellationToken) : DoWriteValueAsync(value.GetValueOrDefault(), cancellationToken);
        }
        public override Task WriteValueAsync(DateTime value, CancellationToken cancellationToken = default)
        {
            return _safeAsync ? DoWriteValueAsync(value, cancellationToken) : base.WriteValueAsync(value, cancellationToken);
        }

        internal async Task DoWriteValueAsync(DateTime value, CancellationToken cancellationToken)
        {
            await InternalWriteValueAsync(JsonToken.Date, cancellationToken).ConfigureAwait(false);
            value = DateTimeUtils.EnsureDateTime(value, DateTimeZoneHandling);

            if (StringUtils.IsNullOrEmpty(DateFormatString))
            {
                int length = WriteValueToBuffer(value);

                await _writer.WriteAsync(_writeBuffer!, 0, length, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                await _writer.WriteAsync(_quoteChar).ConfigureAwait(false);
                await _writer.WriteAsync(value.ToString(DateFormatString, Culture), cancellationToken).ConfigureAwait(false);
                await _writer.WriteAsync(_quoteChar).ConfigureAwait(false);
            }
        }
        public override Task WriteValueAsync(DateTime? value, CancellationToken cancellationToken = default)
        {
            return _safeAsync ? DoWriteValueAsync(value, cancellationToken) : base.WriteValueAsync(value, cancellationToken);
        }

        internal Task DoWriteValueAsync(DateTime? value, CancellationToken cancellationToken)
        {
            return value == null ? DoWriteNullAsync(cancellationToken) : DoWriteValueAsync(value.GetValueOrDefault(), cancellationToken);
        }
        public override Task WriteValueAsync(DateTimeOffset value, CancellationToken cancellationToken = default)
        {
            return _safeAsync ? DoWriteValueAsync(value, cancellationToken) : base.WriteValueAsync(value, cancellationToken);
        }

        internal async Task DoWriteValueAsync(DateTimeOffset value, CancellationToken cancellationToken)
        {
            await InternalWriteValueAsync(JsonToken.Date, cancellationToken).ConfigureAwait(false);

            if (StringUtils.IsNullOrEmpty(DateFormatString))
            {
                int length = WriteValueToBuffer(value);

                await _writer.WriteAsync(_writeBuffer!, 0, length, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                await _writer.WriteAsync(_quoteChar).ConfigureAwait(false);
                await _writer.WriteAsync(value.ToString(DateFormatString, Culture), cancellationToken).ConfigureAwait(false);
                await _writer.WriteAsync(_quoteChar).ConfigureAwait(false);
            }
        }
        public override Task WriteValueAsync(DateTimeOffset? value, CancellationToken cancellationToken = default)
        {
            return _safeAsync ? DoWriteValueAsync(value, cancellationToken) : base.WriteValueAsync(value, cancellationToken);
        }

        internal Task DoWriteValueAsync(DateTimeOffset? value, CancellationToken cancellationToken)
        {
            return value == null ? DoWriteNullAsync(cancellationToken) : DoWriteValueAsync(value.GetValueOrDefault(), cancellationToken);
        }
        public override Task WriteValueAsync(decimal value, CancellationToken cancellationToken = default)
        {
            return _safeAsync ? DoWriteValueAsync(value, cancellationToken) : base.WriteValueAsync(value, cancellationToken);
        }

        internal Task DoWriteValueAsync(decimal value, CancellationToken cancellationToken)
        {
            return WriteValueInternalAsync(JsonToken.Float, JsonConvert.ToString(value), cancellationToken);
        }
        public override Task WriteValueAsync(decimal? value, CancellationToken cancellationToken = default)
        {
            return _safeAsync ? DoWriteValueAsync(value, cancellationToken) : base.WriteValueAsync(value, cancellationToken);
        }

        internal Task DoWriteValueAsync(decimal? value, CancellationToken cancellationToken)
        {
            return value == null ? DoWriteNullAsync(cancellationToken) : DoWriteValueAsync(value.GetValueOrDefault(), cancellationToken);
        }
        public override Task WriteValueAsync(double value, CancellationToken cancellationToken = default)
        {
            return _safeAsync ? WriteValueAsync(value, false, cancellationToken) : base.WriteValueAsync(value, cancellationToken);
        }

        internal Task WriteValueAsync(double value, bool nullable, CancellationToken cancellationToken)
        {
            return WriteValueInternalAsync(JsonToken.Float, JsonConvert.ToString(value, FloatFormatHandling, QuoteChar, nullable), cancellationToken);
        }
        public override Task WriteValueAsync(double? value, CancellationToken cancellationToken = default)
        {
            return _safeAsync ? (value.HasValue ? WriteValueAsync(value.GetValueOrDefault(), true, cancellationToken) : WriteNullAsync(cancellationToken)) : base.WriteValueAsync(value, cancellationToken);
        }
        public override Task WriteValueAsync(float value, CancellationToken cancellationToken = default)
        {
            return _safeAsync ? WriteValueAsync(value, false, cancellationToken) : base.WriteValueAsync(value, cancellationToken);
        }

        internal Task WriteValueAsync(float value, bool nullable, CancellationToken cancellationToken)
        {
            return WriteValueInternalAsync(JsonToken.Float, JsonConvert.ToString(value, FloatFormatHandling, QuoteChar, nullable), cancellationToken);
        }
        public override Task WriteValueAsync(float? value, CancellationToken cancellationToken = default)
        {
            return _safeAsync ? (value.HasValue ? WriteValueAsync(value.GetValueOrDefault(), true, cancellationToken) : WriteNullAsync(cancellationToken)) : base.WriteValueAsync(value, cancellationToken);
        }
        public override Task WriteValueAsync(Guid value, CancellationToken cancellationToken = default)
        {
            return _safeAsync ? DoWriteValueAsync(value, cancellationToken) : base.WriteValueAsync(value, cancellationToken);
        }

        internal async Task DoWriteValueAsync(Guid value, CancellationToken cancellationToken)
        {
            await InternalWriteValueAsync(JsonToken.String, cancellationToken).ConfigureAwait(false);

            await _writer.WriteAsync(_quoteChar).ConfigureAwait(false);
#if HAVE_CHAR_TO_STRING_WITH_CULTURE
            await _writer.WriteAsync(value.ToString("D", CultureInfo.InvariantCulture), cancellationToken).ConfigureAwait(false);
#else
            await _writer.WriteAsync(value.ToString("D"), cancellationToken).ConfigureAwait(false);
#endif
            await _writer.WriteAsync(_quoteChar).ConfigureAwait(false);
        }
        public override Task WriteValueAsync(Guid? value, CancellationToken cancellationToken = default)
        {
            return _safeAsync ? DoWriteValueAsync(value, cancellationToken) : base.WriteValueAsync(value, cancellationToken);
        }

        internal Task DoWriteValueAsync(Guid? value, CancellationToken cancellationToken)
        {
            return value == null ? DoWriteNullAsync(cancellationToken) : DoWriteValueAsync(value.GetValueOrDefault(), cancellationToken);
        }
        public override Task WriteValueAsync(int value, CancellationToken cancellationToken = default)
        {
            return _safeAsync ? WriteIntegerValueAsync(value, cancellationToken) : base.WriteValueAsync(value, cancellationToken);
        }
        public override Task WriteValueAsync(int? value, CancellationToken cancellationToken = default)
        {
            return _safeAsync ? DoWriteValueAsync(value, cancellationToken) : base.WriteValueAsync(value, cancellationToken);
        }

        internal Task DoWriteValueAsync(int? value, CancellationToken cancellationToken)
        {
            return value == null ? DoWriteNullAsync(cancellationToken) : WriteIntegerValueAsync(value.GetValueOrDefault(), cancellationToken);
        }
        public override Task WriteValueAsync(long value, CancellationToken cancellationToken = default)
        {
            return _safeAsync ? WriteIntegerValueAsync(value, cancellationToken) : base.WriteValueAsync(value, cancellationToken);
        }
        public override Task WriteValueAsync(long? value, CancellationToken cancellationToken = default)
        {
            return _safeAsync ? DoWriteValueAsync(value, cancellationToken) : base.WriteValueAsync(value, cancellationToken);
        }

        internal Task DoWriteValueAsync(long? value, CancellationToken cancellationToken)
        {
            return value == null ? DoWriteNullAsync(cancellationToken) : WriteIntegerValueAsync(value.GetValueOrDefault(), cancellationToken);
        }

#if HAVE_BIG_INTEGER
        internal Task WriteValueAsync(BigInteger value, CancellationToken cancellationToken)
        {
            return WriteValueInternalAsync(JsonToken.Integer, value.ToString(CultureInfo.InvariantCulture), cancellationToken);
        }
#endif
        public override Task WriteValueAsync(object? value, CancellationToken cancellationToken = default)
        {
            if (_safeAsync)
            {
                if (value == null)
                {
                    return WriteNullAsync(cancellationToken);
                }
#if HAVE_BIG_INTEGER
                if (value is BigInteger i)
                {
                    return WriteValueAsync(i, cancellationToken);
                }
#endif

                return WriteValueAsync(this, ConvertUtils.GetTypeCode(value.GetType()), value, cancellationToken);
            }

            return base.WriteValueAsync(value, cancellationToken);
        }
        [CLSCompliant(false)]
        public override Task WriteValueAsync(sbyte value, CancellationToken cancellationToken = default)
        {
            return _safeAsync ? WriteIntegerValueAsync(value, cancellationToken) : base.WriteValueAsync(value, cancellationToken);
        }
        [CLSCompliant(false)]
        public override Task WriteValueAsync(sbyte? value, CancellationToken cancellationToken = default)
        {
            return _safeAsync ? DoWriteValueAsync(value, cancellationToken) : base.WriteValueAsync(value, cancellationToken);
        }

        internal Task DoWriteValueAsync(sbyte? value, CancellationToken cancellationToken)
        {
            return value == null ? DoWriteNullAsync(cancellationToken) : WriteIntegerValueAsync(value.GetValueOrDefault(), cancellationToken);
        }
        public override Task WriteValueAsync(short value, CancellationToken cancellationToken = default)
        {
            return _safeAsync ? WriteIntegerValueAsync(value, cancellationToken) : base.WriteValueAsync(value, cancellationToken);
        }
        public override Task WriteValueAsync(short? value, CancellationToken cancellationToken = default)
        {
            return _safeAsync ? DoWriteValueAsync(value, cancellationToken) : base.WriteValueAsync(value, cancellationToken);
        }

        internal Task DoWriteValueAsync(short? value, CancellationToken cancellationToken)
        {
            return value == null ? DoWriteNullAsync(cancellationToken) : WriteIntegerValueAsync(value.GetValueOrDefault(), cancellationToken);
        }
        public override Task WriteValueAsync(string? value, CancellationToken cancellationToken = default)
        {
            return _safeAsync ? DoWriteValueAsync(value, cancellationToken) : base.WriteValueAsync(value, cancellationToken);
        }

        internal Task DoWriteValueAsync(string? value, CancellationToken cancellationToken)
        {
            Task task = InternalWriteValueAsync(JsonToken.String, cancellationToken);
            if (task.IsCompletedSucessfully())
            {
                return value == null ? _writer.WriteAsync(JsonConvert.Null, cancellationToken) : WriteEscapedStringAsync(value, true, cancellationToken);
            }

            return DoWriteValueAsync(task, value, cancellationToken);
        }

        private async Task DoWriteValueAsync(Task task, string? value, CancellationToken cancellationToken)
        {
            await task.ConfigureAwait(false);
            await (value == null ? _writer.WriteAsync(JsonConvert.Null, cancellationToken) : WriteEscapedStringAsync(value, true, cancellationToken)).ConfigureAwait(false);
        }
        public override Task WriteValueAsync(TimeSpan value, CancellationToken cancellationToken = default)
        {
            return _safeAsync ? DoWriteValueAsync(value, cancellationToken) : base.WriteValueAsync(value, cancellationToken);
        }

        internal async Task DoWriteValueAsync(TimeSpan value, CancellationToken cancellationToken)
        {
            await InternalWriteValueAsync(JsonToken.String, cancellationToken).ConfigureAwait(false);
            await _writer.WriteAsync(_quoteChar, cancellationToken).ConfigureAwait(false);
            await _writer.WriteAsync(value.ToString(null, CultureInfo.InvariantCulture), cancellationToken).ConfigureAwait(false);
            await _writer.WriteAsync(_quoteChar, cancellationToken).ConfigureAwait(false);
        }
        public override Task WriteValueAsync(TimeSpan? value, CancellationToken cancellationToken = default)
        {
            return _safeAsync ? DoWriteValueAsync(value, cancellationToken) : base.WriteValueAsync(value, cancellationToken);
        }

        internal Task DoWriteValueAsync(TimeSpan? value, CancellationToken cancellationToken)
        {
            return value == null ? DoWriteNullAsync(cancellationToken) : DoWriteValueAsync(value.GetValueOrDefault(), cancellationToken);
        }
        [CLSCompliant(false)]
        public override Task WriteValueAsync(uint value, CancellationToken cancellationToken = default)
        {
            return _safeAsync ? WriteIntegerValueAsync(value, cancellationToken) : base.WriteValueAsync(value, cancellationToken);
        }
        [CLSCompliant(false)]
        public override Task WriteValueAsync(uint? value, CancellationToken cancellationToken = default)
        {
            return _safeAsync ? DoWriteValueAsync(value, cancellationToken) : base.WriteValueAsync(value, cancellationToken);
        }

        internal Task DoWriteValueAsync(uint? value, CancellationToken cancellationToken)
        {
            return value == null ? DoWriteNullAsync(cancellationToken) : WriteIntegerValueAsync(value.GetValueOrDefault(), cancellationToken);
        }
        [CLSCompliant(false)]
        public override Task WriteValueAsync(ulong value, CancellationToken cancellationToken = default)
        {
            return _safeAsync ? WriteIntegerValueAsync(value, cancellationToken) : base.WriteValueAsync(value, cancellationToken);
        }
        [CLSCompliant(false)]
        public override Task WriteValueAsync(ulong? value, CancellationToken cancellationToken = default)
        {
            return _safeAsync ? DoWriteValueAsync(value, cancellationToken) : base.WriteValueAsync(value, cancellationToken);
        }

        internal Task DoWriteValueAsync(ulong? value, CancellationToken cancellationToken)
        {
            return value == null ? DoWriteNullAsync(cancellationToken) : WriteIntegerValueAsync(value.GetValueOrDefault(), cancellationToken);
        }
        public override Task WriteValueAsync(Uri? value, CancellationToken cancellationToken = default)
        {
            return _safeAsync ? (value == null ? WriteNullAsync(cancellationToken) : WriteValueNotNullAsync(value, cancellationToken)) : base.WriteValueAsync(value, cancellationToken);
        }

        internal Task WriteValueNotNullAsync(Uri value, CancellationToken cancellationToken)
        {
            Task task = InternalWriteValueAsync(JsonToken.String, cancellationToken);
            if (task.IsCompletedSucessfully())
            {
                return WriteEscapedStringAsync(value.OriginalString, true, cancellationToken);
            }

            return WriteValueNotNullAsync(task, value, cancellationToken);
        }

        internal async Task WriteValueNotNullAsync(Task task, Uri value, CancellationToken cancellationToken)
        {
            await task.ConfigureAwait(false);
            await WriteEscapedStringAsync(value.OriginalString, true, cancellationToken).ConfigureAwait(false);
        }
        [CLSCompliant(false)]
        public override Task WriteValueAsync(ushort value, CancellationToken cancellationToken = default)
        {
            return _safeAsync ? WriteIntegerValueAsync(value, cancellationToken) : base.WriteValueAsync(value, cancellationToken);
        }
        [CLSCompliant(false)]
        public override Task WriteValueAsync(ushort? value, CancellationToken cancellationToken = default)
        {
            return _safeAsync ? DoWriteValueAsync(value, cancellationToken) : base.WriteValueAsync(value, cancellationToken);
        }

        internal Task DoWriteValueAsync(ushort? value, CancellationToken cancellationToken)
        {
            return value == null ? DoWriteNullAsync(cancellationToken) : WriteIntegerValueAsync(value.GetValueOrDefault(), cancellationToken);
        }
        public override Task WriteCommentAsync(string? text, CancellationToken cancellationToken = default)
        {
            return _safeAsync ? DoWriteCommentAsync(text, cancellationToken) : base.WriteCommentAsync(text, cancellationToken);
        }

        internal async Task DoWriteCommentAsync(string? text, CancellationToken cancellationToken)
        {
            await InternalWriteCommentAsync(cancellationToken).ConfigureAwait(false);
            await _writer.WriteAsync("/*", cancellationToken).ConfigureAwait(false);
            await _writer.WriteAsync(text ?? string.Empty, cancellationToken).ConfigureAwait(false);
            await _writer.WriteAsync("*/", cancellationToken).ConfigureAwait(false);
        }
        public override Task WriteEndArrayAsync(CancellationToken cancellationToken = default)
        {
            return _safeAsync ? InternalWriteEndAsync(JsonContainerType.Array, cancellationToken) : base.WriteEndArrayAsync(cancellationToken);
        }
        public override Task WriteEndConstructorAsync(CancellationToken cancellationToken = default)
        {
            return _safeAsync ? InternalWriteEndAsync(JsonContainerType.Constructor, cancellationToken) : base.WriteEndConstructorAsync(cancellationToken);
        }
        public override Task WriteEndObjectAsync(CancellationToken cancellationToken = default)
        {
            return _safeAsync ? InternalWriteEndAsync(JsonContainerType.Object, cancellationToken) : base.WriteEndObjectAsync(cancellationToken);
        }
        public override Task WriteRawValueAsync(string? json, CancellationToken cancellationToken = default)
        {
            return _safeAsync ? DoWriteRawValueAsync(json, cancellationToken) : base.WriteRawValueAsync(json, cancellationToken);
        }

        internal Task DoWriteRawValueAsync(string? json, CancellationToken cancellationToken)
        {
            UpdateScopeWithFinishedValue();
            Task task = AutoCompleteAsync(JsonToken.Undefined, cancellationToken);
            if (task.IsCompletedSucessfully())
            {
                return WriteRawAsync(json, cancellationToken);
            }

            return DoWriteRawValueAsync(task, json, cancellationToken);
        }

        private async Task DoWriteRawValueAsync(Task task, string? json, CancellationToken cancellationToken)
        {
            await task.ConfigureAwait(false);
            await WriteRawAsync(json, cancellationToken).ConfigureAwait(false);
        }

        internal char[] EnsureWriteBuffer(int length, int copyTo)
        {
            if (length < 35)
            {
                length = 35;
            }

            char[]? buffer = _writeBuffer;
            if (buffer == null)
            {
                return _writeBuffer = BufferUtils.RentBuffer(_arrayPool, length);
            }

            if (buffer.Length >= length)
            {
                return buffer;
            }

            char[] newBuffer = BufferUtils.RentBuffer(_arrayPool, length);
            if (copyTo != 0)
            {
                Array.Copy(buffer, newBuffer, copyTo);
            }

            BufferUtils.ReturnBuffer(_arrayPool, buffer);
            _writeBuffer = newBuffer;
            return newBuffer;
        }
    }
}
#endif