
using System;
using System.Collections.Generic;
#if HAVE_BIG_INTEGER
using System.Numerics;
#endif
using Simula.Scripting.Json.Utilities;
using System.Globalization;
#if !HAVE_LINQ
using Simula.Scripting.Json.Utilities.LinqBridge;
#else
using System.Linq;

#endif

namespace Simula.Scripting.Json
{
    public abstract partial class JsonWriter : IDisposable
    {
        internal enum State
        {
            Start = 0,
            Property = 1,
            ObjectStart = 2,
            Object = 3,
            ArrayStart = 4,
            Array = 5,
            ConstructorStart = 6,
            Constructor = 7,
            Closed = 8,
            Error = 9
        }
        private static readonly State[][] StateArray;

        internal static readonly State[][] StateArrayTempate = new[]
        {
            /* None                        */new[] { State.Error,            State.Error,            State.Error,        State.Error,      State.Error,            State.Error,            State.Error,            State.Error,            State.Error, State.Error },
            /* StartObject                 */new[] { State.ObjectStart,      State.ObjectStart,      State.Error,        State.Error,      State.ObjectStart,      State.ObjectStart,      State.ObjectStart,      State.ObjectStart,      State.Error, State.Error },
            /* StartArray                  */new[] { State.ArrayStart,       State.ArrayStart,       State.Error,        State.Error,      State.ArrayStart,       State.ArrayStart,       State.ArrayStart,       State.ArrayStart,       State.Error, State.Error },
            /* StartConstructor            */new[] { State.ConstructorStart, State.ConstructorStart, State.Error,        State.Error,      State.ConstructorStart, State.ConstructorStart, State.ConstructorStart, State.ConstructorStart, State.Error, State.Error },
            /* Property                    */new[] { State.Property,         State.Error,            State.Property,     State.Property,   State.Error,            State.Error,            State.Error,            State.Error,            State.Error, State.Error },
            /* Comment                     */new[] { State.Start,            State.Property,         State.ObjectStart,  State.Object,     State.ArrayStart,       State.Array,            State.Constructor,      State.Constructor,      State.Error, State.Error },
            /* Raw                         */new[] { State.Start,            State.Property,         State.ObjectStart,  State.Object,     State.ArrayStart,       State.Array,            State.Constructor,      State.Constructor,      State.Error, State.Error },
            /* Value (this will be copied) */new[] { State.Start,            State.Object,           State.Error,        State.Error,      State.Array,            State.Array,            State.Constructor,      State.Constructor,      State.Error, State.Error }
        };

        internal static State[][] BuildStateArray()
        {
            List<State[]> allStates = StateArrayTempate.ToList();
            State[] errorStates = StateArrayTempate[0];
            State[] valueStates = StateArrayTempate[7];

            EnumInfo enumValuesAndNames = EnumUtils.GetEnumValuesAndNames(typeof(JsonToken));

            foreach (ulong valueToken in enumValuesAndNames.Values) {
                if (allStates.Count <= (int)valueToken) {
                    JsonToken token = (JsonToken)valueToken;
                    switch (token) {
                        case JsonToken.Integer:
                        case JsonToken.Float:
                        case JsonToken.String:
                        case JsonToken.Boolean:
                        case JsonToken.Null:
                        case JsonToken.Undefined:
                        case JsonToken.Date:
                        case JsonToken.Bytes:
                            allStates.Add(valueStates);
                            break;
                        default:
                            allStates.Add(errorStates);
                            break;
                    }
                }
            }

            return allStates.ToArray();
        }

        static JsonWriter()
        {
            StateArray = BuildStateArray();
        }

        private List<JsonPosition>? _stack;
        private JsonPosition _currentPosition;
        private State _currentState;
        private Formatting _formatting;
        public bool CloseOutput { get; set; }
        public bool AutoCompleteOnClose { get; set; }
        protected internal int Top {
            get {
                int depth = _stack?.Count ?? 0;
                if (Peek() != JsonContainerType.None) {
                    depth++;
                }

                return depth;
            }
        }
        public WriteState WriteState {
            get {
                switch (_currentState) {
                    case State.Error:
                        return WriteState.Error;
                    case State.Closed:
                        return WriteState.Closed;
                    case State.Object:
                    case State.ObjectStart:
                        return WriteState.Object;
                    case State.Array:
                    case State.ArrayStart:
                        return WriteState.Array;
                    case State.Constructor:
                    case State.ConstructorStart:
                        return WriteState.Constructor;
                    case State.Property:
                        return WriteState.Property;
                    case State.Start:
                        return WriteState.Start;
                    default:
                        throw JsonWriterException.Create(this, "Invalid state: " + _currentState, null);
                }
            }
        }

        internal string ContainerPath {
            get {
                if (_currentPosition.Type == JsonContainerType.None || _stack == null) {
                    return string.Empty;
                }

                return JsonPosition.BuildPath(_stack, null);
            }
        }
        public string Path {
            get {
                if (_currentPosition.Type == JsonContainerType.None) {
                    return string.Empty;
                }

                bool insideContainer = (_currentState != State.ArrayStart
                                        && _currentState != State.ConstructorStart
                                        && _currentState != State.ObjectStart);

                JsonPosition? current = insideContainer ? (JsonPosition?)_currentPosition : null;

                return JsonPosition.BuildPath(_stack!, current);
            }
        }

        private DateFormatHandling _dateFormatHandling;
        private DateTimeZoneHandling _dateTimeZoneHandling;
        private StringEscapeHandling _stringEscapeHandling;
        private FloatFormatHandling _floatFormatHandling;
        private string? _dateFormatString;
        private CultureInfo? _culture;
        public Formatting Formatting {
            get => _formatting;
            set {
                if (value < Formatting.None || value > Formatting.Indented) {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _formatting = value;
            }
        }
        public DateFormatHandling DateFormatHandling {
            get => _dateFormatHandling;
            set {
                if (value < DateFormatHandling.IsoDateFormat || value > DateFormatHandling.MicrosoftDateFormat) {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _dateFormatHandling = value;
            }
        }
        public DateTimeZoneHandling DateTimeZoneHandling {
            get => _dateTimeZoneHandling;
            set {
                if (value < DateTimeZoneHandling.Local || value > DateTimeZoneHandling.RoundtripKind) {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _dateTimeZoneHandling = value;
            }
        }
        public StringEscapeHandling StringEscapeHandling {
            get => _stringEscapeHandling;
            set {
                if (value < StringEscapeHandling.Default || value > StringEscapeHandling.EscapeHtml) {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _stringEscapeHandling = value;
                OnStringEscapeHandlingChanged();
            }
        }

        internal virtual void OnStringEscapeHandlingChanged()
        {
        }
        public FloatFormatHandling FloatFormatHandling {
            get => _floatFormatHandling;
            set {
                if (value < FloatFormatHandling.String || value > FloatFormatHandling.DefaultValue) {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _floatFormatHandling = value;
            }
        }
        public string? DateFormatString {
            get => _dateFormatString;
            set => _dateFormatString = value;
        }
        public CultureInfo Culture {
            get => _culture ?? CultureInfo.InvariantCulture;
            set => _culture = value;
        }
        protected JsonWriter()
        {
            _currentState = State.Start;
            _formatting = Formatting.None;
            _dateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind;

            CloseOutput = true;
            AutoCompleteOnClose = true;
        }

        internal void UpdateScopeWithFinishedValue()
        {
            if (_currentPosition.HasIndex) {
                _currentPosition.Position++;
            }
        }

        private void Push(JsonContainerType value)
        {
            if (_currentPosition.Type != JsonContainerType.None) {
                if (_stack == null) {
                    _stack = new List<JsonPosition>();
                }

                _stack.Add(_currentPosition);
            }

            _currentPosition = new JsonPosition(value);
        }

        private JsonContainerType Pop()
        {
            JsonPosition oldPosition = _currentPosition;

            if (_stack != null && _stack.Count > 0) {
                _currentPosition = _stack[_stack.Count - 1];
                _stack.RemoveAt(_stack.Count - 1);
            } else {
                _currentPosition = new JsonPosition();
            }

            return oldPosition.Type;
        }

        private JsonContainerType Peek()
        {
            return _currentPosition.Type;
        }
        public abstract void Flush();
        public virtual void Close()
        {
            if (AutoCompleteOnClose) {
                AutoCompleteAll();
            }
        }
        public virtual void WriteStartObject()
        {
            InternalWriteStart(JsonToken.StartObject, JsonContainerType.Object);
        }
        public virtual void WriteEndObject()
        {
            InternalWriteEnd(JsonContainerType.Object);
        }
        public virtual void WriteStartArray()
        {
            InternalWriteStart(JsonToken.StartArray, JsonContainerType.Array);
        }
        public virtual void WriteEndArray()
        {
            InternalWriteEnd(JsonContainerType.Array);
        }
        public virtual void WriteStartConstructor(string name)
        {
            InternalWriteStart(JsonToken.StartConstructor, JsonContainerType.Constructor);
        }
        public virtual void WriteEndConstructor()
        {
            InternalWriteEnd(JsonContainerType.Constructor);
        }
        public virtual void WritePropertyName(string name)
        {
            InternalWritePropertyName(name);
        }
        public virtual void WritePropertyName(string name, bool escape)
        {
            WritePropertyName(name);
        }
        public virtual void WriteEnd()
        {
            WriteEnd(Peek());
        }
        public void WriteToken(JsonReader reader)
        {
            WriteToken(reader, true);
        }
        public void WriteToken(JsonReader reader, bool writeChildren)
        {
            ValidationUtils.ArgumentNotNull(reader, nameof(reader));

            WriteToken(reader, writeChildren, true, true);
        }
        public void WriteToken(JsonToken token, object? value)
        {
            switch (token) {
                case JsonToken.None:
                    break;
                case JsonToken.StartObject:
                    WriteStartObject();
                    break;
                case JsonToken.StartArray:
                    WriteStartArray();
                    break;
                case JsonToken.StartConstructor:
                    ValidationUtils.ArgumentNotNull(value, nameof(value));
                    WriteStartConstructor(value.ToString());
                    break;
                case JsonToken.PropertyName:
                    ValidationUtils.ArgumentNotNull(value, nameof(value));
                    WritePropertyName(value.ToString());
                    break;
                case JsonToken.Comment:
                    WriteComment(value?.ToString());
                    break;
                case JsonToken.Integer:
                    ValidationUtils.ArgumentNotNull(value, nameof(value));
#if HAVE_BIG_INTEGER
                    if (value is BigInteger integer)
                    {
                        WriteValue(integer);
                    }
                    else
#endif
                    {
                        WriteValue(Convert.ToInt64(value, CultureInfo.InvariantCulture));
                    }
                    break;
                case JsonToken.Float:
                    ValidationUtils.ArgumentNotNull(value, nameof(value));
                    if (value is decimal decimalValue) {
                        WriteValue(decimalValue);
                    } else if (value is double doubleValue) {
                        WriteValue(doubleValue);
                    } else if (value is float floatValue) {
                        WriteValue(floatValue);
                    } else {
                        WriteValue(Convert.ToDouble(value, CultureInfo.InvariantCulture));
                    }
                    break;
                case JsonToken.String:
                    ValidationUtils.ArgumentNotNull(value, nameof(value));
                    WriteValue(value.ToString());
                    break;
                case JsonToken.Boolean:
                    ValidationUtils.ArgumentNotNull(value, nameof(value));
                    WriteValue(Convert.ToBoolean(value, CultureInfo.InvariantCulture));
                    break;
                case JsonToken.Null:
                    WriteNull();
                    break;
                case JsonToken.Undefined:
                    WriteUndefined();
                    break;
                case JsonToken.EndObject:
                    WriteEndObject();
                    break;
                case JsonToken.EndArray:
                    WriteEndArray();
                    break;
                case JsonToken.EndConstructor:
                    WriteEndConstructor();
                    break;
                case JsonToken.Date:
                    ValidationUtils.ArgumentNotNull(value, nameof(value));
#if HAVE_DATE_TIME_OFFSET
                    if (value is DateTimeOffset dt) {
                        WriteValue(dt);
                    } else
#endif
                    {
                        WriteValue(Convert.ToDateTime(value, CultureInfo.InvariantCulture));
                    }
                    break;
                case JsonToken.Raw:
                    WriteRawValue(value?.ToString());
                    break;
                case JsonToken.Bytes:
                    ValidationUtils.ArgumentNotNull(value, nameof(value));
                    if (value is Guid guid) {
                        WriteValue(guid);
                    } else {
                        WriteValue((byte[])value!);
                    }
                    break;
                default:
                    throw MiscellaneousUtils.CreateArgumentOutOfRangeException(nameof(token), token, "Unexpected token type.");
            }
        }
        public void WriteToken(JsonToken token)
        {
            WriteToken(token, null);
        }

        internal virtual void WriteToken(JsonReader reader, bool writeChildren, bool writeDateConstructorAsDate, bool writeComments)
        {
            int initialDepth = CalculateWriteTokenInitialDepth(reader);

            do {
                if (writeDateConstructorAsDate && reader.TokenType == JsonToken.StartConstructor && string.Equals(reader.Value?.ToString(), "Date", StringComparison.Ordinal)) {
                    WriteConstructorDate(reader);
                } else {
                    if (writeComments || reader.TokenType != JsonToken.Comment) {
                        WriteToken(reader.TokenType, reader.Value);
                    }
                }
            } while (
                initialDepth - 1 < reader.Depth - (JsonTokenUtils.IsEndToken(reader.TokenType) ? 1 : 0)
                && writeChildren
                && reader.Read());

            if (IsWriteTokenIncomplete(reader, writeChildren, initialDepth)) {
                throw JsonWriterException.Create(this, "Unexpected end when reading token.", null);
            }
        }

        private bool IsWriteTokenIncomplete(JsonReader reader, bool writeChildren, int initialDepth)
        {
            int finalDepth = CalculateWriteTokenFinalDepth(reader);
            return initialDepth < finalDepth ||
                (writeChildren && initialDepth == finalDepth && JsonTokenUtils.IsStartToken(reader.TokenType));
        }

        private int CalculateWriteTokenInitialDepth(JsonReader reader)
        {
            JsonToken type = reader.TokenType;
            if (type == JsonToken.None) {
                return -1;
            }

            return JsonTokenUtils.IsStartToken(type) ? reader.Depth : reader.Depth + 1;
        }

        private int CalculateWriteTokenFinalDepth(JsonReader reader)
        {
            JsonToken type = reader.TokenType;
            if (type == JsonToken.None) {
                return -1;
            }

            return JsonTokenUtils.IsEndToken(type) ? reader.Depth - 1 : reader.Depth;
        }

        private void WriteConstructorDate(JsonReader reader)
        {
            if (!JavaScriptUtils.TryGetDateFromConstructorJson(reader, out DateTime dateTime, out string? errorMessage)) {
                throw JsonWriterException.Create(this, errorMessage, null);
            }

            WriteValue(dateTime);
        }

        private void WriteEnd(JsonContainerType type)
        {
            switch (type) {
                case JsonContainerType.Object:
                    WriteEndObject();
                    break;
                case JsonContainerType.Array:
                    WriteEndArray();
                    break;
                case JsonContainerType.Constructor:
                    WriteEndConstructor();
                    break;
                default:
                    throw JsonWriterException.Create(this, "Unexpected type when writing end: " + type, null);
            }
        }

        private void AutoCompleteAll()
        {
            while (Top > 0) {
                WriteEnd();
            }
        }

        private JsonToken GetCloseTokenForType(JsonContainerType type)
        {
            switch (type) {
                case JsonContainerType.Object:
                    return JsonToken.EndObject;
                case JsonContainerType.Array:
                    return JsonToken.EndArray;
                case JsonContainerType.Constructor:
                    return JsonToken.EndConstructor;
                default:
                    throw JsonWriterException.Create(this, "No close token for type: " + type, null);
            }
        }

        private void AutoCompleteClose(JsonContainerType type)
        {
            int levelsToComplete = CalculateLevelsToComplete(type);

            for (int i = 0; i < levelsToComplete; i++) {
                JsonToken token = GetCloseTokenForType(Pop());

                if (_currentState == State.Property) {
                    WriteNull();
                }

                if (_formatting == Formatting.Indented) {
                    if (_currentState != State.ObjectStart && _currentState != State.ArrayStart) {
                        WriteIndent();
                    }
                }

                WriteEnd(token);

                UpdateCurrentState();
            }
        }

        private int CalculateLevelsToComplete(JsonContainerType type)
        {
            int levelsToComplete = 0;

            if (_currentPosition.Type == type) {
                levelsToComplete = 1;
            } else {
                int top = Top - 2;
                for (int i = top; i >= 0; i--) {
                    int currentLevel = top - i;

                    if (_stack![currentLevel].Type == type) {
                        levelsToComplete = i + 2;
                        break;
                    }
                }
            }

            if (levelsToComplete == 0) {
                throw JsonWriterException.Create(this, "No token to close.", null);
            }

            return levelsToComplete;
        }

        private void UpdateCurrentState()
        {
            JsonContainerType currentLevelType = Peek();

            switch (currentLevelType) {
                case JsonContainerType.Object:
                    _currentState = State.Object;
                    break;
                case JsonContainerType.Array:
                    _currentState = State.Array;
                    break;
                case JsonContainerType.Constructor:
                    _currentState = State.Array;
                    break;
                case JsonContainerType.None:
                    _currentState = State.Start;
                    break;
                default:
                    throw JsonWriterException.Create(this, "Unknown JsonType: " + currentLevelType, null);
            }
        }
        protected virtual void WriteEnd(JsonToken token)
        {
        }
        protected virtual void WriteIndent()
        {
        }
        protected virtual void WriteValueDelimiter()
        {
        }
        protected virtual void WriteIndentSpace()
        {
        }

        internal void AutoComplete(JsonToken tokenBeingWritten)
        {
            State newState = StateArray[(int)tokenBeingWritten][(int)_currentState];

            if (newState == State.Error) {
                throw JsonWriterException.Create(this, "Token {0} in state {1} would result in an invalid JSON object.".FormatWith(CultureInfo.InvariantCulture, tokenBeingWritten.ToString(), _currentState.ToString()), null);
            }

            if ((_currentState == State.Object || _currentState == State.Array || _currentState == State.Constructor) && tokenBeingWritten != JsonToken.Comment) {
                WriteValueDelimiter();
            }

            if (_formatting == Formatting.Indented) {
                if (_currentState == State.Property) {
                    WriteIndentSpace();
                }
                if ((_currentState == State.Array || _currentState == State.ArrayStart || _currentState == State.Constructor || _currentState == State.ConstructorStart)
                    || (tokenBeingWritten == JsonToken.PropertyName && _currentState != State.Start)) {
                    WriteIndent();
                }
            }

            _currentState = newState;
        }

        #region WriteValue methods
        public virtual void WriteNull()
        {
            InternalWriteValue(JsonToken.Null);
        }
        public virtual void WriteUndefined()
        {
            InternalWriteValue(JsonToken.Undefined);
        }
        public virtual void WriteRaw(string? json)
        {
            InternalWriteRaw();
        }
        public virtual void WriteRawValue(string? json)
        {
            UpdateScopeWithFinishedValue();
            AutoComplete(JsonToken.Undefined);
            WriteRaw(json);
        }
        public virtual void WriteValue(string? value)
        {
            InternalWriteValue(JsonToken.String);
        }
        public virtual void WriteValue(int value)
        {
            InternalWriteValue(JsonToken.Integer);
        }
        [CLSCompliant(false)]
        public virtual void WriteValue(uint value)
        {
            InternalWriteValue(JsonToken.Integer);
        }
        public virtual void WriteValue(long value)
        {
            InternalWriteValue(JsonToken.Integer);
        }
        [CLSCompliant(false)]
        public virtual void WriteValue(ulong value)
        {
            InternalWriteValue(JsonToken.Integer);
        }
        public virtual void WriteValue(float value)
        {
            InternalWriteValue(JsonToken.Float);
        }
        public virtual void WriteValue(double value)
        {
            InternalWriteValue(JsonToken.Float);
        }
        public virtual void WriteValue(bool value)
        {
            InternalWriteValue(JsonToken.Boolean);
        }
        public virtual void WriteValue(short value)
        {
            InternalWriteValue(JsonToken.Integer);
        }
        [CLSCompliant(false)]
        public virtual void WriteValue(ushort value)
        {
            InternalWriteValue(JsonToken.Integer);
        }
        public virtual void WriteValue(char value)
        {
            InternalWriteValue(JsonToken.String);
        }
        public virtual void WriteValue(byte value)
        {
            InternalWriteValue(JsonToken.Integer);
        }
        [CLSCompliant(false)]
        public virtual void WriteValue(sbyte value)
        {
            InternalWriteValue(JsonToken.Integer);
        }
        public virtual void WriteValue(decimal value)
        {
            InternalWriteValue(JsonToken.Float);
        }
        public virtual void WriteValue(DateTime value)
        {
            InternalWriteValue(JsonToken.Date);
        }

#if HAVE_DATE_TIME_OFFSET
        public virtual void WriteValue(DateTimeOffset value)
        {
            InternalWriteValue(JsonToken.Date);
        }
#endif
        public virtual void WriteValue(Guid value)
        {
            InternalWriteValue(JsonToken.String);
        }
        public virtual void WriteValue(TimeSpan value)
        {
            InternalWriteValue(JsonToken.String);
        }
        public virtual void WriteValue(int? value)
        {
            if (value == null) {
                WriteNull();
            } else {
                WriteValue(value.GetValueOrDefault());
            }
        }
        [CLSCompliant(false)]
        public virtual void WriteValue(uint? value)
        {
            if (value == null) {
                WriteNull();
            } else {
                WriteValue(value.GetValueOrDefault());
            }
        }
        public virtual void WriteValue(long? value)
        {
            if (value == null) {
                WriteNull();
            } else {
                WriteValue(value.GetValueOrDefault());
            }
        }
        [CLSCompliant(false)]
        public virtual void WriteValue(ulong? value)
        {
            if (value == null) {
                WriteNull();
            } else {
                WriteValue(value.GetValueOrDefault());
            }
        }
        public virtual void WriteValue(float? value)
        {
            if (value == null) {
                WriteNull();
            } else {
                WriteValue(value.GetValueOrDefault());
            }
        }
        public virtual void WriteValue(double? value)
        {
            if (value == null) {
                WriteNull();
            } else {
                WriteValue(value.GetValueOrDefault());
            }
        }
        public virtual void WriteValue(bool? value)
        {
            if (value == null) {
                WriteNull();
            } else {
                WriteValue(value.GetValueOrDefault());
            }
        }
        public virtual void WriteValue(short? value)
        {
            if (value == null) {
                WriteNull();
            } else {
                WriteValue(value.GetValueOrDefault());
            }
        }
        [CLSCompliant(false)]
        public virtual void WriteValue(ushort? value)
        {
            if (value == null) {
                WriteNull();
            } else {
                WriteValue(value.GetValueOrDefault());
            }
        }
        public virtual void WriteValue(char? value)
        {
            if (value == null) {
                WriteNull();
            } else {
                WriteValue(value.GetValueOrDefault());
            }
        }
        public virtual void WriteValue(byte? value)
        {
            if (value == null) {
                WriteNull();
            } else {
                WriteValue(value.GetValueOrDefault());
            }
        }
        [CLSCompliant(false)]
        public virtual void WriteValue(sbyte? value)
        {
            if (value == null) {
                WriteNull();
            } else {
                WriteValue(value.GetValueOrDefault());
            }
        }
        public virtual void WriteValue(decimal? value)
        {
            if (value == null) {
                WriteNull();
            } else {
                WriteValue(value.GetValueOrDefault());
            }
        }
        public virtual void WriteValue(DateTime? value)
        {
            if (value == null) {
                WriteNull();
            } else {
                WriteValue(value.GetValueOrDefault());
            }
        }

#if HAVE_DATE_TIME_OFFSET
        public virtual void WriteValue(DateTimeOffset? value)
        {
            if (value == null) {
                WriteNull();
            } else {
                WriteValue(value.GetValueOrDefault());
            }
        }
#endif
        public virtual void WriteValue(Guid? value)
        {
            if (value == null) {
                WriteNull();
            } else {
                WriteValue(value.GetValueOrDefault());
            }
        }
        public virtual void WriteValue(TimeSpan? value)
        {
            if (value == null) {
                WriteNull();
            } else {
                WriteValue(value.GetValueOrDefault());
            }
        }
        public virtual void WriteValue(byte[]? value)
        {
            if (value == null) {
                WriteNull();
            } else {
                InternalWriteValue(JsonToken.Bytes);
            }
        }
        public virtual void WriteValue(Uri? value)
        {
            if (value == null) {
                WriteNull();
            } else {
                InternalWriteValue(JsonToken.String);
            }
        }
        public virtual void WriteValue(object? value)
        {
            if (value == null) {
                WriteNull();
            } else {
#if HAVE_BIG_INTEGER
                if (value is BigInteger)
                {
                    throw CreateUnsupportedTypeException(this, value);
                }
#endif

                WriteValue(this, ConvertUtils.GetTypeCode(value.GetType()), value);
            }
        }
        #endregion
        public virtual void WriteComment(string? text)
        {
            InternalWriteComment();
        }
        public virtual void WriteWhitespace(string ws)
        {
            InternalWriteWhitespace(ws);
        }

        void IDisposable.Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (_currentState != State.Closed && disposing) {
                Close();
            }
        }

        internal static void WriteValue(JsonWriter writer, PrimitiveTypeCode typeCode, object value)
        {
            while (true) {
                switch (typeCode) {
                    case PrimitiveTypeCode.Char:
                        writer.WriteValue((char)value);
                        return;

                    case PrimitiveTypeCode.CharNullable:
                        writer.WriteValue((value == null) ? (char?)null : (char)value);
                        return;

                    case PrimitiveTypeCode.Boolean:
                        writer.WriteValue((bool)value);
                        return;

                    case PrimitiveTypeCode.BooleanNullable:
                        writer.WriteValue((value == null) ? (bool?)null : (bool)value);
                        return;

                    case PrimitiveTypeCode.SByte:
                        writer.WriteValue((sbyte)value);
                        return;

                    case PrimitiveTypeCode.SByteNullable:
                        writer.WriteValue((value == null) ? (sbyte?)null : (sbyte)value);
                        return;

                    case PrimitiveTypeCode.Int16:
                        writer.WriteValue((short)value);
                        return;

                    case PrimitiveTypeCode.Int16Nullable:
                        writer.WriteValue((value == null) ? (short?)null : (short)value);
                        return;

                    case PrimitiveTypeCode.UInt16:
                        writer.WriteValue((ushort)value);
                        return;

                    case PrimitiveTypeCode.UInt16Nullable:
                        writer.WriteValue((value == null) ? (ushort?)null : (ushort)value);
                        return;

                    case PrimitiveTypeCode.Int32:
                        writer.WriteValue((int)value);
                        return;

                    case PrimitiveTypeCode.Int32Nullable:
                        writer.WriteValue((value == null) ? (int?)null : (int)value);
                        return;

                    case PrimitiveTypeCode.Byte:
                        writer.WriteValue((byte)value);
                        return;

                    case PrimitiveTypeCode.ByteNullable:
                        writer.WriteValue((value == null) ? (byte?)null : (byte)value);
                        return;

                    case PrimitiveTypeCode.UInt32:
                        writer.WriteValue((uint)value);
                        return;

                    case PrimitiveTypeCode.UInt32Nullable:
                        writer.WriteValue((value == null) ? (uint?)null : (uint)value);
                        return;

                    case PrimitiveTypeCode.Int64:
                        writer.WriteValue((long)value);
                        return;

                    case PrimitiveTypeCode.Int64Nullable:
                        writer.WriteValue((value == null) ? (long?)null : (long)value);
                        return;

                    case PrimitiveTypeCode.UInt64:
                        writer.WriteValue((ulong)value);
                        return;

                    case PrimitiveTypeCode.UInt64Nullable:
                        writer.WriteValue((value == null) ? (ulong?)null : (ulong)value);
                        return;

                    case PrimitiveTypeCode.Single:
                        writer.WriteValue((float)value);
                        return;

                    case PrimitiveTypeCode.SingleNullable:
                        writer.WriteValue((value == null) ? (float?)null : (float)value);
                        return;

                    case PrimitiveTypeCode.Double:
                        writer.WriteValue((double)value);
                        return;

                    case PrimitiveTypeCode.DoubleNullable:
                        writer.WriteValue((value == null) ? (double?)null : (double)value);
                        return;

                    case PrimitiveTypeCode.DateTime:
                        writer.WriteValue((DateTime)value);
                        return;

                    case PrimitiveTypeCode.DateTimeNullable:
                        writer.WriteValue((value == null) ? (DateTime?)null : (DateTime)value);
                        return;

#if HAVE_DATE_TIME_OFFSET
                    case PrimitiveTypeCode.DateTimeOffset:
                        writer.WriteValue((DateTimeOffset)value);
                        return;

                    case PrimitiveTypeCode.DateTimeOffsetNullable:
                        writer.WriteValue((value == null) ? (DateTimeOffset?)null : (DateTimeOffset)value);
                        return;
#endif
                    case PrimitiveTypeCode.Decimal:
                        writer.WriteValue((decimal)value);
                        return;

                    case PrimitiveTypeCode.DecimalNullable:
                        writer.WriteValue((value == null) ? (decimal?)null : (decimal)value);
                        return;

                    case PrimitiveTypeCode.Guid:
                        writer.WriteValue((Guid)value);
                        return;

                    case PrimitiveTypeCode.GuidNullable:
                        writer.WriteValue((value == null) ? (Guid?)null : (Guid)value);
                        return;

                    case PrimitiveTypeCode.TimeSpan:
                        writer.WriteValue((TimeSpan)value);
                        return;

                    case PrimitiveTypeCode.TimeSpanNullable:
                        writer.WriteValue((value == null) ? (TimeSpan?)null : (TimeSpan)value);
                        return;

#if HAVE_BIG_INTEGER
                    case PrimitiveTypeCode.BigInteger:
                        writer.WriteValue((BigInteger)value);
                        return;

                    case PrimitiveTypeCode.BigIntegerNullable:
                        writer.WriteValue((value == null) ? (BigInteger?)null : (BigInteger)value);
                        return;
#endif
                    case PrimitiveTypeCode.Uri:
                        writer.WriteValue((Uri)value);
                        return;

                    case PrimitiveTypeCode.String:
                        writer.WriteValue((string)value);
                        return;

                    case PrimitiveTypeCode.Bytes:
                        writer.WriteValue((byte[])value);
                        return;

#if HAVE_DB_NULL_TYPE_CODE
                    case PrimitiveTypeCode.DBNull:
                        writer.WriteNull();
                        return;
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
                            writer.WriteNull();
                            return;
                        }

                        throw CreateUnsupportedTypeException(writer, value);
                }
            }
        }

#if HAVE_ICONVERTIBLE
        private static void ResolveConvertibleValue(IConvertible convertible, out PrimitiveTypeCode typeCode, out object value)
        {
            TypeInformation typeInformation = ConvertUtils.GetTypeInformation(convertible);
            typeCode = typeInformation.TypeCode == PrimitiveTypeCode.Object ? PrimitiveTypeCode.String : typeInformation.TypeCode;
            Type resolvedType = typeInformation.TypeCode == PrimitiveTypeCode.Object ? typeof(string) : typeInformation.Type;
            value = convertible.ToType(resolvedType, CultureInfo.InvariantCulture);
        }
#endif

        private static JsonWriterException CreateUnsupportedTypeException(JsonWriter writer, object value)
        {
            return JsonWriterException.Create(writer, "Unsupported type: {0}. Use the JsonSerializer class to get the object's JSON representation.".FormatWith(CultureInfo.InvariantCulture, value.GetType()), null);
        }
        protected void SetWriteState(JsonToken token, object value)
        {
            switch (token) {
                case JsonToken.StartObject:
                    InternalWriteStart(token, JsonContainerType.Object);
                    break;
                case JsonToken.StartArray:
                    InternalWriteStart(token, JsonContainerType.Array);
                    break;
                case JsonToken.StartConstructor:
                    InternalWriteStart(token, JsonContainerType.Constructor);
                    break;
                case JsonToken.PropertyName:
                    if (!(value is string s)) {
                        throw new ArgumentException("A name is required when setting property name state.", nameof(value));
                    }

                    InternalWritePropertyName(s);
                    break;
                case JsonToken.Comment:
                    InternalWriteComment();
                    break;
                case JsonToken.Raw:
                    InternalWriteRaw();
                    break;
                case JsonToken.Integer:
                case JsonToken.Float:
                case JsonToken.String:
                case JsonToken.Boolean:
                case JsonToken.Date:
                case JsonToken.Bytes:
                case JsonToken.Null:
                case JsonToken.Undefined:
                    InternalWriteValue(token);
                    break;
                case JsonToken.EndObject:
                    InternalWriteEnd(JsonContainerType.Object);
                    break;
                case JsonToken.EndArray:
                    InternalWriteEnd(JsonContainerType.Array);
                    break;
                case JsonToken.EndConstructor:
                    InternalWriteEnd(JsonContainerType.Constructor);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(token));
            }
        }

        internal void InternalWriteEnd(JsonContainerType container)
        {
            AutoCompleteClose(container);
        }

        internal void InternalWritePropertyName(string name)
        {
            _currentPosition.PropertyName = name;
            AutoComplete(JsonToken.PropertyName);
        }

        internal void InternalWriteRaw()
        {
        }

        internal void InternalWriteStart(JsonToken token, JsonContainerType container)
        {
            UpdateScopeWithFinishedValue();
            AutoComplete(token);
            Push(container);
        }

        internal void InternalWriteValue(JsonToken token)
        {
            UpdateScopeWithFinishedValue();
            AutoComplete(token);
        }

        internal void InternalWriteWhitespace(string ws)
        {
            if (ws != null) {
                if (!StringUtils.IsWhiteSpace(ws)) {
                    throw JsonWriterException.Create(this, "Only white space characters should be used.", null);
                }
            }
        }

        internal void InternalWriteComment()
        {
            AutoComplete(JsonToken.Comment);
        }
    }
}