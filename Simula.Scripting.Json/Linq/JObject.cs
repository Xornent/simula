
using System;
using System.Collections.Generic;
#if HAVE_INOTIFY_COLLECTION_CHANGED
using System.Collections.ObjectModel;
using System.Collections.Specialized;
#endif
using System.ComponentModel;
#if HAVE_DYNAMIC
using System.Dynamic;
using System.Linq.Expressions;
#endif
using System.IO;
using Simula.Scripting.Json.Utilities;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;
#if !HAVE_LINQ
using Simula.Scripting.Json.Utilities.LinqBridge;
#else
using System.Linq;
#endif

namespace Simula.Scripting.Json.Linq
{
    public partial class JObject : JContainer, IDictionary<string, JToken?>, INotifyPropertyChanged
#if HAVE_COMPONENT_MODEL
        , ICustomTypeDescriptor
#endif
#if HAVE_INOTIFY_PROPERTY_CHANGING
        , INotifyPropertyChanging
#endif
    {
        private readonly JPropertyKeyedCollection _properties = new JPropertyKeyedCollection();
        protected override IList<JToken> ChildrenTokens => _properties;
        public event PropertyChangedEventHandler? PropertyChanged;

#if HAVE_INOTIFY_PROPERTY_CHANGING
        public event PropertyChangingEventHandler? PropertyChanging;
#endif
        public JObject()
        {
        }
        public JObject(JObject other)
            : base(other)
        {
        }
        public JObject(params object[] content)
            : this((object)content)
        {
        }
        public JObject(object content)
        {
            Add(content);
        }

        internal override bool DeepEquals(JToken node)
        {
            if (!(node is JObject t))
            {
                return false;
            }

            return _properties.Compare(t._properties);
        }

        internal override int IndexOfItem(JToken? item)
        {
            if (item == null)
            {
                return -1;
            }

            return _properties.IndexOfReference(item);
        }

        internal override void InsertItem(int index, JToken? item, bool skipParentCheck)
        {
            if (item != null && item.Type == JTokenType.Comment)
            {
                return;
            }

            base.InsertItem(index, item, skipParentCheck);
        }

        internal override void ValidateToken(JToken o, JToken? existing)
        {
            ValidationUtils.ArgumentNotNull(o, nameof(o));

            if (o.Type != JTokenType.Property)
            {
                throw new ArgumentException("Can not add {0} to {1}.".FormatWith(CultureInfo.InvariantCulture, o.GetType(), GetType()));
            }

            JProperty newProperty = (JProperty)o;

            if (existing != null)
            {
                JProperty existingProperty = (JProperty)existing;

                if (newProperty.Name == existingProperty.Name)
                {
                    return;
                }
            }

            if (_properties.TryGetValue(newProperty.Name, out existing))
            {
                throw new ArgumentException("Can not add property {0} to {1}. Property with the same name already exists on object.".FormatWith(CultureInfo.InvariantCulture, newProperty.Name, GetType()));
            }
        }

        internal override void MergeItem(object content, JsonMergeSettings? settings)
        {
            if (!(content is JObject o))
            {
                return;
            }

            foreach (KeyValuePair<string, JToken?> contentItem in o)
            {
                JProperty? existingProperty = Property(contentItem.Key, settings?.PropertyNameComparison ?? StringComparison.Ordinal);

                if (existingProperty == null)
                {
                    Add(contentItem.Key, contentItem.Value);
                }
                else if (contentItem.Value != null)
                {
                    if (!(existingProperty.Value is JContainer existingContainer) || existingContainer.Type != contentItem.Value.Type)
                    {
                        if (!IsNull(contentItem.Value) || settings?.MergeNullValueHandling == MergeNullValueHandling.Merge)
                        {
                            existingProperty.Value = contentItem.Value;
                        }
                    }
                    else
                    {
                        existingContainer.Merge(contentItem.Value, settings);
                    }
                }
            }
        }

        private static bool IsNull(JToken token)
        {
            if (token.Type == JTokenType.Null)
            {
                return true;
            }

            if (token is JValue v && v.Value == null)
            {
                return true;
            }

            return false;
        }

        internal void InternalPropertyChanged(JProperty childProperty)
        {
            OnPropertyChanged(childProperty.Name);
#if HAVE_COMPONENT_MODEL
            if (_listChanged != null)
            {
                OnListChanged(new ListChangedEventArgs(ListChangedType.ItemChanged, IndexOfItem(childProperty)));
            }
#endif
#if HAVE_INOTIFY_COLLECTION_CHANGED
            if (_collectionChanged != null)
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, childProperty, childProperty, IndexOfItem(childProperty)));
            }
#endif
        }

        internal void InternalPropertyChanging(JProperty childProperty)
        {
#if HAVE_INOTIFY_PROPERTY_CHANGING
            OnPropertyChanging(childProperty.Name);
#endif
        }

        internal override JToken CloneToken()
        {
            return new JObject(this);
        }
        public override JTokenType Type => JTokenType.Object;
        public IEnumerable<JProperty> Properties()
        {
            return _properties.Cast<JProperty>();
        }
        public JProperty? Property(string name)
        {
            return Property(name, StringComparison.Ordinal);
        }
        public JProperty? Property(string name, StringComparison comparison)
        {
            if (name == null)
            {
                return null;
            }

            if (_properties.TryGetValue(name, out JToken? property))
            {
                return (JProperty)property;
            }
            if (comparison != StringComparison.Ordinal)
            {
                for (int i = 0; i < _properties.Count; i++)
                {
                    JProperty p = (JProperty)_properties[i];
                    if (string.Equals(p.Name, name, comparison))
                    {
                        return p;
                    }
                }
            }

            return null;
        }
        public JEnumerable<JToken> PropertyValues()
        {
            return new JEnumerable<JToken>(Properties().Select(p => p.Value));
        }
        public override JToken? this[object key]
        {
            get
            {
                ValidationUtils.ArgumentNotNull(key, nameof(key));

                if (!(key is string propertyName))
                {
                    throw new ArgumentException("Accessed JObject values with invalid key value: {0}. Object property name expected.".FormatWith(CultureInfo.InvariantCulture, MiscellaneousUtils.ToString(key)));
                }

                return this[propertyName];
            }
            set
            {
                ValidationUtils.ArgumentNotNull(key, nameof(key));

                if (!(key is string propertyName))
                {
                    throw new ArgumentException("Set JObject values with invalid key value: {0}. Object property name expected.".FormatWith(CultureInfo.InvariantCulture, MiscellaneousUtils.ToString(key)));
                }

                this[propertyName] = value;
            }
        }
        public JToken? this[string propertyName]
        {
            get
            {
                ValidationUtils.ArgumentNotNull(propertyName, nameof(propertyName));

                JProperty? property = Property(propertyName, StringComparison.Ordinal);

                return property?.Value;
            }
            set
            {
                JProperty? property = Property(propertyName, StringComparison.Ordinal);
                if (property != null)
                {
                    property.Value = value!;
                }
                else
                {
#if HAVE_INOTIFY_PROPERTY_CHANGING
                    OnPropertyChanging(propertyName);
#endif
                    Add(propertyName, value);
                    OnPropertyChanged(propertyName);
                }
            }
        }
        public new static JObject Load(JsonReader reader)
        {
            return Load(reader, null);
        }
        public new static JObject Load(JsonReader reader, JsonLoadSettings? settings)
        {
            ValidationUtils.ArgumentNotNull(reader, nameof(reader));

            if (reader.TokenType == JsonToken.None)
            {
                if (!reader.Read())
                {
                    throw JsonReaderException.Create(reader, "Error reading JObject from JsonReader.");
                }
            }

            reader.MoveToContent();

            if (reader.TokenType != JsonToken.StartObject)
            {
                throw JsonReaderException.Create(reader, "Error reading JObject from JsonReader. Current JsonReader item is not an object: {0}".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
            }

            JObject o = new JObject();
            o.SetLineInfo(reader as IJsonLineInfo, settings);

            o.ReadTokenFrom(reader, settings);

            return o;
        }
        public new static JObject Parse(string json)
        {
            return Parse(json, null);
        }
        public new static JObject Parse(string json, JsonLoadSettings? settings)
        {
            using (JsonReader reader = new JsonTextReader(new StringReader(json)))
            {
                JObject o = Load(reader, settings);

                while (reader.Read())
                {
                }

                return o;
            }
        }
        public new static JObject FromObject(object o)
        {
            return FromObject(o, JsonSerializer.CreateDefault());
        }
        public new static JObject FromObject(object o, JsonSerializer jsonSerializer)
        {
            JToken token = FromObjectInternal(o, jsonSerializer);

            if (token.Type != JTokenType.Object)
            {
                throw new ArgumentException("Object serialized to {0}. JObject instance expected.".FormatWith(CultureInfo.InvariantCulture, token.Type));
            }

            return (JObject)token;
        }
        public override void WriteTo(JsonWriter writer, params JsonConverter[] converters)
        {
            writer.WriteStartObject();

            for (int i = 0; i < _properties.Count; i++)
            {
                _properties[i].WriteTo(writer, converters);
            }

            writer.WriteEndObject();
        }
        public JToken? GetValue(string? propertyName)
        {
            return GetValue(propertyName, StringComparison.Ordinal);
        }
        public JToken? GetValue(string? propertyName, StringComparison comparison)
        {
            if (propertyName == null)
            {
                return null;
            }
            var property = Property(propertyName, comparison);

            return property?.Value;
        }
        public bool TryGetValue(string propertyName, StringComparison comparison, [NotNullWhen(true)]out JToken? value)
        {
            value = GetValue(propertyName, comparison);
            return (value != null);
        }

        #region IDictionary<string,JToken> Members
        public void Add(string propertyName, JToken? value)
        {
            Add(new JProperty(propertyName, value));
        }
        public bool ContainsKey(string propertyName)
        {
            ValidationUtils.ArgumentNotNull(propertyName, nameof(propertyName));

            return _properties.Contains(propertyName);
        }

        ICollection<string> IDictionary<string, JToken?>.Keys => _properties.Keys;
        public bool Remove(string propertyName)
        {
            JProperty? property = Property(propertyName, StringComparison.Ordinal);
            if (property == null)
            {
                return false;
            }

            property.Remove();
            return true;
        }
        public bool TryGetValue(string propertyName, [NotNullWhen(true)]out JToken? value)
        {
            JProperty? property = Property(propertyName, StringComparison.Ordinal);
            if (property == null)
            {
                value = null;
                return false;
            }

            value = property.Value;
            return true;
        }

        ICollection<JToken?> IDictionary<string, JToken?>.Values => throw new NotImplementedException();

        #endregion

        #region ICollection<KeyValuePair<string,JToken>> Members
        void ICollection<KeyValuePair<string, JToken?>>.Add(KeyValuePair<string, JToken?> item)
        {
            Add(new JProperty(item.Key, item.Value));
        }

        void ICollection<KeyValuePair<string, JToken?>>.Clear()
        {
            RemoveAll();
        }

        bool ICollection<KeyValuePair<string, JToken?>>.Contains(KeyValuePair<string, JToken?> item)
        {
            JProperty? property = Property(item.Key, StringComparison.Ordinal);
            if (property == null)
            {
                return false;
            }

            return (property.Value == item.Value);
        }

        void ICollection<KeyValuePair<string, JToken?>>.CopyTo(KeyValuePair<string, JToken?>[] array, int arrayIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }
            if (arrayIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(arrayIndex), "arrayIndex is less than 0.");
            }
            if (arrayIndex >= array.Length && arrayIndex != 0)
            {
                throw new ArgumentException("arrayIndex is equal to or greater than the length of array.");
            }
            if (Count > array.Length - arrayIndex)
            {
                throw new ArgumentException("The number of elements in the source JObject is greater than the available space from arrayIndex to the end of the destination array.");
            }

            int index = 0;
            foreach (JProperty property in _properties)
            {
                array[arrayIndex + index] = new KeyValuePair<string, JToken?>(property.Name, property.Value);
                index++;
            }
        }

        bool ICollection<KeyValuePair<string, JToken?>>.IsReadOnly => false;

        bool ICollection<KeyValuePair<string, JToken?>>.Remove(KeyValuePair<string, JToken?> item)
        {
            if (!((ICollection<KeyValuePair<string, JToken?>>)this).Contains(item))
            {
                return false;
            }

            ((IDictionary<string, JToken>)this).Remove(item.Key);
            return true;
        }
        #endregion

        internal override int GetDeepHashCode()
        {
            return ContentsHashCode();
        }
        public IEnumerator<KeyValuePair<string, JToken?>> GetEnumerator()
        {
            foreach (JProperty property in _properties)
            {
                yield return new KeyValuePair<string, JToken?>(property.Name, property.Value);
            }
        }
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

#if HAVE_INOTIFY_PROPERTY_CHANGING
        protected virtual void OnPropertyChanging(string propertyName)
        {
            PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
        }
#endif

#if HAVE_COMPONENT_MODEL

        #region ICustomTypeDescriptor
        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
        {
            return ((ICustomTypeDescriptor)this).GetProperties(null);
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
        {
            PropertyDescriptor[] propertiesArray = new PropertyDescriptor[Count];
            int i = 0;
            foreach (KeyValuePair<string, JToken?> propertyValue in this)
            {
                propertiesArray[i] = new JPropertyDescriptor(propertyValue.Key);
                i++;
            }

            return new PropertyDescriptorCollection(propertiesArray);
        }

        AttributeCollection ICustomTypeDescriptor.GetAttributes()
        {
            return AttributeCollection.Empty;
        }

        string? ICustomTypeDescriptor.GetClassName()
        {
            return null;
        }

        string? ICustomTypeDescriptor.GetComponentName()
        {
            return null;
        }

        TypeConverter ICustomTypeDescriptor.GetConverter()
        {
            return new TypeConverter();
        }

        EventDescriptor? ICustomTypeDescriptor.GetDefaultEvent()
        {
            return null;
        }

        PropertyDescriptor? ICustomTypeDescriptor.GetDefaultProperty()
        {
            return null;
        }

        object? ICustomTypeDescriptor.GetEditor(Type editorBaseType)
        {
            return null;
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
        {
            return EventDescriptorCollection.Empty;
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
        {
            return EventDescriptorCollection.Empty;
        }

        object? ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
        {
            if (pd is JPropertyDescriptor)
            {
                return this;
            }

            return null;
        }
        #endregion

#endif

#if HAVE_DYNAMIC                            
        protected override DynamicMetaObject GetMetaObject(Expression parameter)
        {
            return new DynamicProxyMetaObject<JObject>(parameter, this, new JObjectDynamicProxy());
        }

        private class JObjectDynamicProxy : DynamicProxy<JObject>
        {
            public override bool TryGetMember(JObject instance, GetMemberBinder binder, out object? result)
            {
                result = instance[binder.Name];
                return true;
            }

            public override bool TrySetMember(JObject instance, SetMemberBinder binder, object value)
            {
                if (!(value is JToken v))
                {
                    v = new JValue(value);
                }

                instance[binder.Name] = v;
                return true;
            }

            public override IEnumerable<string> GetDynamicMemberNames(JObject instance)
            {
                return instance.Properties().Select(p => p.Name);
            }
        }
#endif
    }
}
