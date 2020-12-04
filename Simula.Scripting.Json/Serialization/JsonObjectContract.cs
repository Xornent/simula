
using Simula.Scripting.Json.Linq;
using System;

namespace Simula.Scripting.Json.Serialization
{
    public class JsonObjectContract : JsonContainerContract
    {
        public MemberSerialization MemberSerialization { get; set; }
        public MissingMemberHandling? MissingMemberHandling { get; set; }
        public Required? ItemRequired { get; set; }
        public NullValueHandling? ItemNullValueHandling { get; set; }
        public JsonPropertyCollection Properties { get; }
        public JsonPropertyCollection CreatorParameters {
            get {
                if (_creatorParameters == null) {
                    _creatorParameters = new JsonPropertyCollection(UnderlyingType);
                }

                return _creatorParameters;
            }
        }
        public ObjectConstructor<object>? OverrideCreator {
            get => _overrideCreator;
            set => _overrideCreator = value;
        }

        internal ObjectConstructor<object>? ParameterizedCreator {
            get => _parameterizedCreator;
            set => _parameterizedCreator = value;
        }
        public ExtensionDataSetter? ExtensionDataSetter { get; set; }
        public ExtensionDataGetter? ExtensionDataGetter { get; set; }
        public Type? ExtensionDataValueType {
            get => _extensionDataValueType;
            set {
                _extensionDataValueType = value;
                ExtensionDataIsJToken = (value != null && typeof(JToken).IsAssignableFrom(value));
            }
        }
        public Func<string, string>? ExtensionDataNameResolver { get; set; }

        internal bool ExtensionDataIsJToken;
        private bool? _hasRequiredOrDefaultValueProperties;
        private ObjectConstructor<object>? _overrideCreator;
        private ObjectConstructor<object>? _parameterizedCreator;
        private JsonPropertyCollection? _creatorParameters;
        private Type? _extensionDataValueType;

        internal bool HasRequiredOrDefaultValueProperties {
            get {
                if (_hasRequiredOrDefaultValueProperties == null) {
                    _hasRequiredOrDefaultValueProperties = false;

                    if (ItemRequired.GetValueOrDefault(Required.Default) != Required.Default) {
                        _hasRequiredOrDefaultValueProperties = true;
                    } else {
                        foreach (JsonProperty property in Properties) {
                            if (property.Required != Required.Default || (property.DefaultValueHandling & DefaultValueHandling.Populate) == DefaultValueHandling.Populate) {
                                _hasRequiredOrDefaultValueProperties = true;
                                break;
                            }
                        }
                    }
                }

                return _hasRequiredOrDefaultValueProperties.GetValueOrDefault();
            }
        }
        public JsonObjectContract(Type underlyingType)
            : base(underlyingType)
        {
            ContractType = JsonContractType.Object;

            Properties = new JsonPropertyCollection(UnderlyingType);
        }

#if HAVE_BINARY_FORMATTER
#if HAVE_SECURITY_SAFE_CRITICAL_ATTRIBUTE
        [SecuritySafeCritical]
#endif
        internal object GetUninitializedObject()
        {
            if (!JsonTypeReflector.FullyTrusted)
            {
                throw new JsonException("Insufficient permissions. Creating an uninitialized '{0}' type requires full trust.".FormatWith(CultureInfo.InvariantCulture, NonNullableUnderlyingType));
            }

            return FormatterServices.GetUninitializedObject(NonNullableUnderlyingType);
        }
#endif
    }
}