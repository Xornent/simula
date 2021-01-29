
using Simula.Scripting.Json.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Simula.Scripting.Json.Linq
{
    public partial class JConstructor : JContainer
    {
        private string? _name;
        private readonly List<JToken> _values = new List<JToken>();
        protected override IList<JToken> ChildrenTokens => _values;

        internal override int IndexOfItem(JToken? item)
        {
            if (item == null) {
                return -1;
            }

            return _values.IndexOfReference(item);
        }

        internal override void MergeItem(object content, JsonMergeSettings? settings)
        {
            if (!(content is JConstructor c)) {
                return;
            }

            if (c.Name != null) {
                Name = c.Name;
            }
            MergeEnumerableContent(this, c, settings);
        }
        public string? Name {
            get => _name;
            set => _name = value;
        }
        public override JTokenType Type => JTokenType.Constructor;
        public JConstructor()
        {
        }
        public JConstructor(JConstructor other)
            : base(other)
        {
            _name = other.Name;
        }
        public JConstructor(string name, params object[] content)
            : this(name, (object)content)
        {
        }
        public JConstructor(string name, object content)
            : this(name)
        {
            Add(content);
        }
        public JConstructor(string name)
        {
            if (name == null) {
                throw new ArgumentNullException(nameof(name));
            }

            if (name.Length == 0) {
                throw new ArgumentException("Constructor name cannot be empty.", nameof(name));
            }

            _name = name;
        }

        internal override bool DeepEquals(JToken node)
        {
            return (node is JConstructor c && _name == c.Name && ContentsEqual(c));
        }

        internal override JToken CloneToken()
        {
            return new JConstructor(this);
        }
        public override void WriteTo(JsonWriter writer, params JsonConverter[] converters)
        {
            writer.WriteStartConstructor(_name!);

            int count = _values.Count;
            for (int i = 0; i < count; i++) {
                _values[i].WriteTo(writer, converters);
            }

            writer.WriteEndConstructor();
        }
        public override JToken? this[object key] {
            get {
                ValidationUtils.ArgumentNotNull(key, nameof(key));

                if (!(key is int i)) {
                    throw new ArgumentException("Accessed JConstructor values with invalid key value: {0}. Argument position index expected.".FormatWith(CultureInfo.InvariantCulture, MiscellaneousUtils.ToString(key)));
                }

                return GetItem(i);
            }
            set {
                ValidationUtils.ArgumentNotNull(key, nameof(key));

                if (!(key is int i)) {
                    throw new ArgumentException("Set JConstructor values with invalid key value: {0}. Argument position index expected.".FormatWith(CultureInfo.InvariantCulture, MiscellaneousUtils.ToString(key)));
                }

                SetItem(i, value);
            }
        }

        internal override int GetDeepHashCode()
        {
            return (_name?.GetHashCode() ?? 0) ^ ContentsHashCode();
        }
        public new static JConstructor Load(JsonReader reader)
        {
            return Load(reader, null);
        }
        public new static JConstructor Load(JsonReader reader, JsonLoadSettings? settings)
        {
            if (reader.TokenType == JsonToken.None) {
                if (!reader.Read()) {
                    throw JsonReaderException.Create(reader, "Error reading JConstructor from JsonReader.");
                }
            }

            reader.MoveToContent();

            if (reader.TokenType != JsonToken.StartConstructor) {
                throw JsonReaderException.Create(reader, "Error reading JConstructor from JsonReader. Current JsonReader item is not a constructor: {0}".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
            }

            JConstructor c = new JConstructor((string)reader.Value!);
            c.SetLineInfo(reader as IJsonLineInfo, settings);

            c.ReadTokenFrom(reader, settings);

            return c;
        }
    }
}