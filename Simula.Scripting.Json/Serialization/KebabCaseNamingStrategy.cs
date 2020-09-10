
using Simula.Scripting.Json.Utilities;

namespace Simula.Scripting.Json.Serialization
{
    public class KebabCaseNamingStrategy : NamingStrategy
    {
        public KebabCaseNamingStrategy(bool processDictionaryKeys, bool overrideSpecifiedNames)
        {
            ProcessDictionaryKeys = processDictionaryKeys;
            OverrideSpecifiedNames = overrideSpecifiedNames;
        }
        public KebabCaseNamingStrategy(bool processDictionaryKeys, bool overrideSpecifiedNames, bool processExtensionDataNames)
            : this(processDictionaryKeys, overrideSpecifiedNames)
        {
            ProcessExtensionDataNames = processExtensionDataNames;
        }
        public KebabCaseNamingStrategy()
        {
        }
        protected override string ResolvePropertyName(string name) => StringUtils.ToKebabCase(name);
    }
}