
using System;
using System.Collections;
using System.Collections.Generic;
using Simula.Scripting.Json.Utilities;
using System.IO;
using System.Globalization;

namespace Simula.Scripting.Json.Linq
{
    public partial class JArray : JContainer, IList<JToken>
    {
        private readonly List<JToken> _values = new List<JToken>();
        protected override IList<JToken> ChildrenTokens => _values;
        public override JTokenType Type => JTokenType.Array;
        public JArray()
        {
        }
        public JArray(JArray other)
            : base(other)
        {
        }
        public JArray(params object[] content)
            : this((object)content)
        {
        }
        public JArray(object content)
        {
            Add(content);
        }

        internal override bool DeepEquals(JToken node)
        {
            return (node is JArray t && ContentsEqual(t));
        }

        internal override JToken CloneToken()
        {
            return new JArray(this);
        }
        public new static JArray Load(JsonReader reader)
        {
            return Load(reader, null);
        }
        public new static JArray Load(JsonReader reader, JsonLoadSettings? settings)
        {
            if (reader.TokenType == JsonToken.None)
            {
                if (!reader.Read())
                {
                    throw JsonReaderException.Create(reader, "Error reading JArray from JsonReader.");
                }
            }

            reader.MoveToContent();

            if (reader.TokenType != JsonToken.StartArray)
            {
                throw JsonReaderException.Create(reader, "Error reading JArray from JsonReader. Current JsonReader item is not an array: {0}".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
            }

            JArray a = new JArray();
            a.SetLineInfo(reader as IJsonLineInfo, settings);

            a.ReadTokenFrom(reader, settings);

            return a;
        }
        public new static JArray Parse(string json)
        {
            return Parse(json, null);
        }
        public new static JArray Parse(string json, JsonLoadSettings? settings)
        {
            using (JsonReader reader = new JsonTextReader(new StringReader(json)))
            {
                JArray a = Load(reader, settings);

                while (reader.Read())
                {
                }

                return a;
            }
        }
        public new static JArray FromObject(object o)
        {
            return FromObject(o, JsonSerializer.CreateDefault());
        }
        public new static JArray FromObject(object o, JsonSerializer jsonSerializer)
        {
            JToken token = FromObjectInternal(o, jsonSerializer);

            if (token.Type != JTokenType.Array)
            {
                throw new ArgumentException("Object serialized to {0}. JArray instance expected.".FormatWith(CultureInfo.InvariantCulture, token.Type));
            }

            return (JArray)token;
        }
        public override void WriteTo(JsonWriter writer, params JsonConverter[] converters)
        {
            writer.WriteStartArray();

            for (int i = 0; i < _values.Count; i++)
            {
                _values[i].WriteTo(writer, converters);
            }

            writer.WriteEndArray();
        }
        public override JToken? this[object key]
        {
            get
            {
                ValidationUtils.ArgumentNotNull(key, nameof(key));

                if (!(key is int))
                {
                    throw new ArgumentException("Accessed JArray values with invalid key value: {0}. Int32 array index expected.".FormatWith(CultureInfo.InvariantCulture, MiscellaneousUtils.ToString(key)));
                }

                return GetItem((int)key);
            }
            set
            {
                ValidationUtils.ArgumentNotNull(key, nameof(key));

                if (!(key is int))
                {
                    throw new ArgumentException("Set JArray values with invalid key value: {0}. Int32 array index expected.".FormatWith(CultureInfo.InvariantCulture, MiscellaneousUtils.ToString(key)));
                }

                SetItem((int)key, value);
            }
        }
        public JToken this[int index]
        {
            get => GetItem(index);
            set => SetItem(index, value);
        }

        internal override int IndexOfItem(JToken? item)
        {
            if (item == null)
            {
                return -1;
            }

            return _values.IndexOfReference(item);
        }

        internal override void MergeItem(object content, JsonMergeSettings? settings)
        {
            IEnumerable? a = (IsMultiContent(content) || content is JArray)
                ? (IEnumerable)content
                : null;
            if (a == null)
            {
                return;
            }

            MergeEnumerableContent(this, a, settings);
        }

        #region IList<JToken> Members
        public int IndexOf(JToken item)
        {
            return IndexOfItem(item);
        }
        public void Insert(int index, JToken item)
        {
            InsertItem(index, item, false);
        }
        public void RemoveAt(int index)
        {
            RemoveItemAt(index);
        }
        public IEnumerator<JToken> GetEnumerator()
        {
            return Children().GetEnumerator();
        }
        #endregion

        #region ICollection<JToken> Members
        public void Add(JToken item)
        {
            Add((object)item);
        }
        public void Clear()
        {
            ClearItems();
        }
        public bool Contains(JToken item)
        {
            return ContainsItem(item);
        }
        public void CopyTo(JToken[] array, int arrayIndex)
        {
            CopyItemsTo(array, arrayIndex);
        }
        public bool IsReadOnly => false;
        public bool Remove(JToken item)
        {
            return RemoveItem(item);
        }
        #endregion

        internal override int GetDeepHashCode()
        {
            return ContentsHashCode();
        }
    }
}