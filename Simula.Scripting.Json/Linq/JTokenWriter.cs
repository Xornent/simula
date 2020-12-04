
using System;
#if HAVE_BIG_INTEGER
using System.Numerics;
#endif
using Simula.Scripting.Json.Utilities;

namespace Simula.Scripting.Json.Linq
{
    public partial class JTokenWriter : JsonWriter
    {
        private JContainer? _token;
        private JContainer? _parent;
        private JValue? _value;
        private JToken? _current;
        public JToken? CurrentToken => _current;
        public JToken? Token {
            get {
                if (_token != null) {
                    return _token;
                }

                return _value;
            }
        }
        public JTokenWriter(JContainer container)
        {
            ValidationUtils.ArgumentNotNull(container, nameof(container));

            _token = container;
            _parent = container;
        }
        public JTokenWriter()
        {
        }
        public override void Flush()
        {
        }
        public override void Close()
        {
            base.Close();
        }
        public override void WriteStartObject()
        {
            base.WriteStartObject();

            AddParent(new JObject());
        }

        private void AddParent(JContainer container)
        {
            if (_parent == null) {
                _token = container;
            } else {
                _parent.AddAndSkipParentCheck(container);
            }

            _parent = container;
            _current = container;
        }

        private void RemoveParent()
        {
            _current = _parent;
            _parent = _parent!.Parent;

            if (_parent != null && _parent.Type == JTokenType.Property) {
                _parent = _parent.Parent;
            }
        }
        public override void WriteStartArray()
        {
            base.WriteStartArray();

            AddParent(new JArray());
        }
        public override void WriteStartConstructor(string name)
        {
            base.WriteStartConstructor(name);

            AddParent(new JConstructor(name));
        }
        protected override void WriteEnd(JsonToken token)
        {
            RemoveParent();
        }
        public override void WritePropertyName(string name)
        {
            (_parent as JObject)?.Remove(name);

            AddParent(new JProperty(name));
            base.WritePropertyName(name);
        }

        private void AddValue(object? value, JsonToken token)
        {
            AddValue(new JValue(value), token);
        }

        internal void AddValue(JValue? value, JsonToken token)
        {
            if (_parent != null) {
                _parent.Add(value);
                _current = _parent.Last;

                if (_parent.Type == JTokenType.Property) {
                    _parent = _parent.Parent;
                }
            } else {
                _value = value ?? JValue.CreateNull();
                _current = _value;
            }
        }

        #region WriteValue methods
        public override void WriteValue(object? value)
        {
#if HAVE_BIG_INTEGER
            if (value is BigInteger)
            {
                InternalWriteValue(JsonToken.Integer);
                AddValue(value, JsonToken.Integer);
            }
            else
#endif
            {
                base.WriteValue(value);
            }
        }
        public override void WriteNull()
        {
            base.WriteNull();
            AddValue(null, JsonToken.Null);
        }
        public override void WriteUndefined()
        {
            base.WriteUndefined();
            AddValue(null, JsonToken.Undefined);
        }
        public override void WriteRaw(string? json)
        {
            base.WriteRaw(json);
            AddValue(new JRaw(json), JsonToken.Raw);
        }
        public override void WriteComment(string? text)
        {
            base.WriteComment(text);
            AddValue(JValue.CreateComment(text), JsonToken.Comment);
        }
        public override void WriteValue(string? value)
        {
            base.WriteValue(value);
            AddValue(value, JsonToken.String);
        }
        public override void WriteValue(int value)
        {
            base.WriteValue(value);
            AddValue(value, JsonToken.Integer);
        }
        [CLSCompliant(false)]
        public override void WriteValue(uint value)
        {
            base.WriteValue(value);
            AddValue(value, JsonToken.Integer);
        }
        public override void WriteValue(long value)
        {
            base.WriteValue(value);
            AddValue(value, JsonToken.Integer);
        }
        [CLSCompliant(false)]
        public override void WriteValue(ulong value)
        {
            base.WriteValue(value);
            AddValue(value, JsonToken.Integer);
        }
        public override void WriteValue(float value)
        {
            base.WriteValue(value);
            AddValue(value, JsonToken.Float);
        }
        public override void WriteValue(double value)
        {
            base.WriteValue(value);
            AddValue(value, JsonToken.Float);
        }
        public override void WriteValue(bool value)
        {
            base.WriteValue(value);
            AddValue(value, JsonToken.Boolean);
        }
        public override void WriteValue(short value)
        {
            base.WriteValue(value);
            AddValue(value, JsonToken.Integer);
        }
        [CLSCompliant(false)]
        public override void WriteValue(ushort value)
        {
            base.WriteValue(value);
            AddValue(value, JsonToken.Integer);
        }
        public override void WriteValue(char value)
        {
            base.WriteValue(value);
            string s;
#if HAVE_CHAR_TO_STRING_WITH_CULTURE
            s = value.ToString(CultureInfo.InvariantCulture);
#else
            s = value.ToString();
#endif
            AddValue(s, JsonToken.String);
        }
        public override void WriteValue(byte value)
        {
            base.WriteValue(value);
            AddValue(value, JsonToken.Integer);
        }
        [CLSCompliant(false)]
        public override void WriteValue(sbyte value)
        {
            base.WriteValue(value);
            AddValue(value, JsonToken.Integer);
        }
        public override void WriteValue(decimal value)
        {
            base.WriteValue(value);
            AddValue(value, JsonToken.Float);
        }
        public override void WriteValue(DateTime value)
        {
            base.WriteValue(value);
            value = DateTimeUtils.EnsureDateTime(value, DateTimeZoneHandling);
            AddValue(value, JsonToken.Date);
        }

#if HAVE_DATE_TIME_OFFSET
        public override void WriteValue(DateTimeOffset value)
        {
            base.WriteValue(value);
            AddValue(value, JsonToken.Date);
        }
#endif
        public override void WriteValue(byte[]? value)
        {
            base.WriteValue(value);
            AddValue(value, JsonToken.Bytes);
        }
        public override void WriteValue(TimeSpan value)
        {
            base.WriteValue(value);
            AddValue(value, JsonToken.String);
        }
        public override void WriteValue(Guid value)
        {
            base.WriteValue(value);
            AddValue(value, JsonToken.String);
        }
        public override void WriteValue(Uri? value)
        {
            base.WriteValue(value);
            AddValue(value, JsonToken.String);
        }
        #endregion

        internal override void WriteToken(JsonReader reader, bool writeChildren, bool writeDateConstructorAsDate, bool writeComments)
        {
            if (reader is JTokenReader tokenReader && writeChildren && writeDateConstructorAsDate && writeComments) {
                if (tokenReader.TokenType == JsonToken.None) {
                    if (!tokenReader.Read()) {
                        return;
                    }
                }

                JToken value = tokenReader.CurrentToken!.CloneToken();

                if (_parent != null) {
                    _parent.Add(value);
                    _current = _parent.Last;
                    if (_parent.Type == JTokenType.Property) {
                        _parent = _parent.Parent;
                        InternalWriteValue(JsonToken.Null);
                    }
                } else {
                    _current = value;

                    if (_token == null && _value == null) {
                        _token = value as JContainer;
                        _value = value as JValue;
                    }
                }

                tokenReader.Skip();
            } else {
                base.WriteToken(reader, writeChildren, writeDateConstructorAsDate, writeComments);
            }
        }
    }
}