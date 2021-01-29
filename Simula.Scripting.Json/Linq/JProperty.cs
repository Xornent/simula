
using Simula.Scripting.Json.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace Simula.Scripting.Json.Linq
{
    public partial class JProperty : JContainer
    {
        #region JPropertyList
        private class JPropertyList : IList<JToken>
        {
            internal JToken? _token;

            public IEnumerator<JToken> GetEnumerator()
            {
                if (_token != null) {
                    yield return _token;
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public void Add(JToken item)
            {
                _token = item;
            }

            public void Clear()
            {
                _token = null;
            }

            public bool Contains(JToken item)
            {
                return (_token == item);
            }

            public void CopyTo(JToken[] array, int arrayIndex)
            {
                if (_token != null) {
                    array[arrayIndex] = _token;
                }
            }

            public bool Remove(JToken item)
            {
                if (_token == item) {
                    _token = null;
                    return true;
                }
                return false;
            }

            public int Count => (_token != null) ? 1 : 0;

            public bool IsReadOnly => false;

            public int IndexOf(JToken item)
            {
                return (_token == item) ? 0 : -1;
            }

            public void Insert(int index, JToken item)
            {
                if (index == 0) {
                    _token = item;
                }
            }

            public void RemoveAt(int index)
            {
                if (index == 0) {
                    _token = null;
                }
            }

            public JToken this[int index] {
                get {
                    if (index != 0) {
                        throw new IndexOutOfRangeException();
                    }

                    MiscellaneousUtils.Assert(_token != null);
                    return _token;
                }
                set {
                    if (index != 0) {
                        throw new IndexOutOfRangeException();
                    }

                    _token = value;
                }
            }
        }
        #endregion

        private readonly JPropertyList _content = new JPropertyList();
        private readonly string _name;
        protected override IList<JToken> ChildrenTokens => _content;
        public string Name {
            [DebuggerStepThrough]
            get { return _name; }
        }
        public JToken Value {
            [DebuggerStepThrough]
            get { return _content._token!; }
            set {
                CheckReentrancy();

                JToken newValue = value ?? JValue.CreateNull();

                if (_content._token == null) {
                    InsertItem(0, newValue, false);
                } else {
                    SetItem(0, newValue);
                }
            }
        }
        public JProperty(JProperty other)
            : base(other)
        {
            _name = other.Name;
        }

        internal override JToken GetItem(int index)
        {
            if (index != 0) {
                throw new ArgumentOutOfRangeException();
            }

            return Value;
        }

        internal override void SetItem(int index, JToken? item)
        {
            if (index != 0) {
                throw new ArgumentOutOfRangeException();
            }

            if (IsTokenUnchanged(Value, item)) {
                return;
            }

            ((JObject?)Parent)?.InternalPropertyChanging(this);

            base.SetItem(0, item);

            ((JObject?)Parent)?.InternalPropertyChanged(this);
        }

        internal override bool RemoveItem(JToken? item)
        {
            throw new JsonException("Cannot add or remove items from {0}.".FormatWith(CultureInfo.InvariantCulture, typeof(JProperty)));
        }

        internal override void RemoveItemAt(int index)
        {
            throw new JsonException("Cannot add or remove items from {0}.".FormatWith(CultureInfo.InvariantCulture, typeof(JProperty)));
        }

        internal override int IndexOfItem(JToken? item)
        {
            if (item == null) {
                return -1;
            }

            return _content.IndexOf(item);
        }

        internal override void InsertItem(int index, JToken? item, bool skipParentCheck)
        {
            if (item != null && item.Type == JTokenType.Comment) {
                return;
            }

            if (Value != null) {
                throw new JsonException("{0} cannot have multiple values.".FormatWith(CultureInfo.InvariantCulture, typeof(JProperty)));
            }

            base.InsertItem(0, item, false);
        }

        internal override bool ContainsItem(JToken? item)
        {
            return (Value == item);
        }

        internal override void MergeItem(object content, JsonMergeSettings? settings)
        {
            JToken? value = (content as JProperty)?.Value;

            if (value != null && value.Type != JTokenType.Null) {
                Value = value;
            }
        }

        internal override void ClearItems()
        {
            throw new JsonException("Cannot add or remove items from {0}.".FormatWith(CultureInfo.InvariantCulture, typeof(JProperty)));
        }

        internal override bool DeepEquals(JToken node)
        {
            return (node is JProperty t && _name == t.Name && ContentsEqual(t));
        }

        internal override JToken CloneToken()
        {
            return new JProperty(this);
        }
        public override JTokenType Type {
            [DebuggerStepThrough]
            get { return JTokenType.Property; }
        }

        internal JProperty(string name)
        {
            ValidationUtils.ArgumentNotNull(name, nameof(name));

            _name = name;
        }
        public JProperty(string name, params object[] content)
            : this(name, (object)content)
        {
        }
        public JProperty(string name, object? content)
        {
            ValidationUtils.ArgumentNotNull(name, nameof(name));

            _name = name;

            Value = IsMultiContent(content)
                ? new JArray(content)
                : CreateFromContent(content);
        }
        public override void WriteTo(JsonWriter writer, params JsonConverter[] converters)
        {
            writer.WritePropertyName(_name);

            JToken value = Value;
            if (value != null) {
                value.WriteTo(writer, converters);
            } else {
                writer.WriteNull();
            }
        }

        internal override int GetDeepHashCode()
        {
            return _name.GetHashCode() ^ (Value?.GetDeepHashCode() ?? 0);
        }
        public new static JProperty Load(JsonReader reader)
        {
            return Load(reader, null);
        }
        public new static JProperty Load(JsonReader reader, JsonLoadSettings? settings)
        {
            if (reader.TokenType == JsonToken.None) {
                if (!reader.Read()) {
                    throw JsonReaderException.Create(reader, "Error reading JProperty from JsonReader.");
                }
            }

            reader.MoveToContent();

            if (reader.TokenType != JsonToken.PropertyName) {
                throw JsonReaderException.Create(reader, "Error reading JProperty from JsonReader. Current JsonReader item is not a property: {0}".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
            }

            JProperty p = new JProperty((string)reader.Value!);
            p.SetLineInfo(reader as IJsonLineInfo, settings);

            p.ReadTokenFrom(reader, settings);

            return p;
        }
    }
}