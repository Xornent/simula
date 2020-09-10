
using System;
using System.Globalization;
using System.ComponentModel;
using System.Collections.Generic;
using Simula.Scripting.Json.Linq;
using Simula.Scripting.Json.Utilities;
using Simula.Scripting.Json.Serialization;
#if !HAVE_LINQ
using Simula.Scripting.Json.Utilities.LinqBridge;
#else
using System.Linq;

#endif

#nullable disable

namespace Simula.Scripting.Json.Schema
{
    [Obsolete("JSON Schema validation has been moved to its own package. See https://www.newtonsoft.com/jsonschema for more details.")]
    public class JsonSchemaGenerator
    {
        public UndefinedSchemaIdHandling UndefinedSchemaIdHandling { get; set; }

        private IContractResolver _contractResolver;
        public IContractResolver ContractResolver
        {
            get
            {
                if (_contractResolver == null)
                {
                    return DefaultContractResolver.Instance;
                }

                return _contractResolver;
            }
            set => _contractResolver = value;
        }

        private class TypeSchema
        {
            public Type Type { get; }
            public JsonSchema Schema { get; }

            public TypeSchema(Type type, JsonSchema schema)
            {
                ValidationUtils.ArgumentNotNull(type, nameof(type));
                ValidationUtils.ArgumentNotNull(schema, nameof(schema));

                Type = type;
                Schema = schema;
            }
        }

        private JsonSchemaResolver _resolver;
        private readonly IList<TypeSchema> _stack = new List<TypeSchema>();
        private JsonSchema _currentSchema;

        private JsonSchema CurrentSchema => _currentSchema;

        private void Push(TypeSchema typeSchema)
        {
            _currentSchema = typeSchema.Schema;
            _stack.Add(typeSchema);
            _resolver.LoadedSchemas.Add(typeSchema.Schema);
        }

        private TypeSchema Pop()
        {
            TypeSchema popped = _stack[_stack.Count - 1];
            _stack.RemoveAt(_stack.Count - 1);
            TypeSchema newValue = _stack.LastOrDefault();
            if (newValue != null)
            {
                _currentSchema = newValue.Schema;
            }
            else
            {
                _currentSchema = null;
            }

            return popped;
        }
        public JsonSchema Generate(Type type)
        {
            return Generate(type, new JsonSchemaResolver(), false);
        }
        public JsonSchema Generate(Type type, JsonSchemaResolver resolver)
        {
            return Generate(type, resolver, false);
        }
        public JsonSchema Generate(Type type, bool rootSchemaNullable)
        {
            return Generate(type, new JsonSchemaResolver(), rootSchemaNullable);
        }
        public JsonSchema Generate(Type type, JsonSchemaResolver resolver, bool rootSchemaNullable)
        {
            ValidationUtils.ArgumentNotNull(type, nameof(type));
            ValidationUtils.ArgumentNotNull(resolver, nameof(resolver));

            _resolver = resolver;

            return GenerateInternal(type, (!rootSchemaNullable) ? Required.Always : Required.Default, false);
        }

        private string GetTitle(Type type)
        {
            JsonContainerAttribute containerAttribute = JsonTypeReflector.GetCachedAttribute<JsonContainerAttribute>(type);

            if (!StringUtils.IsNullOrEmpty(containerAttribute?.Title))
            {
                return containerAttribute.Title;
            }

            return null;
        }

        private string GetDescription(Type type)
        {
            JsonContainerAttribute containerAttribute = JsonTypeReflector.GetCachedAttribute<JsonContainerAttribute>(type);

            if (!StringUtils.IsNullOrEmpty(containerAttribute?.Description))
            {
                return containerAttribute.Description;
            }

#if HAVE_ADO_NET
            DescriptionAttribute descriptionAttribute = ReflectionUtils.GetAttribute<DescriptionAttribute>(type);
            return descriptionAttribute?.Description;
#else
            return null;
#endif
        }

        private string GetTypeId(Type type, bool explicitOnly)
        {
            JsonContainerAttribute containerAttribute = JsonTypeReflector.GetCachedAttribute<JsonContainerAttribute>(type);

            if (!StringUtils.IsNullOrEmpty(containerAttribute?.Id))
            {
                return containerAttribute.Id;
            }

            if (explicitOnly)
            {
                return null;
            }

            switch (UndefinedSchemaIdHandling)
            {
                case UndefinedSchemaIdHandling.UseTypeName:
                    return type.FullName;
                case UndefinedSchemaIdHandling.UseAssemblyQualifiedName:
                    return type.AssemblyQualifiedName;
                default:
                    return null;
            }
        }

        private JsonSchema GenerateInternal(Type type, Required valueRequired, bool required)
        {
            ValidationUtils.ArgumentNotNull(type, nameof(type));

            string resolvedId = GetTypeId(type, false);
            string explicitId = GetTypeId(type, true);

            if (!StringUtils.IsNullOrEmpty(resolvedId))
            {
                JsonSchema resolvedSchema = _resolver.GetSchema(resolvedId);
                if (resolvedSchema != null)
                {
                    if (valueRequired != Required.Always && !HasFlag(resolvedSchema.Type, JsonSchemaType.Null))
                    {
                        resolvedSchema.Type |= JsonSchemaType.Null;
                    }
                    if (required && resolvedSchema.Required != true)
                    {
                        resolvedSchema.Required = true;
                    }

                    return resolvedSchema;
                }
            }
            if (_stack.Any(tc => tc.Type == type))
            {
                throw new JsonException("Unresolved circular reference for type '{0}'. Explicitly define an Id for the type using a JsonObject/JsonArray attribute or automatically generate a type Id using the UndefinedSchemaIdHandling property.".FormatWith(CultureInfo.InvariantCulture, type));
            }

            JsonContract contract = ContractResolver.ResolveContract(type);
            JsonConverter converter = contract.Converter ?? contract.InternalConverter;

            Push(new TypeSchema(type, new JsonSchema()));

            if (explicitId != null)
            {
                CurrentSchema.Id = explicitId;
            }

            if (required)
            {
                CurrentSchema.Required = true;
            }
            CurrentSchema.Title = GetTitle(type);
            CurrentSchema.Description = GetDescription(type);

            if (converter != null)
            {
                CurrentSchema.Type = JsonSchemaType.Any;
            }
            else
            {
                switch (contract.ContractType)
                {
                    case JsonContractType.Object:
                        CurrentSchema.Type = AddNullType(JsonSchemaType.Object, valueRequired);
                        CurrentSchema.Id = GetTypeId(type, false);
                        GenerateObjectSchema(type, (JsonObjectContract)contract);
                        break;
                    case JsonContractType.Array:
                        CurrentSchema.Type = AddNullType(JsonSchemaType.Array, valueRequired);

                        CurrentSchema.Id = GetTypeId(type, false);

                        JsonArrayAttribute arrayAttribute = JsonTypeReflector.GetCachedAttribute<JsonArrayAttribute>(type);
                        bool allowNullItem = (arrayAttribute == null || arrayAttribute.AllowNullItems);

                        Type collectionItemType = ReflectionUtils.GetCollectionItemType(type);
                        if (collectionItemType != null)
                        {
                            CurrentSchema.Items = new List<JsonSchema>();
                            CurrentSchema.Items.Add(GenerateInternal(collectionItemType, (!allowNullItem) ? Required.Always : Required.Default, false));
                        }
                        break;
                    case JsonContractType.Primitive:
                        CurrentSchema.Type = GetJsonSchemaType(type, valueRequired);

                        if (CurrentSchema.Type == JsonSchemaType.Integer && type.IsEnum() && !type.IsDefined(typeof(FlagsAttribute), true))
                        {
                            CurrentSchema.Enum = new List<JToken>();

                            EnumInfo enumValues = EnumUtils.GetEnumValuesAndNames(type);
                            for (int i = 0; i < enumValues.Names.Length; i++)
                            {
                                ulong v = enumValues.Values[i];
                                JToken value = JToken.FromObject(Enum.ToObject(type, v));

                                CurrentSchema.Enum.Add(value);
                            }
                        }
                        break;
                    case JsonContractType.String:
                        JsonSchemaType schemaType = (!ReflectionUtils.IsNullable(contract.UnderlyingType))
                            ? JsonSchemaType.String
                            : AddNullType(JsonSchemaType.String, valueRequired);

                        CurrentSchema.Type = schemaType;
                        break;
                    case JsonContractType.Dictionary:
                        CurrentSchema.Type = AddNullType(JsonSchemaType.Object, valueRequired);

                        Type keyType;
                        Type valueType;
                        ReflectionUtils.GetDictionaryKeyValueTypes(type, out keyType, out valueType);

                        if (keyType != null)
                        {
                            JsonContract keyContract = ContractResolver.ResolveContract(keyType);
                            if (keyContract.ContractType == JsonContractType.Primitive)
                            {
                                CurrentSchema.AdditionalProperties = GenerateInternal(valueType, Required.Default, false);
                            }
                        }
                        break;
#if HAVE_BINARY_SERIALIZATION
                    case JsonContractType.Serializable:
                        CurrentSchema.Type = AddNullType(JsonSchemaType.Object, valueRequired);
                        CurrentSchema.Id = GetTypeId(type, false);
                        GenerateISerializableContract(type, (JsonISerializableContract)contract);
                        break;
#endif
#if HAVE_DYNAMIC
                    case JsonContractType.Dynamic:
#endif
                    case JsonContractType.Linq:
                        CurrentSchema.Type = JsonSchemaType.Any;
                        break;
                    default:
                        throw new JsonException("Unexpected contract type: {0}".FormatWith(CultureInfo.InvariantCulture, contract));
                }
            }

            return Pop().Schema;
        }

        private JsonSchemaType AddNullType(JsonSchemaType type, Required valueRequired)
        {
            if (valueRequired != Required.Always)
            {
                return type | JsonSchemaType.Null;
            }

            return type;
        }

        private bool HasFlag(DefaultValueHandling value, DefaultValueHandling flag)
        {
            return ((value & flag) == flag);
        }

        private void GenerateObjectSchema(Type type, JsonObjectContract contract)
        {
            CurrentSchema.Properties = new Dictionary<string, JsonSchema>();
            foreach (JsonProperty property in contract.Properties)
            {
                if (!property.Ignored)
                {
                    bool optional = property.NullValueHandling == NullValueHandling.Ignore ||
                                    HasFlag(property.DefaultValueHandling.GetValueOrDefault(), DefaultValueHandling.Ignore) ||
                                    property.ShouldSerialize != null ||
                                    property.GetIsSpecified != null;

                    JsonSchema propertySchema = GenerateInternal(property.PropertyType, property.Required, !optional);

                    if (property.DefaultValue != null)
                    {
                        propertySchema.Default = JToken.FromObject(property.DefaultValue);
                    }

                    CurrentSchema.Properties.Add(property.PropertyName, propertySchema);
                }
            }

            if (type.IsSealed())
            {
                CurrentSchema.AllowAdditionalProperties = false;
            }
        }

#if HAVE_BINARY_SERIALIZATION
        private void GenerateISerializableContract(Type type, JsonISerializableContract contract)
        {
            CurrentSchema.AllowAdditionalProperties = true;
        }
#endif

        internal static bool HasFlag(JsonSchemaType? value, JsonSchemaType flag)
        {
            if (value == null)
            {
                return true;
            }

            bool match = ((value & flag) == flag);
            if (match)
            {
                return true;
            }
            if (flag == JsonSchemaType.Integer && (value & JsonSchemaType.Float) == JsonSchemaType.Float)
            {
                return true;
            }

            return false;
        }

        private JsonSchemaType GetJsonSchemaType(Type type, Required valueRequired)
        {
            JsonSchemaType schemaType = JsonSchemaType.None;
            if (valueRequired != Required.Always && ReflectionUtils.IsNullable(type))
            {
                schemaType = JsonSchemaType.Null;
                if (ReflectionUtils.IsNullableType(type))
                {
                    type = Nullable.GetUnderlyingType(type);
                }
            }

            PrimitiveTypeCode typeCode = ConvertUtils.GetTypeCode(type);

            switch (typeCode)
            {
                case PrimitiveTypeCode.Empty:
                case PrimitiveTypeCode.Object:
                    return schemaType | JsonSchemaType.String;
#if HAVE_DB_NULL_TYPE_CODE
                case PrimitiveTypeCode.DBNull:
                    return schemaType | JsonSchemaType.Null;
#endif
                case PrimitiveTypeCode.Boolean:
                    return schemaType | JsonSchemaType.Boolean;
                case PrimitiveTypeCode.Char:
                    return schemaType | JsonSchemaType.String;
                case PrimitiveTypeCode.SByte:
                case PrimitiveTypeCode.Byte:
                case PrimitiveTypeCode.Int16:
                case PrimitiveTypeCode.UInt16:
                case PrimitiveTypeCode.Int32:
                case PrimitiveTypeCode.UInt32:
                case PrimitiveTypeCode.Int64:
                case PrimitiveTypeCode.UInt64:
#if HAVE_BIG_INTEGER
                case PrimitiveTypeCode.BigInteger:
#endif
                    return schemaType | JsonSchemaType.Integer;
                case PrimitiveTypeCode.Single:
                case PrimitiveTypeCode.Double:
                case PrimitiveTypeCode.Decimal:
                    return schemaType | JsonSchemaType.Float;
                case PrimitiveTypeCode.DateTime:
#if HAVE_DATE_TIME_OFFSET
                case PrimitiveTypeCode.DateTimeOffset:
#endif
                    return schemaType | JsonSchemaType.String;
                case PrimitiveTypeCode.String:
                case PrimitiveTypeCode.Uri:
                case PrimitiveTypeCode.Guid:
                case PrimitiveTypeCode.TimeSpan:
                case PrimitiveTypeCode.Bytes:
                    return schemaType | JsonSchemaType.String;
                default:
                    throw new JsonException("Unexpected type code '{0}' for type '{1}'.".FormatWith(CultureInfo.InvariantCulture, typeCode, type));
            }
        }
    }
}