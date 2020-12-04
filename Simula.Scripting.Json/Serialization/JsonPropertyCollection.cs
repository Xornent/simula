
using Simula.Scripting.Json.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Simula.Scripting.Json.Serialization
{
    public class JsonPropertyCollection : KeyedCollection<string, JsonProperty>
    {
        private readonly Type _type;
        private readonly List<JsonProperty> _list;
        public JsonPropertyCollection(Type type)
            : base(StringComparer.Ordinal)
        {
            ValidationUtils.ArgumentNotNull(type, "type");
            _type = type;
            _list = (List<JsonProperty>)Items;
        }
        protected override string GetKeyForItem(JsonProperty item)
        {
            return item.PropertyName!;
        }
        public void AddProperty(JsonProperty property)
        {
            MiscellaneousUtils.Assert(property.PropertyName != null);

            if (Contains(property.PropertyName)) {
                if (property.Ignored) {
                    return;
                }

                JsonProperty existingProperty = this[property.PropertyName];
                bool duplicateProperty = true;

                if (existingProperty.Ignored) {
                    Remove(existingProperty);
                    duplicateProperty = false;
                } else {
                    if (property.DeclaringType != null && existingProperty.DeclaringType != null) {
                        if (property.DeclaringType.IsSubclassOf(existingProperty.DeclaringType)
                            || (existingProperty.DeclaringType.IsInterface() && property.DeclaringType.ImplementInterface(existingProperty.DeclaringType))) {
                            Remove(existingProperty);
                            duplicateProperty = false;
                        }
                        if (existingProperty.DeclaringType.IsSubclassOf(property.DeclaringType)
                            || (property.DeclaringType.IsInterface() && existingProperty.DeclaringType.ImplementInterface(property.DeclaringType))) {
                            return;
                        }

                        if (_type.ImplementInterface(existingProperty.DeclaringType) && _type.ImplementInterface(property.DeclaringType)) {
                            return;
                        }
                    }
                }

                if (duplicateProperty) {
                    throw new JsonSerializationException("A member with the name '{0}' already exists on '{1}'. Use the JsonPropertyAttribute to specify another name.".FormatWith(CultureInfo.InvariantCulture, property.PropertyName, _type));
                }
            }

            Add(property);
        }
        public JsonProperty? GetClosestMatchProperty(string propertyName)
        {
            JsonProperty? property = GetProperty(propertyName, StringComparison.Ordinal);
            if (property == null) {
                property = GetProperty(propertyName, StringComparison.OrdinalIgnoreCase);
            }

            return property;
        }

        private bool TryGetValue(string key, [NotNullWhen(true)] out JsonProperty? item)
        {
            if (Dictionary == null) {
                item = default;
                return false;
            }

            return Dictionary.TryGetValue(key, out item);
        }
        public JsonProperty? GetProperty(string propertyName, StringComparison comparisonType)
        {
            if (comparisonType == StringComparison.Ordinal) {
                if (TryGetValue(propertyName, out JsonProperty? property)) {
                    return property;
                }

                return null;
            }

            for (int i = 0; i < _list.Count; i++) {
                JsonProperty property = _list[i];
                if (string.Equals(propertyName, property.PropertyName, comparisonType)) {
                    return property;
                }
            }

            return null;
        }
    }
}
