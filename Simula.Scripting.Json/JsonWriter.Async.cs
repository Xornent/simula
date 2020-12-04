
#if HAVE_ASYNC

using System;
using System.Globalization;
using System.Threading;
#if HAVE_BIG_INTEGER
using System.Numerics;
#endif
using System.Threading.Tasks;
using Simula.Scripting.Json.Utilities;

namespace Simula.Scripting.Json
{
    public abstract partial class JsonWriter
    {
        internal Task AutoCompleteAsync(JsonToken tokenBeingWritten, CancellationToken cancellationToken)
        {
            State oldState = _currentState;
            State newState = StateArray[(int)tokenBeingWritten][(int)oldState];

            if (newState == State.Error) {
                throw JsonWriterException.Create(this, "Token {0} in state {1} would result in an invalid JSON object.".FormatWith(CultureInfo.InvariantCulture, tokenBeingWritten.ToString(), oldState.ToString()), null);
            }

            _currentState = newState;

            if (_formatting == Formatting.Indented) {
                switch (oldState) {
                    case State.Start:
                        break;
                    case State.Property:
                        return WriteIndentSpaceAsync(cancellationToken);
                    case State.ArrayStart:
                    case State.ConstructorStart:
                        return WriteIndentAsync(cancellationToken);
                    case State.Array:
                    case State.Constructor:
                        return tokenBeingWritten == JsonToken.Comment ? WriteIndentAsync(cancellationToken) : AutoCompleteAsync(cancellationToken);
                    case State.Object:
                        switch (tokenBeingWritten) {
                            case JsonToken.Comment:
                                break;
                            case JsonToken.PropertyName:
                                return AutoCompleteAsync(cancellationToken);
                            default:
                                return WriteValueDelimiterAsync(cancellationToken);
                        }

                        break;
                    default:
                        if (tokenBeingWritten == JsonToken.PropertyName) {
                            return WriteIndentAsync(cancellationToken);
                        }

                        break;
                }
            } else if (tokenBeingWritten != JsonToken.Comment) {
                switch (oldState) {
                    case State.Object:
                    case State.Array:
                    case State.Constructor:
                        return WriteValueDelimiterAsync(cancellationToken);
                }
            }

            return AsyncUtils.CompletedTask;
        }

        private async Task AutoCompleteAsync(CancellationToken cancellationToken)
        {
            await WriteValueDelimiterAsync(cancellationToken).ConfigureAwait(false);
            await WriteIndentAsync(cancellationToken).ConfigureAwait(false);
        }
        public virtual Task CloseAsync(CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested) {
                return cancellationToken.FromCanceled();
            }

            Close();
            return AsyncUtils.CompletedTask;
        }
        public virtual Task FlushAsync(CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested) {
                return cancellationToken.FromCanceled();
            }

            Flush();
            return AsyncUtils.CompletedTask;
        }
        protected virtual Task WriteEndAsync(JsonToken token, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) {
                return cancellationToken.FromCanceled();
            }

            WriteEnd(token);
            return AsyncUtils.CompletedTask;
        }
        protected virtual Task WriteIndentAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) {
                return cancellationToken.FromCanceled();
            }

            WriteIndent();
            return AsyncUtils.CompletedTask;
        }
        protected virtual Task WriteValueDelimiterAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) {
                return cancellationToken.FromCanceled();
            }

            WriteValueDelimiter();
            return AsyncUtils.CompletedTask;
        }
        protected virtual Task WriteIndentSpaceAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) {
                return cancellationToken.FromCanceled();
            }

            WriteIndentSpace();
            return AsyncUtils.CompletedTask;
        }
        public virtual Task WriteRawAsync(string? json, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested) {
                return cancellationToken.FromCanceled();
            }

            WriteRaw(json);
            return AsyncUtils.CompletedTask;
        }
        public virtual Task WriteEndAsync(CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested) {
                return cancellationToken.FromCanceled();
            }

            WriteEnd();
            return AsyncUtils.CompletedTask;
        }

        internal Task WriteEndInternalAsync(CancellationToken cancellationToken)
        {
            JsonContainerType type = Peek();
            switch (type) {
                case JsonContainerType.Object:
                    return WriteEndObjectAsync(cancellationToken);
                case JsonContainerType.Array:
                    return WriteEndArrayAsync(cancellationToken);
                case JsonContainerType.Constructor:
                    return WriteEndConstructorAsync(cancellationToken);
                default:
                    if (cancellationToken.IsCancellationRequested) {
                        return cancellationToken.FromCanceled();
                    }

                    throw JsonWriterException.Create(this, "Unexpected type when writing end: " + type, null);
            }
        }

        internal Task InternalWriteEndAsync(JsonContainerType type, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) {
                return cancellationToken.FromCanceled();
            }

            int levelsToComplete = CalculateLevelsToComplete(type);
            while (levelsToComplete-- > 0) {
                JsonToken token = GetCloseTokenForType(Pop());

                Task t;
                if (_currentState == State.Property) {
                    t = WriteNullAsync(cancellationToken);
                    if (!t.IsCompletedSucessfully()) {
                        return AwaitProperty(t, levelsToComplete, token, cancellationToken);
                    }
                }

                if (_formatting == Formatting.Indented) {
                    if (_currentState != State.ObjectStart && _currentState != State.ArrayStart) {
                        t = WriteIndentAsync(cancellationToken);
                        if (!t.IsCompletedSucessfully()) {
                            return AwaitIndent(t, levelsToComplete, token, cancellationToken);
                        }
                    }
                }

                t = WriteEndAsync(token, cancellationToken);
                if (!t.IsCompletedSucessfully()) {
                    return AwaitEnd(t, levelsToComplete, cancellationToken);
                }

                UpdateCurrentState();
            }

            return AsyncUtils.CompletedTask;
            async Task AwaitProperty(Task task, int LevelsToComplete, JsonToken token, CancellationToken CancellationToken)
            {
                await task.ConfigureAwait(false);
                if (_formatting == Formatting.Indented) {
                    if (_currentState != State.ObjectStart && _currentState != State.ArrayStart) {
                        await WriteIndentAsync(CancellationToken).ConfigureAwait(false);
                    }
                }

                await WriteEndAsync(token, CancellationToken).ConfigureAwait(false);

                UpdateCurrentState();

                await AwaitRemaining(LevelsToComplete, CancellationToken).ConfigureAwait(false);
            }

            async Task AwaitIndent(Task task, int LevelsToComplete, JsonToken token, CancellationToken CancellationToken)
            {
                await task.ConfigureAwait(false);

                await WriteEndAsync(token, CancellationToken).ConfigureAwait(false);

                UpdateCurrentState();

                await AwaitRemaining(LevelsToComplete, CancellationToken).ConfigureAwait(false);
            }

            async Task AwaitEnd(Task task, int LevelsToComplete, CancellationToken CancellationToken)
            {
                await task.ConfigureAwait(false);

                UpdateCurrentState();

                await AwaitRemaining(LevelsToComplete, CancellationToken).ConfigureAwait(false);
            }

            async Task AwaitRemaining(int LevelsToComplete, CancellationToken CancellationToken)
            {
                while (LevelsToComplete-- > 0) {
                    JsonToken token = GetCloseTokenForType(Pop());

                    if (_currentState == State.Property) {
                        await WriteNullAsync(CancellationToken).ConfigureAwait(false);
                    }

                    if (_formatting == Formatting.Indented) {
                        if (_currentState != State.ObjectStart && _currentState != State.ArrayStart) {
                            await WriteIndentAsync(CancellationToken).ConfigureAwait(false);
                        }
                    }

                    await WriteEndAsync(token, CancellationToken).ConfigureAwait(false);

                    UpdateCurrentState();
                }
            }
        }
        public virtual Task WriteEndArrayAsync(CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested) {
                return cancellationToken.FromCanceled();
            }

            WriteEndArray();
            return AsyncUtils.CompletedTask;
        }
        public virtual Task WriteEndConstructorAsync(CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested) {
                return cancellationToken.FromCanceled();
            }

            WriteEndConstructor();
            return AsyncUtils.CompletedTask;
        }
        public virtual Task WriteEndObjectAsync(CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested) {
                return cancellationToken.FromCanceled();
            }

            WriteEndObject();
            return AsyncUtils.CompletedTask;
        }
        public virtual Task WriteNullAsync(CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested) {
                return cancellationToken.FromCanceled();
            }

            WriteNull();
            return AsyncUtils.CompletedTask;
        }
        public virtual Task WritePropertyNameAsync(string name, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested) {
                return cancellationToken.FromCanceled();
            }

            WritePropertyName(name);
            return AsyncUtils.CompletedTask;
        }
        public virtual Task WritePropertyNameAsync(string name, bool escape, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested) {
                return cancellationToken.FromCanceled();
            }

            WritePropertyName(name, escape);
            return AsyncUtils.CompletedTask;
        }

        internal Task InternalWritePropertyNameAsync(string name, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) {
                return cancellationToken.FromCanceled();
            }

            _currentPosition.PropertyName = name;
            return AutoCompleteAsync(JsonToken.PropertyName, cancellationToken);
        }
        public virtual Task WriteStartArrayAsync(CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested) {
                return cancellationToken.FromCanceled();
            }

            WriteStartArray();
            return AsyncUtils.CompletedTask;
        }

        internal async Task InternalWriteStartAsync(JsonToken token, JsonContainerType container, CancellationToken cancellationToken)
        {
            UpdateScopeWithFinishedValue();
            await AutoCompleteAsync(token, cancellationToken).ConfigureAwait(false);
            Push(container);
        }
        public virtual Task WriteCommentAsync(string? text, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested) {
                return cancellationToken.FromCanceled();
            }

            WriteComment(text);
            return AsyncUtils.CompletedTask;
        }

        internal Task InternalWriteCommentAsync(CancellationToken cancellationToken)
        {
            return AutoCompleteAsync(JsonToken.Comment, cancellationToken);
        }
        public virtual Task WriteRawValueAsync(string? json, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested) {
                return cancellationToken.FromCanceled();
            }

            WriteRawValue(json);
            return AsyncUtils.CompletedTask;
        }
        public virtual Task WriteStartConstructorAsync(string name, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested) {
                return cancellationToken.FromCanceled();
            }

            WriteStartConstructor(name);
            return AsyncUtils.CompletedTask;
        }
        public virtual Task WriteStartObjectAsync(CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested) {
                return cancellationToken.FromCanceled();
            }

            WriteStartObject();
            return AsyncUtils.CompletedTask;
        }
        public Task WriteTokenAsync(JsonReader reader, CancellationToken cancellationToken = default)
        {
            return WriteTokenAsync(reader, true, cancellationToken);
        }
        public Task WriteTokenAsync(JsonReader reader, bool writeChildren, CancellationToken cancellationToken = default)
        {
            ValidationUtils.ArgumentNotNull(reader, nameof(reader));

            return WriteTokenAsync(reader, writeChildren, true, true, cancellationToken);
        }
        public Task WriteTokenAsync(JsonToken token, CancellationToken cancellationToken = default)
        {
            return WriteTokenAsync(token, null, cancellationToken);
        }
        public Task WriteTokenAsync(JsonToken token, object? value, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested) {
                return cancellationToken.FromCanceled();
            }

            switch (token) {
                case JsonToken.None:
                    return AsyncUtils.CompletedTask;
                case JsonToken.StartObject:
                    return WriteStartObjectAsync(cancellationToken);
                case JsonToken.StartArray:
                    return WriteStartArrayAsync(cancellationToken);
                case JsonToken.StartConstructor:
                    ValidationUtils.ArgumentNotNull(value, nameof(value));
                    return WriteStartConstructorAsync(value.ToString(), cancellationToken);
                case JsonToken.PropertyName:
                    ValidationUtils.ArgumentNotNull(value, nameof(value));
                    return WritePropertyNameAsync(value.ToString(), cancellationToken);
                case JsonToken.Comment:
                    return WriteCommentAsync(value?.ToString(), cancellationToken);
                case JsonToken.Integer:
                    ValidationUtils.ArgumentNotNull(value, nameof(value));
                    return
#if HAVE_BIG_INTEGER
                        value is BigInteger integer ? WriteValueAsync(integer, cancellationToken) :
#endif
                        WriteValueAsync(Convert.ToInt64(value, CultureInfo.InvariantCulture), cancellationToken);
                case JsonToken.Float:
                    ValidationUtils.ArgumentNotNull(value, nameof(value));
                    if (value is decimal dec) {
                        return WriteValueAsync(dec, cancellationToken);
                    }

                    if (value is double doub) {
                        return WriteValueAsync(doub, cancellationToken);
                    }

                    if (value is float f) {
                        return WriteValueAsync(f, cancellationToken);
                    }

                    return WriteValueAsync(Convert.ToDouble(value, CultureInfo.InvariantCulture), cancellationToken);
                case JsonToken.String:
                    ValidationUtils.ArgumentNotNull(value, nameof(value));
                    return WriteValueAsync(value.ToString(), cancellationToken);
                case JsonToken.Boolean:
                    ValidationUtils.ArgumentNotNull(value, nameof(value));
                    return WriteValueAsync(Convert.ToBoolean(value, CultureInfo.InvariantCulture), cancellationToken);
                case JsonToken.Null:
                    return WriteNullAsync(cancellationToken);
                case JsonToken.Undefined:
                    return WriteUndefinedAsync(cancellationToken);
                case JsonToken.EndObject:
                    return WriteEndObjectAsync(cancellationToken);
                case JsonToken.EndArray:
                    return WriteEndArrayAsync(cancellationToken);
                case JsonToken.EndConstructor:
                    return WriteEndConstructorAsync(cancellationToken);
                case JsonToken.Date:
                    ValidationUtils.ArgumentNotNull(value, nameof(value));
                    if (value is DateTimeOffset offset) {
                        return WriteValueAsync(offset, cancellationToken);
                    }

                    return WriteValueAsync(Convert.ToDateTime(value, CultureInfo.InvariantCulture), cancellationToken);
                case JsonToken.Raw:
                    return WriteRawValueAsync(value?.ToString(), cancellationToken);
                case JsonToken.Bytes:
                    ValidationUtils.ArgumentNotNull(value, nameof(value));
                    if (value is Guid guid) {
                        return WriteValueAsync(guid, cancellationToken);
                    }

                    return WriteValueAsync((byte[]?)value, cancellationToken);
                default:
                    throw MiscellaneousUtils.CreateArgumentOutOfRangeException(nameof(token), token, "Unexpected token type.");
            }
        }

        internal virtual async Task WriteTokenAsync(JsonReader reader, bool writeChildren, bool writeDateConstructorAsDate, bool writeComments, CancellationToken cancellationToken)
        {
            int initialDepth = CalculateWriteTokenInitialDepth(reader);

            do {
                if (writeDateConstructorAsDate && reader.TokenType == JsonToken.StartConstructor && string.Equals(reader.Value?.ToString(), "Date", StringComparison.Ordinal)) {
                    await WriteConstructorDateAsync(reader, cancellationToken).ConfigureAwait(false);
                } else {
                    if (writeComments || reader.TokenType != JsonToken.Comment) {
                        await WriteTokenAsync(reader.TokenType, reader.Value, cancellationToken).ConfigureAwait(false);
                    }
                }
            } while (
                initialDepth - 1 < reader.Depth - (JsonTokenUtils.IsEndToken(reader.TokenType) ? 1 : 0)
                && writeChildren
                && await reader.ReadAsync(cancellationToken).ConfigureAwait(false));

            if (IsWriteTokenIncomplete(reader, writeChildren, initialDepth)) {
                throw JsonWriterException.Create(this, "Unexpected end when reading token.", null);
            }
        }
        internal async Task WriteTokenSyncReadingAsync(JsonReader reader, CancellationToken cancellationToken)
        {
            int initialDepth = CalculateWriteTokenInitialDepth(reader);

            do {
                if (reader.TokenType == JsonToken.StartConstructor && string.Equals(reader.Value?.ToString(), "Date", StringComparison.Ordinal)) {
                    WriteConstructorDate(reader);
                } else {
                    WriteToken(reader.TokenType, reader.Value);
                }
            } while (
                initialDepth - 1 < reader.Depth - (JsonTokenUtils.IsEndToken(reader.TokenType) ? 1 : 0)
                && await reader.ReadAsync(cancellationToken).ConfigureAwait(false));

            if (initialDepth < CalculateWriteTokenFinalDepth(reader)) {
                throw JsonWriterException.Create(this, "Unexpected end when reading token.", null);
            }
        }

        private async Task WriteConstructorDateAsync(JsonReader reader, CancellationToken cancellationToken)
        {
            if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false)) {
                throw JsonWriterException.Create(this, "Unexpected end when reading date constructor.", null);
            }
            if (reader.TokenType != JsonToken.Integer) {
                throw JsonWriterException.Create(this, "Unexpected token when reading date constructor. Expected Integer, got " + reader.TokenType, null);
            }

            DateTime date = DateTimeUtils.ConvertJavaScriptTicksToDateTime((long)reader.Value!);

            if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false)) {
                throw JsonWriterException.Create(this, "Unexpected end when reading date constructor.", null);
            }
            if (reader.TokenType != JsonToken.EndConstructor) {
                throw JsonWriterException.Create(this, "Unexpected token when reading date constructor. Expected EndConstructor, got " + reader.TokenType, null);
            }

            await WriteValueAsync(date, cancellationToken).ConfigureAwait(false);
        }
        public virtual Task WriteValueAsync(bool value, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested) {
                return cancellationToken.FromCanceled();
            }

            WriteValue(value);
            return AsyncUtils.CompletedTask;
        }
        public virtual Task WriteValueAsync(bool? value, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested) {
                return cancellationToken.FromCanceled();
            }

            WriteValue(value);
            return AsyncUtils.CompletedTask;
        }
        public virtual Task WriteValueAsync(byte value, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested) {
                return cancellationToken.FromCanceled();
            }

            WriteValue(value);
            return AsyncUtils.CompletedTask;
        }
        public virtual Task WriteValueAsync(byte? value, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested) {
                return cancellationToken.FromCanceled();
            }

            WriteValue(value);
            return AsyncUtils.CompletedTask;
        }
        public virtual Task WriteValueAsync(byte[]? value, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested) {
                return cancellationToken.FromCanceled();
            }

            WriteValue(value);
            return AsyncUtils.CompletedTask;
        }
        public virtual Task WriteValueAsync(char value, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested) {
                return cancellationToken.FromCanceled();
            }

            WriteValue(value);
            return AsyncUtils.CompletedTask;
        }
        public virtual Task WriteValueAsync(char? value, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested) {
                return cancellationToken.FromCanceled();
            }

            WriteValue(value);
            return AsyncUtils.CompletedTask;
        }
        public virtual Task WriteValueAsync(DateTime value, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested) {
                return cancellationToken.FromCanceled();
            }

            WriteValue(value);
            return AsyncUtils.CompletedTask;
        }
        public virtual Task WriteValueAsync(DateTime? value, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested) {
                return cancellationToken.FromCanceled();
            }

            WriteValue(value);
            return AsyncUtils.CompletedTask;
        }
        public virtual Task WriteValueAsync(DateTimeOffset value, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested) {
                return cancellationToken.FromCanceled();
            }

            WriteValue(value);
            return AsyncUtils.CompletedTask;
        }
        public virtual Task WriteValueAsync(DateTimeOffset? value, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested) {
                return cancellationToken.FromCanceled();
            }

            WriteValue(value);
            return AsyncUtils.CompletedTask;
        }
        public virtual Task WriteValueAsync(decimal value, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested) {
                return cancellationToken.FromCanceled();
            }

            WriteValue(value);
            return AsyncUtils.CompletedTask;
        }
        public virtual Task WriteValueAsync(decimal? value, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested) {
                return cancellationToken.FromCanceled();
            }

            WriteValue(value);
            return AsyncUtils.CompletedTask;
        }
        public virtual Task WriteValueAsync(double value, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested) {
                return cancellationToken.FromCanceled();
            }

            WriteValue(value);
            return AsyncUtils.CompletedTask;
        }
        public virtual Task WriteValueAsync(double? value, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested) {
                return cancellationToken.FromCanceled();
            }

            WriteValue(value);
            return AsyncUtils.CompletedTask;
        }
        public virtual Task WriteValueAsync(float value, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested) {
                return cancellationToken.FromCanceled();
            }

            WriteValue(value);
            return AsyncUtils.CompletedTask;
        }
        public virtual Task WriteValueAsync(float? value, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested) {
                return cancellationToken.FromCanceled();
            }

            WriteValue(value);
            return AsyncUtils.CompletedTask;
        }
        public virtual Task WriteValueAsync(Guid value, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested) {
                return cancellationToken.FromCanceled();
            }

            WriteValue(value);
            return AsyncUtils.CompletedTask;
        }
        public virtual Task WriteValueAsync(Guid? value, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested) {
                return cancellationToken.FromCanceled();
            }

            WriteValue(value);
            return AsyncUtils.CompletedTask;
        }
        public virtual Task WriteValueAsync(int value, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested) {
                return cancellationToken.FromCanceled();
            }

            WriteValue(value);
            return AsyncUtils.CompletedTask;
        }
        public virtual Task WriteValueAsync(int? value, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested) {
                return cancellationToken.FromCanceled();
            }

            WriteValue(value);
            return AsyncUtils.CompletedTask;
        }
        public virtual Task WriteValueAsync(long value, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested) {
                return cancellationToken.FromCanceled();
            }

            WriteValue(value);
            return AsyncUtils.CompletedTask;
        }
        public virtual Task WriteValueAsync(long? value, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested) {
                return cancellationToken.FromCanceled();
            }

            WriteValue(value);
            return AsyncUtils.CompletedTask;
        }
        public virtual Task WriteValueAsync(object? value, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested) {
                return cancellationToken.FromCanceled();
            }

            WriteValue(value);
            return AsyncUtils.CompletedTask;
        }
        [CLSCompliant(false)]
        public virtual Task WriteValueAsync(sbyte value, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested) {
                return cancellationToken.FromCanceled();
            }

            WriteValue(value);
            return AsyncUtils.CompletedTask;
        }
        [CLSCompliant(false)]
        public virtual Task WriteValueAsync(sbyte? value, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested) {
                return cancellationToken.FromCanceled();
            }

            WriteValue(value);
            return AsyncUtils.CompletedTask;
        }
        public virtual Task WriteValueAsync(short value, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested) {
                return cancellationToken.FromCanceled();
            }

            WriteValue(value);
            return AsyncUtils.CompletedTask;
        }
        public virtual Task WriteValueAsync(short? value, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested) {
                return cancellationToken.FromCanceled();
            }

            WriteValue(value);
            return AsyncUtils.CompletedTask;
        }
        public virtual Task WriteValueAsync(string? value, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested) {
                return cancellationToken.FromCanceled();
            }

            WriteValue(value);
            return AsyncUtils.CompletedTask;
        }
        public virtual Task WriteValueAsync(TimeSpan value, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested) {
                return cancellationToken.FromCanceled();
            }

            WriteValue(value);
            return AsyncUtils.CompletedTask;
        }
        public virtual Task WriteValueAsync(TimeSpan? value, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested) {
                return cancellationToken.FromCanceled();
            }

            WriteValue(value);
            return AsyncUtils.CompletedTask;
        }
        [CLSCompliant(false)]
        public virtual Task WriteValueAsync(uint value, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested) {
                return cancellationToken.FromCanceled();
            }

            WriteValue(value);
            return AsyncUtils.CompletedTask;
        }
        [CLSCompliant(false)]
        public virtual Task WriteValueAsync(uint? value, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested) {
                return cancellationToken.FromCanceled();
            }

            WriteValue(value);
            return AsyncUtils.CompletedTask;
        }
        [CLSCompliant(false)]
        public virtual Task WriteValueAsync(ulong value, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested) {
                return cancellationToken.FromCanceled();
            }

            WriteValue(value);
            return AsyncUtils.CompletedTask;
        }
        [CLSCompliant(false)]
        public virtual Task WriteValueAsync(ulong? value, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested) {
                return cancellationToken.FromCanceled();
            }

            WriteValue(value);
            return AsyncUtils.CompletedTask;
        }
        public virtual Task WriteValueAsync(Uri? value, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested) {
                return cancellationToken.FromCanceled();
            }

            WriteValue(value);
            return AsyncUtils.CompletedTask;
        }
        [CLSCompliant(false)]
        public virtual Task WriteValueAsync(ushort value, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested) {
                return cancellationToken.FromCanceled();
            }

            WriteValue(value);
            return AsyncUtils.CompletedTask;
        }
        [CLSCompliant(false)]
        public virtual Task WriteValueAsync(ushort? value, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested) {
                return cancellationToken.FromCanceled();
            }

            WriteValue(value);
            return AsyncUtils.CompletedTask;
        }
        public virtual Task WriteUndefinedAsync(CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested) {
                return cancellationToken.FromCanceled();
            }

            WriteUndefined();
            return AsyncUtils.CompletedTask;
        }
        public virtual Task WriteWhitespaceAsync(string ws, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested) {
                return cancellationToken.FromCanceled();
            }

            WriteWhitespace(ws);
            return AsyncUtils.CompletedTask;
        }

        internal Task InternalWriteValueAsync(JsonToken token, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) {
                return cancellationToken.FromCanceled();
            }

            UpdateScopeWithFinishedValue();
            return AutoCompleteAsync(token, cancellationToken);
        }
        protected Task SetWriteStateAsync(JsonToken token, object value, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) {
                return cancellationToken.FromCanceled();
            }

            switch (token) {
                case JsonToken.StartObject:
                    return InternalWriteStartAsync(token, JsonContainerType.Object, cancellationToken);
                case JsonToken.StartArray:
                    return InternalWriteStartAsync(token, JsonContainerType.Array, cancellationToken);
                case JsonToken.StartConstructor:
                    return InternalWriteStartAsync(token, JsonContainerType.Constructor, cancellationToken);
                case JsonToken.PropertyName:
                    if (!(value is string s)) {
                        throw new ArgumentException("A name is required when setting property name state.", nameof(value));
                    }

                    return InternalWritePropertyNameAsync(s, cancellationToken);
                case JsonToken.Comment:
                    return InternalWriteCommentAsync(cancellationToken);
                case JsonToken.Raw:
                    return AsyncUtils.CompletedTask;
                case JsonToken.Integer:
                case JsonToken.Float:
                case JsonToken.String:
                case JsonToken.Boolean:
                case JsonToken.Date:
                case JsonToken.Bytes:
                case JsonToken.Null:
                case JsonToken.Undefined:
                    return InternalWriteValueAsync(token, cancellationToken);
                case JsonToken.EndObject:
                    return InternalWriteEndAsync(JsonContainerType.Object, cancellationToken);
                case JsonToken.EndArray:
                    return InternalWriteEndAsync(JsonContainerType.Array, cancellationToken);
                case JsonToken.EndConstructor:
                    return InternalWriteEndAsync(JsonContainerType.Constructor, cancellationToken);
                default:
                    throw new ArgumentOutOfRangeException(nameof(token));
            }
        }

        internal static Task WriteValueAsync(JsonWriter writer, PrimitiveTypeCode typeCode, object value, CancellationToken cancellationToken)
        {
            while (true) {
                switch (typeCode) {
                    case PrimitiveTypeCode.Char:
                        return writer.WriteValueAsync((char)value, cancellationToken);
                    case PrimitiveTypeCode.CharNullable:
                        return writer.WriteValueAsync(value == null ? (char?)null : (char)value, cancellationToken);
                    case PrimitiveTypeCode.Boolean:
                        return writer.WriteValueAsync((bool)value, cancellationToken);
                    case PrimitiveTypeCode.BooleanNullable:
                        return writer.WriteValueAsync(value == null ? (bool?)null : (bool)value, cancellationToken);
                    case PrimitiveTypeCode.SByte:
                        return writer.WriteValueAsync((sbyte)value, cancellationToken);
                    case PrimitiveTypeCode.SByteNullable:
                        return writer.WriteValueAsync(value == null ? (sbyte?)null : (sbyte)value, cancellationToken);
                    case PrimitiveTypeCode.Int16:
                        return writer.WriteValueAsync((short)value, cancellationToken);
                    case PrimitiveTypeCode.Int16Nullable:
                        return writer.WriteValueAsync(value == null ? (short?)null : (short)value, cancellationToken);
                    case PrimitiveTypeCode.UInt16:
                        return writer.WriteValueAsync((ushort)value, cancellationToken);
                    case PrimitiveTypeCode.UInt16Nullable:
                        return writer.WriteValueAsync(value == null ? (ushort?)null : (ushort)value, cancellationToken);
                    case PrimitiveTypeCode.Int32:
                        return writer.WriteValueAsync((int)value, cancellationToken);
                    case PrimitiveTypeCode.Int32Nullable:
                        return writer.WriteValueAsync(value == null ? (int?)null : (int)value, cancellationToken);
                    case PrimitiveTypeCode.Byte:
                        return writer.WriteValueAsync((byte)value, cancellationToken);
                    case PrimitiveTypeCode.ByteNullable:
                        return writer.WriteValueAsync(value == null ? (byte?)null : (byte)value, cancellationToken);
                    case PrimitiveTypeCode.UInt32:
                        return writer.WriteValueAsync((uint)value, cancellationToken);
                    case PrimitiveTypeCode.UInt32Nullable:
                        return writer.WriteValueAsync(value == null ? (uint?)null : (uint)value, cancellationToken);
                    case PrimitiveTypeCode.Int64:
                        return writer.WriteValueAsync((long)value, cancellationToken);
                    case PrimitiveTypeCode.Int64Nullable:
                        return writer.WriteValueAsync(value == null ? (long?)null : (long)value, cancellationToken);
                    case PrimitiveTypeCode.UInt64:
                        return writer.WriteValueAsync((ulong)value, cancellationToken);
                    case PrimitiveTypeCode.UInt64Nullable:
                        return writer.WriteValueAsync(value == null ? (ulong?)null : (ulong)value, cancellationToken);
                    case PrimitiveTypeCode.Single:
                        return writer.WriteValueAsync((float)value, cancellationToken);
                    case PrimitiveTypeCode.SingleNullable:
                        return writer.WriteValueAsync(value == null ? (float?)null : (float)value, cancellationToken);
                    case PrimitiveTypeCode.Double:
                        return writer.WriteValueAsync((double)value, cancellationToken);
                    case PrimitiveTypeCode.DoubleNullable:
                        return writer.WriteValueAsync(value == null ? (double?)null : (double)value, cancellationToken);
                    case PrimitiveTypeCode.DateTime:
                        return writer.WriteValueAsync((DateTime)value, cancellationToken);
                    case PrimitiveTypeCode.DateTimeNullable:
                        return writer.WriteValueAsync(value == null ? (DateTime?)null : (DateTime)value, cancellationToken);
                    case PrimitiveTypeCode.DateTimeOffset:
                        return writer.WriteValueAsync((DateTimeOffset)value, cancellationToken);
                    case PrimitiveTypeCode.DateTimeOffsetNullable:
                        return writer.WriteValueAsync(value == null ? (DateTimeOffset?)null : (DateTimeOffset)value, cancellationToken);
                    case PrimitiveTypeCode.Decimal:
                        return writer.WriteValueAsync((decimal)value, cancellationToken);
                    case PrimitiveTypeCode.DecimalNullable:
                        return writer.WriteValueAsync(value == null ? (decimal?)null : (decimal)value, cancellationToken);
                    case PrimitiveTypeCode.Guid:
                        return writer.WriteValueAsync((Guid)value, cancellationToken);
                    case PrimitiveTypeCode.GuidNullable:
                        return writer.WriteValueAsync(value == null ? (Guid?)null : (Guid)value, cancellationToken);
                    case PrimitiveTypeCode.TimeSpan:
                        return writer.WriteValueAsync((TimeSpan)value, cancellationToken);
                    case PrimitiveTypeCode.TimeSpanNullable:
                        return writer.WriteValueAsync(value == null ? (TimeSpan?)null : (TimeSpan)value, cancellationToken);
#if HAVE_BIG_INTEGER
                    case PrimitiveTypeCode.BigInteger:
                        return writer.WriteValueAsync((BigInteger)value, cancellationToken);
                    case PrimitiveTypeCode.BigIntegerNullable:
                        return writer.WriteValueAsync(value == null ? (BigInteger?)null : (BigInteger)value, cancellationToken);
#endif
                    case PrimitiveTypeCode.Uri:
                        return writer.WriteValueAsync((Uri)value, cancellationToken);
                    case PrimitiveTypeCode.String:
                        return writer.WriteValueAsync((string)value, cancellationToken);
                    case PrimitiveTypeCode.Bytes:
                        return writer.WriteValueAsync((byte[])value, cancellationToken);
#if HAVE_DB_NULL_TYPE_CODE
                    case PrimitiveTypeCode.DBNull:
                        return writer.WriteNullAsync(cancellationToken);
#endif
                    default:
#if HAVE_ICONVERTIBLE
                        if (value is IConvertible convertible)
                        {
                            ResolveConvertibleValue(convertible, out typeCode, out value);
                            continue;
                        }
#endif
                        if (value == null) {
                            return writer.WriteNullAsync(cancellationToken);
                        }

                        throw CreateUnsupportedTypeException(writer, value);
                }
            }
        }
    }
}

#endif
