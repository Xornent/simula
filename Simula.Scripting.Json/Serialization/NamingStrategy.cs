
namespace Simula.Scripting.Json.Serialization
{
    public abstract class NamingStrategy
    {
        public bool ProcessDictionaryKeys { get; set; }
        public bool ProcessExtensionDataNames { get; set; }
        public bool OverrideSpecifiedNames { get; set; }
        public virtual string GetPropertyName(string name, bool hasSpecifiedName)
        {
            if (hasSpecifiedName && !OverrideSpecifiedNames) {
                return name;
            }

            return ResolvePropertyName(name);
        }
        public virtual string GetExtensionDataName(string name)
        {
            if (!ProcessExtensionDataNames) {
                return name;
            }

            return ResolvePropertyName(name);
        }
        public virtual string GetDictionaryKey(string key)
        {
            if (!ProcessDictionaryKeys) {
                return key;
            }

            return ResolvePropertyName(key);
        }
        protected abstract string ResolvePropertyName(string name);
        public override int GetHashCode()
        {
            unchecked {
                var hashCode = GetType().GetHashCode();     // make sure different types do not result in equal values
                hashCode = (hashCode * 397) ^ ProcessDictionaryKeys.GetHashCode();
                hashCode = (hashCode * 397) ^ ProcessExtensionDataNames.GetHashCode();
                hashCode = (hashCode * 397) ^ OverrideSpecifiedNames.GetHashCode();
                return hashCode;
            }
        }
        public override bool Equals(object obj) => Equals(obj as NamingStrategy);
        protected bool Equals(NamingStrategy? other)
        {
            if (other == null) {
                return false;
            }

            return GetType() == other.GetType() &&
                ProcessDictionaryKeys == other.ProcessDictionaryKeys &&
                ProcessExtensionDataNames == other.ProcessExtensionDataNames &&
                OverrideSpecifiedNames == other.OverrideSpecifiedNames;
        }
    }
}