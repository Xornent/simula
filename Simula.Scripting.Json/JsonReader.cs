
using System;
using System.Collections.Generic;
using System.Globalization;
#if HAVE_BIG_INTEGER
using System.Numerics;
#endif
using Simula.Scripting.Json.Serialization;
using Simula.Scripting.Json.Utilities;

namespace Simula.Scripting.Json
{
    public abstract partial class JsonReader : IDisposable
    {
        protected internal enum State
        {
            Start,
            Complete,
            Property,
            ObjectStart,
            Object,
            ArrayStart,
            Array,
            Closed,
            PostValue,
            ConstructorStart,
            Constructor,
            Error,
            Finished
        }
        private JsonToken _tokenType;
        private object? _value;
        internal char _quoteChar;
        internal State _currentState;
        private JsonPosition _currentPosition;
        private CultureInfo? _culture;
        private DateTimeZoneHandling _dateTimeZoneHandling;
        private int? _maxDepth;
        private bool _hasExceededMaxDepth;
        internal DateParseHandling _dateParseHandling;
        internal FloatParseHandling _floatParseHandling;
        private string? _dateFormatString;
        private List<JsonPosition>? _stack;
        protected State CurrentState => _currentState;
        public bool CloseInput { get; set; }
        public bool SupportMultipleContent { get; set; }
        public virtual char QuoteChar {
            get => _quoteChar;
            protected internal set => _quoteChar = value;
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
        public DateParseHandling DateParseHandling {
            get => _dateParseHandling;
            set {
                if (value < DateParseHandling.None ||
#if HAVE_DATE_TIME_OFFSET
                    value > DateParseHandling.DateTimeOffset
#else
                    value > DateParseHandling.DateTime
#endif
                    ) {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _dateParseHandling = value;
            }
        }
        public FloatParseHandling FloatParseHandling {
            get => _floatParseHandling;
            set {
                if (value < FloatParseHandling.Double || value > FloatParseHandling.Decimal) {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _floatParseHandling = value;
            }
        }
        public string? DateFormatString {
            get => _dateFormatString;
            set => _dateFormatString = value;
        }
        public int? MaxDepth {
            get => _maxDepth;
            set {
                if (value <= 0) {
                    throw new ArgumentException("Value must be positive.", nameof(value));
                }

                _maxDepth = value;
            }
        }
        public virtual JsonToken TokenType => _tokenType;
        public virtual object? Value => _value;
        public virtual Type? ValueType => _value?.GetType();
        public virtual int Depth {
            get {
                int depth = _stack?.Count ?? 0;
                if (JsonTokenUtils.IsStartToken(TokenType) || _currentPosition.Type == JsonContainerType.None) {
                    return depth;
                } else {
                    return depth + 1;
                }
            }
        }
        public virtual string Path {
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
        public CultureInfo Culture {
            get => _culture ?? CultureInfo.InvariantCulture;
            set => _culture = value;
        }

        internal JsonPosition GetPosition(int depth)
        {
            if (_stack != null && depth < _stack.Count) {
                return _stack[depth];
            }

            return _currentPosition;
        }
        protected JsonReader()
        {
            _currentState = State.Start;
            _dateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind;
            _dateParseHandling = DateParseHandling.DateTime;
            _floatParseHandling = FloatParseHandling.Double;

            CloseInput = true;
        }

        private void Push(JsonContainerType value)
        {
            UpdateScopeWithFinishedValue();

            if (_currentPosition.Type == JsonContainerType.None) {
                _currentPosition = new JsonPosition(value);
            } else {
                if (_stack == null) {
                    _stack = new List<JsonPosition>();
                }

                _stack.Add(_currentPosition);
                _currentPosition = new JsonPosition(value);
                if (_maxDepth != null && Depth + 1 > _maxDepth && !_hasExceededMaxDepth) {
                    _hasExceededMaxDepth = true;
                    throw JsonReaderException.Create(this, "The reader's MaxDepth of {0} has been exceeded.".FormatWith(CultureInfo.InvariantCulture, _maxDepth));
                }
            }
        }

        private JsonContainerType Pop()
        {
            JsonPosition oldPosition;
            if (_stack != null && _stack.Count > 0) {
                oldPosition = _currentPosition;
                _currentPosition = _stack[_stack.Count - 1];
                _stack.RemoveAt(_stack.Count - 1);
            } else {
                oldPosition = _currentPosition;
                _currentPosition = new JsonPosition();
            }

            if (_maxDepth != null && Depth <= _maxDepth) {
                _hasExceededMaxDepth = false;
            }

            return oldPosition.Type;
        }

        private JsonContainerType Peek()
        {
            return _currentPosition.Type;
        }
        public abstract bool Read();
        public virtual int? ReadAsInt32()
        {
            JsonToken t = GetContentToken();

            switch (t) {
                case JsonToken.None:
                case JsonToken.Null:
                case JsonToken.EndArray:
                    return null;
                case JsonToken.Integer:
                case JsonToken.Float:
                    object v = Value!;
                    if (v is int i) {
                        return i;
                    }

#if HAVE_BIG_INTEGER
                    if (v is BigInteger value)
                    {
                        i = (int)value;
                    }
                    else
#endif
                    {
                        try {
                            i = Convert.ToInt32(v, CultureInfo.InvariantCulture);
                        } catch (Exception ex) {
                            throw JsonReaderException.Create(this, "Could not convert to integer: {0}.".FormatWith(CultureInfo.InvariantCulture, v), ex);
                        }
                    }

                    SetToken(JsonToken.Integer, i, false);
                    return i;
                case JsonToken.String:
                    string? s = (string?)Value;
                    return ReadInt32String(s);
            }

            throw JsonReaderException.Create(this, "Error reading integer. Unexpected token: {0}.".FormatWith(CultureInfo.InvariantCulture, t));
        }

        internal int? ReadInt32String(string? s)
        {
            if (StringUtils.IsNullOrEmpty(s)) {
                SetToken(JsonToken.Null, null, false);
                return null;
            }

            if (int.TryParse(s, NumberStyles.Integer, Culture, out int i)) {
                SetToken(JsonToken.Integer, i, false);
                return i;
            } else {
                SetToken(JsonToken.String, s, false);
                throw JsonReaderException.Create(this, "Could not convert string to integer: {0}.".FormatWith(CultureInfo.InvariantCulture, s));
            }
        }
        public virtual string? ReadAsString()
        {
            JsonToken t = GetContentToken();

            switch (t) {
                case JsonToken.None:
                case JsonToken.Null:
                case JsonToken.EndArray:
                    return null;
                case JsonToken.String:
                    return (string?)Value;
            }

            if (JsonTokenUtils.IsPrimitiveToken(t)) {
                object? v = Value;
                if (v != null) {
                    string s;
                    if (v is IFormattable formattable) {
                        s = formattable.ToString(null, Culture);
                    } else {
                        s = v is Uri uri ? uri.OriginalString : v.ToString();
                    }

                    SetToken(JsonToken.String, s, false);
                    return s;
                }
            }

            throw JsonReaderException.Create(this, "Error reading string. Unexpected token: {0}.".FormatWith(CultureInfo.InvariantCulture, t));
        }
        public virtual byte[]? ReadAsBytes()
        {
            JsonToken t = GetContentToken();

            switch (t) {
                case JsonToken.StartObject: {
                        ReadIntoWrappedTypeObject();

                        byte[]? data = ReadAsBytes();
                        ReaderReadAndAssert();

                        if (TokenType != JsonToken.EndObject) {
                            throw JsonReaderException.Create(this, "Error reading bytes. Unexpected token: {0}.".FormatWith(CultureInfo.InvariantCulture, TokenType));
                        }

                        SetToken(JsonToken.Bytes, data, false);
                        return data;
                    }
                case JsonToken.String: {
                        string s = (string)Value!;

                        byte[] data;

                        if (s.Length == 0) {
                            data = CollectionUtils.ArrayEmpty<byte>();
                        } else if (ConvertUtils.TryConvertGuid(s, out Guid g1)) {
                            data = g1.ToByteArray();
                        } else {
                            data = Convert.FromBase64String(s);
                        }

                        SetToken(JsonToken.Bytes, data, false);
                        return data;
                    }
                case JsonToken.None:
                case JsonToken.Null:
                case JsonToken.EndArray:
                    return null;
                case JsonToken.Bytes:
                    if (Value is Guid g2) {
                        byte[] data = g2.ToByteArray();
                        SetToken(JsonToken.Bytes, data, false);
                        return data;
                    }

                    return (byte[]?)Value;
                case JsonToken.StartArray:
                    return ReadArrayIntoByteArray();
            }

            throw JsonReaderException.Create(this, "Error reading bytes. Unexpected token: {0}.".FormatWith(CultureInfo.InvariantCulture, t));
        }

        internal byte[] ReadArrayIntoByteArray()
        {
            List<byte> buffer = new List<byte>();

            while (true) {
                if (!Read()) {
                    SetToken(JsonToken.None);
                }

                if (ReadArrayElementIntoByteArrayReportDone(buffer)) {
                    byte[] d = buffer.ToArray();
                    SetToken(JsonToken.Bytes, d, false);
                    return d;
                }
            }
        }

        private bool ReadArrayElementIntoByteArrayReportDone(List<byte> buffer)
        {
            switch (TokenType) {
                case JsonToken.None:
                    throw JsonReaderException.Create(this, "Unexpected end when reading bytes.");
                case JsonToken.Integer:
                    buffer.Add(Convert.ToByte(Value, CultureInfo.InvariantCulture));
                    return false;
                case JsonToken.EndArray:
                    return true;
                case JsonToken.Comment:
                    return false;
                default:
                    throw JsonReaderException.Create(this, "Unexpected token when reading bytes: {0}.".FormatWith(CultureInfo.InvariantCulture, TokenType));
            }
        }
        public virtual double? ReadAsDouble()
        {
            JsonToken t = GetContentToken();

            switch (t) {
                case JsonToken.None:
                case JsonToken.Null:
                case JsonToken.EndArray:
                    return null;
                case JsonToken.Integer:
                case JsonToken.Float:
                    object v = Value!;
                    if (v is double d) {
                        return d;
                    }

#if HAVE_BIG_INTEGER
                    if (v is BigInteger value)
                    {
                        d = (double)value;
                    }
                    else
#endif
                    {
                        d = Convert.ToDouble(v, CultureInfo.InvariantCulture);
                    }

                    SetToken(JsonToken.Float, d, false);

                    return (double)d;
                case JsonToken.String:
                    return ReadDoubleString((string?)Value);
            }

            throw JsonReaderException.Create(this, "Error reading double. Unexpected token: {0}.".FormatWith(CultureInfo.InvariantCulture, t));
        }

        internal double? ReadDoubleString(string? s)
        {
            if (StringUtils.IsNullOrEmpty(s)) {
                SetToken(JsonToken.Null, null, false);
                return null;
            }

            if (double.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, Culture, out double d)) {
                SetToken(JsonToken.Float, d, false);
                return d;
            } else {
                SetToken(JsonToken.String, s, false);
                throw JsonReaderException.Create(this, "Could not convert string to double: {0}.".FormatWith(CultureInfo.InvariantCulture, s));
            }
        }
        public virtual bool? ReadAsBoolean()
        {
            JsonToken t = GetContentToken();

            switch (t) {
                case JsonToken.None:
                case JsonToken.Null:
                case JsonToken.EndArray:
                    return null;
                case JsonToken.Integer:
                case JsonToken.Float:
                    bool b;
#if HAVE_BIG_INTEGER
                    if (Value is BigInteger integer)
                    {
                        b = integer != 0;
                    }
                    else
#endif
                    {
                        b = Convert.ToBoolean(Value, CultureInfo.InvariantCulture);
                    }

                    SetToken(JsonToken.Boolean, b, false);
                    return b;
                case JsonToken.String:
                    return ReadBooleanString((string?)Value);
                case JsonToken.Boolean:
                    return (bool)Value!;
            }

            throw JsonReaderException.Create(this, "Error reading boolean. Unexpected token: {0}.".FormatWith(CultureInfo.InvariantCulture, t));
        }

        internal bool? ReadBooleanString(string? s)
        {
            if (StringUtils.IsNullOrEmpty(s)) {
                SetToken(JsonToken.Null, null, false);
                return null;
            }

            if (bool.TryParse(s, out bool b)) {
                SetToken(JsonToken.Boolean, b, false);
                return b;
            } else {
                SetToken(JsonToken.String, s, false);
                throw JsonReaderException.Create(this, "Could not convert string to boolean: {0}.".FormatWith(CultureInfo.InvariantCulture, s));
            }
        }
        public virtual decimal? ReadAsDecimal()
        {
            JsonToken t = GetContentToken();

            switch (t) {
                case JsonToken.None:
                case JsonToken.Null:
                case JsonToken.EndArray:
                    return null;
                case JsonToken.Integer:
                case JsonToken.Float:
                    object v = Value!;

                    if (v is decimal d) {
                        return d;
                    }

#if HAVE_BIG_INTEGER
                    if (v is BigInteger value)
                    {
                        d = (decimal)value;
                    }
                    else
#endif
                    {
                        try {
                            d = Convert.ToDecimal(v, CultureInfo.InvariantCulture);
                        } catch (Exception ex) {
                            throw JsonReaderException.Create(this, "Could not convert to decimal: {0}.".FormatWith(CultureInfo.InvariantCulture, v), ex);
                        }
                    }

                    SetToken(JsonToken.Float, d, false);
                    return d;
                case JsonToken.String:
                    return ReadDecimalString((string?)Value);
            }

            throw JsonReaderException.Create(this, "Error reading decimal. Unexpected token: {0}.".FormatWith(CultureInfo.InvariantCulture, t));
        }

        internal decimal? ReadDecimalString(string? s)
        {
            if (StringUtils.IsNullOrEmpty(s)) {
                SetToken(JsonToken.Null, null, false);
                return null;
            }

            if (decimal.TryParse(s, NumberStyles.Number, Culture, out decimal d)) {
                SetToken(JsonToken.Float, d, false);
                return d;
            } else if (ConvertUtils.DecimalTryParse(s.ToCharArray(), 0, s.Length, out d) == ParseResult.Success) {
                SetToken(JsonToken.Float, d, false);
                return d;
            } else {
                SetToken(JsonToken.String, s, false);
                throw JsonReaderException.Create(this, "Could not convert string to decimal: {0}.".FormatWith(CultureInfo.InvariantCulture, s));
            }
        }
        public virtual DateTime? ReadAsDateTime()
        {
            switch (GetContentToken()) {
                case JsonToken.None:
                case JsonToken.Null:
                case JsonToken.EndArray:
                    return null;
                case JsonToken.Date:
#if HAVE_DATE_TIME_OFFSET
                    if (Value is DateTimeOffset offset) {
                        SetToken(JsonToken.Date, offset.DateTime, false);
                    }
#endif

                    return (DateTime)Value!;
                case JsonToken.String:
                    return ReadDateTimeString((string?)Value);
            }

            throw JsonReaderException.Create(this, "Error reading date. Unexpected token: {0}.".FormatWith(CultureInfo.InvariantCulture, TokenType));
        }

        internal DateTime? ReadDateTimeString(string? s)
        {
            if (StringUtils.IsNullOrEmpty(s)) {
                SetToken(JsonToken.Null, null, false);
                return null;
            }

            if (DateTimeUtils.TryParseDateTime(s, DateTimeZoneHandling, _dateFormatString, Culture, out DateTime dt)) {
                dt = DateTimeUtils.EnsureDateTime(dt, DateTimeZoneHandling);
                SetToken(JsonToken.Date, dt, false);
                return dt;
            }

            if (DateTime.TryParse(s, Culture, DateTimeStyles.RoundtripKind, out dt)) {
                dt = DateTimeUtils.EnsureDateTime(dt, DateTimeZoneHandling);
                SetToken(JsonToken.Date, dt, false);
                return dt;
            }

            throw JsonReaderException.Create(this, "Could not convert string to DateTime: {0}.".FormatWith(CultureInfo.InvariantCulture, s));
        }

#if HAVE_DATE_TIME_OFFSET
        public virtual DateTimeOffset? ReadAsDateTimeOffset()
        {
            JsonToken t = GetContentToken();

            switch (t) {
                case JsonToken.None:
                case JsonToken.Null:
                case JsonToken.EndArray:
                    return null;
                case JsonToken.Date:
                    if (Value is DateTime time) {
                        SetToken(JsonToken.Date, new DateTimeOffset(time), false);
                    }

                    return (DateTimeOffset)Value!;
                case JsonToken.String:
                    string? s = (string?)Value;
                    return ReadDateTimeOffsetString(s);
                default:
                    throw JsonReaderException.Create(this, "Error reading date. Unexpected token: {0}.".FormatWith(CultureInfo.InvariantCulture, t));
            }
        }

        internal DateTimeOffset? ReadDateTimeOffsetString(string? s)
        {
            if (StringUtils.IsNullOrEmpty(s)) {
                SetToken(JsonToken.Null, null, false);
                return null;
            }

            if (DateTimeUtils.TryParseDateTimeOffset(s, _dateFormatString, Culture, out DateTimeOffset dt)) {
                SetToken(JsonToken.Date, dt, false);
                return dt;
            }

            if (DateTimeOffset.TryParse(s, Culture, DateTimeStyles.RoundtripKind, out dt)) {
                SetToken(JsonToken.Date, dt, false);
                return dt;
            }

            SetToken(JsonToken.String, s, false);
            throw JsonReaderException.Create(this, "Could not convert string to DateTimeOffset: {0}.".FormatWith(CultureInfo.InvariantCulture, s));
        }
#endif

        internal void ReaderReadAndAssert()
        {
            if (!Read()) {
                throw CreateUnexpectedEndException();
            }
        }

        internal JsonReaderException CreateUnexpectedEndException()
        {
            return JsonReaderException.Create(this, "Unexpected end when reading JSON.");
        }

        internal void ReadIntoWrappedTypeObject()
        {
            ReaderReadAndAssert();
            if (Value != null && Value.ToString() == JsonTypeReflector.TypePropertyName) {
                ReaderReadAndAssert();
                if (Value != null && Value.ToString().StartsWith("System.Byte[]", StringComparison.Ordinal)) {
                    ReaderReadAndAssert();
                    if (Value.ToString() == JsonTypeReflector.ValuePropertyName) {
                        return;
                    }
                }
            }

            throw JsonReaderException.Create(this, "Error reading bytes. Unexpected token: {0}.".FormatWith(CultureInfo.InvariantCulture, JsonToken.StartObject));
        }
        public void Skip()
        {
            if (TokenType == JsonToken.PropertyName) {
                Read();
            }

            if (JsonTokenUtils.IsStartToken(TokenType)) {
                int depth = Depth;

                while (Read() && (depth < Depth)) {
                }
            }
        }
        protected void SetToken(JsonToken newToken)
        {
            SetToken(newToken, null, true);
        }
        protected void SetToken(JsonToken newToken, object? value)
        {
            SetToken(newToken, value, true);
        }
        protected void SetToken(JsonToken newToken, object? value, bool updateIndex)
        {
            _tokenType = newToken;
            _value = value;

            switch (newToken) {
                case JsonToken.StartObject:
                    _currentState = State.ObjectStart;
                    Push(JsonContainerType.Object);
                    break;
                case JsonToken.StartArray:
                    _currentState = State.ArrayStart;
                    Push(JsonContainerType.Array);
                    break;
                case JsonToken.StartConstructor:
                    _currentState = State.ConstructorStart;
                    Push(JsonContainerType.Constructor);
                    break;
                case JsonToken.EndObject:
                    ValidateEnd(JsonToken.EndObject);
                    break;
                case JsonToken.EndArray:
                    ValidateEnd(JsonToken.EndArray);
                    break;
                case JsonToken.EndConstructor:
                    ValidateEnd(JsonToken.EndConstructor);
                    break;
                case JsonToken.PropertyName:
                    _currentState = State.Property;

                    _currentPosition.PropertyName = (string)value!;
                    break;
                case JsonToken.Undefined:
                case JsonToken.Integer:
                case JsonToken.Float:
                case JsonToken.Boolean:
                case JsonToken.Null:
                case JsonToken.Date:
                case JsonToken.String:
                case JsonToken.Raw:
                case JsonToken.Bytes:
                    SetPostValueState(updateIndex);
                    break;
            }
        }

        internal void SetPostValueState(bool updateIndex)
        {
            if (Peek() != JsonContainerType.None || SupportMultipleContent) {
                _currentState = State.PostValue;
            } else {
                SetFinished();
            }

            if (updateIndex) {
                UpdateScopeWithFinishedValue();
            }
        }

        private void UpdateScopeWithFinishedValue()
        {
            if (_currentPosition.HasIndex) {
                _currentPosition.Position++;
            }
        }

        private void ValidateEnd(JsonToken endToken)
        {
            JsonContainerType currentObject = Pop();

            if (GetTypeForCloseToken(endToken) != currentObject) {
                throw JsonReaderException.Create(this, "JsonToken {0} is not valid for closing JsonType {1}.".FormatWith(CultureInfo.InvariantCulture, endToken, currentObject));
            }

            if (Peek() != JsonContainerType.None || SupportMultipleContent) {
                _currentState = State.PostValue;
            } else {
                SetFinished();
            }
        }
        protected void SetStateBasedOnCurrent()
        {
            JsonContainerType currentObject = Peek();

            switch (currentObject) {
                case JsonContainerType.Object:
                    _currentState = State.Object;
                    break;
                case JsonContainerType.Array:
                    _currentState = State.Array;
                    break;
                case JsonContainerType.Constructor:
                    _currentState = State.Constructor;
                    break;
                case JsonContainerType.None:
                    SetFinished();
                    break;
                default:
                    throw JsonReaderException.Create(this, "While setting the reader state back to current object an unexpected JsonType was encountered: {0}".FormatWith(CultureInfo.InvariantCulture, currentObject));
            }
        }

        private void SetFinished()
        {
            _currentState = SupportMultipleContent ? State.Start : State.Finished;
        }

        private JsonContainerType GetTypeForCloseToken(JsonToken token)
        {
            switch (token) {
                case JsonToken.EndObject:
                    return JsonContainerType.Object;
                case JsonToken.EndArray:
                    return JsonContainerType.Array;
                case JsonToken.EndConstructor:
                    return JsonContainerType.Constructor;
                default:
                    throw JsonReaderException.Create(this, "Not a valid close JsonToken: {0}".FormatWith(CultureInfo.InvariantCulture, token));
            }
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
        public virtual void Close()
        {
            _currentState = State.Closed;
            _tokenType = JsonToken.None;
            _value = null;
        }

        internal void ReadAndAssert()
        {
            if (!Read()) {
                throw JsonSerializationException.Create(this, "Unexpected end when reading JSON.");
            }
        }

        internal void ReadForTypeAndAssert(JsonContract? contract, bool hasConverter)
        {
            if (!ReadForType(contract, hasConverter)) {
                throw JsonSerializationException.Create(this, "Unexpected end when reading JSON.");
            }
        }

        internal bool ReadForType(JsonContract? contract, bool hasConverter)
        {
            if (hasConverter) {
                return Read();
            }

            ReadType t = contract?.InternalReadType ?? ReadType.Read;

            switch (t) {
                case ReadType.Read:
                    return ReadAndMoveToContent();
                case ReadType.ReadAsInt32:
                    ReadAsInt32();
                    break;
                case ReadType.ReadAsInt64:
                    bool result = ReadAndMoveToContent();
                    if (TokenType == JsonToken.Undefined) {
                        throw JsonReaderException.Create(this, "An undefined token is not a valid {0}.".FormatWith(CultureInfo.InvariantCulture, contract?.UnderlyingType ?? typeof(long)));
                    }
                    return result;
                case ReadType.ReadAsDecimal:
                    ReadAsDecimal();
                    break;
                case ReadType.ReadAsDouble:
                    ReadAsDouble();
                    break;
                case ReadType.ReadAsBytes:
                    ReadAsBytes();
                    break;
                case ReadType.ReadAsBoolean:
                    ReadAsBoolean();
                    break;
                case ReadType.ReadAsString:
                    ReadAsString();
                    break;
                case ReadType.ReadAsDateTime:
                    ReadAsDateTime();
                    break;
#if HAVE_DATE_TIME_OFFSET
                case ReadType.ReadAsDateTimeOffset:
                    ReadAsDateTimeOffset();
                    break;
#endif
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return (TokenType != JsonToken.None);
        }

        internal bool ReadAndMoveToContent()
        {
            return Read() && MoveToContent();
        }

        internal bool MoveToContent()
        {
            JsonToken t = TokenType;
            while (t == JsonToken.None || t == JsonToken.Comment) {
                if (!Read()) {
                    return false;
                }

                t = TokenType;
            }

            return true;
        }

        private JsonToken GetContentToken()
        {
            JsonToken t;
            do {
                if (!Read()) {
                    SetToken(JsonToken.None);
                    return JsonToken.None;
                } else {
                    t = TokenType;
                }
            } while (t == JsonToken.Comment);

            return t;
        }
    }
}
