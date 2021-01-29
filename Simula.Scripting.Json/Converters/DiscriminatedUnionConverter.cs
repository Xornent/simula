
#if HAVE_FSHARP_TYPES
using Simula.Scripting.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
#if !HAVE_LINQ
using Simula.Scripting.Json.Utilities.LinqBridge;
#else
using System.Linq;
#endif
using System.Reflection;
using Simula.Scripting.Json.Serialization;
using System.Globalization;
using Simula.Scripting.Json.Utilities;

namespace Simula.Scripting.Json.Converters
{
    public class DiscriminatedUnionConverter : JsonConverter
    {
        #region UnionDefinition
        internal class Union
        {
            public readonly FSharpFunction TagReader;
            public readonly List<UnionCase> Cases;

            public Union(FSharpFunction tagReader, List<UnionCase> cases)
            {
                TagReader = tagReader;
                Cases = cases;
            }
        }

        internal class UnionCase
        {
            public readonly int Tag;
            public readonly string Name;
            public readonly PropertyInfo[] Fields;
            public readonly FSharpFunction FieldReader;
            public readonly FSharpFunction Constructor;

            public UnionCase(int tag, string name, PropertyInfo[] fields, FSharpFunction fieldReader, FSharpFunction constructor)
            {
                Tag = tag;
                Name = name;
                Fields = fields;
                FieldReader = fieldReader;
                Constructor = constructor;
            }
        }
        #endregion

        private const string CasePropertyName = "Case";
        private const string FieldsPropertyName = "Fields";

        private static readonly ThreadSafeStore<Type, Union> UnionCache = new ThreadSafeStore<Type, Union>(CreateUnion);
        private static readonly ThreadSafeStore<Type, Type> UnionTypeLookupCache = new ThreadSafeStore<Type, Type>(CreateUnionTypeLookup);

        private static Type CreateUnionTypeLookup(Type t)
        {
            object[] cases = (object[])FSharpUtils.Instance.GetUnionCases(null, t, null)!;

            object caseInfo = cases.First();

            Type unionType = (Type)FSharpUtils.Instance.GetUnionCaseInfoDeclaringType(caseInfo)!;
            return unionType;
        }

        private static Union CreateUnion(Type t)
        {
            Union u = new Union((FSharpFunction)FSharpUtils.Instance.PreComputeUnionTagReader(null, t, null), new List<UnionCase>());

            object[] cases = (object[])FSharpUtils.Instance.GetUnionCases(null, t, null)!;

            foreach (object unionCaseInfo in cases) {
                UnionCase unionCase = new UnionCase(
                    (int)FSharpUtils.Instance.GetUnionCaseInfoTag(unionCaseInfo),
                    (string)FSharpUtils.Instance.GetUnionCaseInfoName(unionCaseInfo),
                    (PropertyInfo[])FSharpUtils.Instance.GetUnionCaseInfoFields(unionCaseInfo)!,
                    (FSharpFunction)FSharpUtils.Instance.PreComputeUnionReader(null, unionCaseInfo, null),
                    (FSharpFunction)FSharpUtils.Instance.PreComputeUnionConstructor(null, unionCaseInfo, null));

                u.Cases.Add(unionCase);
            }

            return u;
        }
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value == null) {
                writer.WriteNull();
                return;
            }

            DefaultContractResolver? resolver = serializer.ContractResolver as DefaultContractResolver;

            Type unionType = UnionTypeLookupCache.Get(value.GetType());
            Union union = UnionCache.Get(unionType);

            int tag = (int)union.TagReader.Invoke(value);
            UnionCase caseInfo = union.Cases.Single(c => c.Tag == tag);

            writer.WriteStartObject();
            writer.WritePropertyName((resolver != null) ? resolver.GetResolvedPropertyName(CasePropertyName) : CasePropertyName);
            writer.WriteValue(caseInfo.Name);
            if (caseInfo.Fields != null && caseInfo.Fields.Length > 0) {
                object[] fields = (object[])caseInfo.FieldReader.Invoke(value)!;

                writer.WritePropertyName((resolver != null) ? resolver.GetResolvedPropertyName(FieldsPropertyName) : FieldsPropertyName);
                writer.WriteStartArray();
                foreach (object field in fields) {
                    serializer.Serialize(writer, field);
                }
                writer.WriteEndArray();
            }
            writer.WriteEndObject();
        }
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) {
                return null;
            }

            UnionCase? caseInfo = null;
            string? caseName = null;
            JArray? fields = null;
            reader.ReadAndAssert();

            while (reader.TokenType == JsonToken.PropertyName) {
                string propertyName = reader.Value!.ToString();
                if (string.Equals(propertyName, CasePropertyName, StringComparison.OrdinalIgnoreCase)) {
                    reader.ReadAndAssert();

                    Union union = UnionCache.Get(objectType);

                    caseName = reader.Value!.ToString();

                    caseInfo = union.Cases.SingleOrDefault(c => c.Name == caseName);

                    if (caseInfo == null) {
                        throw JsonSerializationException.Create(reader, "No union type found with the name '{0}'.".FormatWith(CultureInfo.InvariantCulture, caseName));
                    }
                } else if (string.Equals(propertyName, FieldsPropertyName, StringComparison.OrdinalIgnoreCase)) {
                    reader.ReadAndAssert();
                    if (reader.TokenType != JsonToken.StartArray) {
                        throw JsonSerializationException.Create(reader, "Union fields must been an array.");
                    }

                    fields = (JArray)JToken.ReadFrom(reader);
                } else {
                    throw JsonSerializationException.Create(reader, "Unexpected property '{0}' found when reading union.".FormatWith(CultureInfo.InvariantCulture, propertyName));
                }

                reader.ReadAndAssert();
            }

            if (caseInfo == null) {
                throw JsonSerializationException.Create(reader, "No '{0}' property with union name found.".FormatWith(CultureInfo.InvariantCulture, CasePropertyName));
            }

            object?[] typedFieldValues = new object?[caseInfo.Fields.Length];

            if (caseInfo.Fields.Length > 0 && fields == null) {
                throw JsonSerializationException.Create(reader, "No '{0}' property with union fields found.".FormatWith(CultureInfo.InvariantCulture, FieldsPropertyName));
            }

            if (fields != null) {
                if (caseInfo.Fields.Length != fields.Count) {
                    throw JsonSerializationException.Create(reader, "The number of field values does not match the number of properties defined by union '{0}'.".FormatWith(CultureInfo.InvariantCulture, caseName));
                }

                for (int i = 0; i < fields.Count; i++) {
                    JToken t = fields[i];
                    PropertyInfo fieldProperty = caseInfo.Fields[i];

                    typedFieldValues[i] = t.ToObject(fieldProperty.PropertyType, serializer);
                }
            }

            object[] args = { typedFieldValues };

            return caseInfo.Constructor.Invoke(args);
        }
        public override bool CanConvert(Type objectType)
        {
            if (typeof(IEnumerable).IsAssignableFrom(objectType)) {
                return false;
            }
            object[] attributes;
#if HAVE_FULL_REFLECTION
            attributes = objectType.GetCustomAttributes(true);
#else
            attributes = objectType.GetTypeInfo().GetCustomAttributes(true).ToArray();
#endif

            bool isFSharpType = false;
            foreach (object attribute in attributes) {
                Type attributeType = attribute.GetType();
                if (attributeType.FullName == "Microsoft.FSharp.Core.CompilationMappingAttribute") {
                    FSharpUtils.EnsureInitialized(attributeType.Assembly());

                    isFSharpType = true;
                    break;
                }
            }

            if (!isFSharpType) {
                return false;
            }

            return (bool)FSharpUtils.Instance.IsUnion(null, objectType, null);
        }
    }
}

#endif