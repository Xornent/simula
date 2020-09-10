
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters;
using Simula.Scripting.Json.Converters;
using Simula.Scripting.Json.Serialization;
using Simula.Scripting.Json.Utilities;
using System.Runtime.Serialization;
using ErrorEventArgs = Simula.Scripting.Json.Serialization.ErrorEventArgs;
using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;

namespace Simula.Scripting.Json
{
    public class JsonSerializer
    {
        internal TypeNameHandling _typeNameHandling;
        internal TypeNameAssemblyFormatHandling _typeNameAssemblyFormatHandling;
        internal PreserveReferencesHandling _preserveReferencesHandling;
        internal ReferenceLoopHandling _referenceLoopHandling;
        internal MissingMemberHandling _missingMemberHandling;
        internal ObjectCreationHandling _objectCreationHandling;
        internal NullValueHandling _nullValueHandling;
        internal DefaultValueHandling _defaultValueHandling;
        internal ConstructorHandling _constructorHandling;
        internal MetadataPropertyHandling _metadataPropertyHandling;
        internal JsonConverterCollection? _converters;
        internal IContractResolver _contractResolver;
        internal ITraceWriter? _traceWriter;
        internal IEqualityComparer? _equalityComparer;
        internal ISerializationBinder _serializationBinder;
        internal StreamingContext _context;
        private IReferenceResolver? _referenceResolver;

        private Formatting? _formatting;
        private DateFormatHandling? _dateFormatHandling;
        private DateTimeZoneHandling? _dateTimeZoneHandling;
        private DateParseHandling? _dateParseHandling;
        private FloatFormatHandling? _floatFormatHandling;
        private FloatParseHandling? _floatParseHandling;
        private StringEscapeHandling? _stringEscapeHandling;
        private CultureInfo _culture;
        private int? _maxDepth;
        private bool _maxDepthSet;
        private bool? _checkAdditionalContent;
        private string? _dateFormatString;
        private bool _dateFormatStringSet;
        public virtual event EventHandler<ErrorEventArgs>? Error;
        public virtual IReferenceResolver? ReferenceResolver
        {
            get => GetReferenceResolver();
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value), "Reference resolver cannot be null.");
                }

                _referenceResolver = value;
            }
        }
        [Obsolete("Binder is obsolete. Use SerializationBinder instead.")]
        public virtual SerializationBinder Binder
        {
            get
            {
                if (_serializationBinder is SerializationBinder legacySerializationBinder)
                {
                    return legacySerializationBinder;
                }

                if (_serializationBinder is SerializationBinderAdapter adapter)
                {
                    return adapter.SerializationBinder;
                }

                throw new InvalidOperationException("Cannot get SerializationBinder because an ISerializationBinder was previously set.");
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value), "Serialization binder cannot be null.");
                }

                _serializationBinder = value as ISerializationBinder ?? new SerializationBinderAdapter(value);
            }
        }
        public virtual ISerializationBinder SerializationBinder
        {
            get => _serializationBinder;
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value), "Serialization binder cannot be null.");
                }

                _serializationBinder = value;
            }
        }
        public virtual ITraceWriter? TraceWriter
        {
            get => _traceWriter;
            set => _traceWriter = value;
        }
        public virtual IEqualityComparer? EqualityComparer
        {
            get => _equalityComparer;
            set => _equalityComparer = value;
        }
        public virtual TypeNameHandling TypeNameHandling
        {
            get => _typeNameHandling;
            set
            {
                if (value < TypeNameHandling.None || value > TypeNameHandling.Auto)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _typeNameHandling = value;
            }
        }
        [Obsolete("TypeNameAssemblyFormat is obsolete. Use TypeNameAssemblyFormatHandling instead.")]
        public virtual FormatterAssemblyStyle TypeNameAssemblyFormat
        {
            get => (FormatterAssemblyStyle)_typeNameAssemblyFormatHandling;
            set
            {
                if (value < FormatterAssemblyStyle.Simple || value > FormatterAssemblyStyle.Full)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _typeNameAssemblyFormatHandling = (TypeNameAssemblyFormatHandling)value;
            }
        }
        public virtual TypeNameAssemblyFormatHandling TypeNameAssemblyFormatHandling
        {
            get => _typeNameAssemblyFormatHandling;
            set
            {
                if (value < TypeNameAssemblyFormatHandling.Simple || value > TypeNameAssemblyFormatHandling.Full)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _typeNameAssemblyFormatHandling = value;
            }
        }
        public virtual PreserveReferencesHandling PreserveReferencesHandling
        {
            get => _preserveReferencesHandling;
            set
            {
                if (value < PreserveReferencesHandling.None || value > PreserveReferencesHandling.All)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _preserveReferencesHandling = value;
            }
        }
        public virtual ReferenceLoopHandling ReferenceLoopHandling
        {
            get => _referenceLoopHandling;
            set
            {
                if (value < ReferenceLoopHandling.Error || value > ReferenceLoopHandling.Serialize)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _referenceLoopHandling = value;
            }
        }
        public virtual MissingMemberHandling MissingMemberHandling
        {
            get => _missingMemberHandling;
            set
            {
                if (value < MissingMemberHandling.Ignore || value > MissingMemberHandling.Error)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _missingMemberHandling = value;
            }
        }
        public virtual NullValueHandling NullValueHandling
        {
            get => _nullValueHandling;
            set
            {
                if (value < NullValueHandling.Include || value > NullValueHandling.Ignore)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _nullValueHandling = value;
            }
        }
        public virtual DefaultValueHandling DefaultValueHandling
        {
            get => _defaultValueHandling;
            set
            {
                if (value < DefaultValueHandling.Include || value > DefaultValueHandling.IgnoreAndPopulate)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _defaultValueHandling = value;
            }
        }
        public virtual ObjectCreationHandling ObjectCreationHandling
        {
            get => _objectCreationHandling;
            set
            {
                if (value < ObjectCreationHandling.Auto || value > ObjectCreationHandling.Replace)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _objectCreationHandling = value;
            }
        }
        public virtual ConstructorHandling ConstructorHandling
        {
            get => _constructorHandling;
            set
            {
                if (value < ConstructorHandling.Default || value > ConstructorHandling.AllowNonPublicDefaultConstructor)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _constructorHandling = value;
            }
        }
        public virtual MetadataPropertyHandling MetadataPropertyHandling
        {
            get => _metadataPropertyHandling;
            set
            {
                if (value < MetadataPropertyHandling.Default || value > MetadataPropertyHandling.Ignore)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _metadataPropertyHandling = value;
            }
        }
        public virtual JsonConverterCollection Converters
        {
            get
            {
                if (_converters == null)
                {
                    _converters = new JsonConverterCollection();
                }

                return _converters;
            }
        }
        public virtual IContractResolver ContractResolver
        {
            get => _contractResolver;
            set => _contractResolver = value ?? DefaultContractResolver.Instance;
        }
        public virtual StreamingContext Context
        {
            get => _context;
            set => _context = value;
        }
        public virtual Formatting Formatting
        {
            get => _formatting ?? JsonSerializerSettings.DefaultFormatting;
            set => _formatting = value;
        }
        public virtual DateFormatHandling DateFormatHandling
        {
            get => _dateFormatHandling ?? JsonSerializerSettings.DefaultDateFormatHandling;
            set => _dateFormatHandling = value;
        }
        public virtual DateTimeZoneHandling DateTimeZoneHandling
        {
            get => _dateTimeZoneHandling ?? JsonSerializerSettings.DefaultDateTimeZoneHandling;
            set => _dateTimeZoneHandling = value;
        }
        public virtual DateParseHandling DateParseHandling
        {
            get => _dateParseHandling ?? JsonSerializerSettings.DefaultDateParseHandling;
            set => _dateParseHandling = value;
        }
        public virtual FloatParseHandling FloatParseHandling
        {
            get => _floatParseHandling ?? JsonSerializerSettings.DefaultFloatParseHandling;
            set => _floatParseHandling = value;
        }
        public virtual FloatFormatHandling FloatFormatHandling
        {
            get => _floatFormatHandling ?? JsonSerializerSettings.DefaultFloatFormatHandling;
            set => _floatFormatHandling = value;
        }
        public virtual StringEscapeHandling StringEscapeHandling
        {
            get => _stringEscapeHandling ?? JsonSerializerSettings.DefaultStringEscapeHandling;
            set => _stringEscapeHandling = value;
        }
        public virtual string DateFormatString
        {
            get => _dateFormatString ?? JsonSerializerSettings.DefaultDateFormatString;
            set
            {
                _dateFormatString = value;
                _dateFormatStringSet = true;
            }
        }
        public virtual CultureInfo Culture
        {
            get => _culture ?? JsonSerializerSettings.DefaultCulture;
            set => _culture = value;
        }
        public virtual int? MaxDepth
        {
            get => _maxDepth;
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException("Value must be positive.", nameof(value));
                }

                _maxDepth = value;
                _maxDepthSet = true;
            }
        }
        public virtual bool CheckAdditionalContent
        {
            get => _checkAdditionalContent ?? JsonSerializerSettings.DefaultCheckAdditionalContent;
            set => _checkAdditionalContent = value;
        }

        internal bool IsCheckAdditionalContentSet()
        {
            return (_checkAdditionalContent != null);
        }
        public JsonSerializer()
        {
            _referenceLoopHandling = JsonSerializerSettings.DefaultReferenceLoopHandling;
            _missingMemberHandling = JsonSerializerSettings.DefaultMissingMemberHandling;
            _nullValueHandling = JsonSerializerSettings.DefaultNullValueHandling;
            _defaultValueHandling = JsonSerializerSettings.DefaultDefaultValueHandling;
            _objectCreationHandling = JsonSerializerSettings.DefaultObjectCreationHandling;
            _preserveReferencesHandling = JsonSerializerSettings.DefaultPreserveReferencesHandling;
            _constructorHandling = JsonSerializerSettings.DefaultConstructorHandling;
            _typeNameHandling = JsonSerializerSettings.DefaultTypeNameHandling;
            _metadataPropertyHandling = JsonSerializerSettings.DefaultMetadataPropertyHandling;
            _context = JsonSerializerSettings.DefaultContext;
            _serializationBinder = DefaultSerializationBinder.Instance;

            _culture = JsonSerializerSettings.DefaultCulture;
            _contractResolver = DefaultContractResolver.Instance;
        }
        public static JsonSerializer Create()
        {
            return new JsonSerializer();
        }
        public static JsonSerializer Create(JsonSerializerSettings? settings)
        {
            JsonSerializer serializer = Create();

            if (settings != null)
            {
                ApplySerializerSettings(serializer, settings);
            }

            return serializer;
        }
        public static JsonSerializer CreateDefault()
        {
            JsonSerializerSettings? defaultSettings = JsonConvert.DefaultSettings?.Invoke();

            return Create(defaultSettings);
        }
        public static JsonSerializer CreateDefault(JsonSerializerSettings? settings)
        {
            JsonSerializer serializer = CreateDefault();
            if (settings != null)
            {
                ApplySerializerSettings(serializer, settings);
            }

            return serializer;
        }

        private static void ApplySerializerSettings(JsonSerializer serializer, JsonSerializerSettings settings)
        {
            if (!CollectionUtils.IsNullOrEmpty(settings.Converters))
            {
                for (int i = 0; i < settings.Converters.Count; i++)
                {
                    serializer.Converters.Insert(i, settings.Converters[i]);
                }
            }
            if (settings._typeNameHandling != null)
            {
                serializer.TypeNameHandling = settings.TypeNameHandling;
            }
            if (settings._metadataPropertyHandling != null)
            {
                serializer.MetadataPropertyHandling = settings.MetadataPropertyHandling;
            }
            if (settings._typeNameAssemblyFormatHandling != null)
            {
                serializer.TypeNameAssemblyFormatHandling = settings.TypeNameAssemblyFormatHandling;
            }
            if (settings._preserveReferencesHandling != null)
            {
                serializer.PreserveReferencesHandling = settings.PreserveReferencesHandling;
            }
            if (settings._referenceLoopHandling != null)
            {
                serializer.ReferenceLoopHandling = settings.ReferenceLoopHandling;
            }
            if (settings._missingMemberHandling != null)
            {
                serializer.MissingMemberHandling = settings.MissingMemberHandling;
            }
            if (settings._objectCreationHandling != null)
            {
                serializer.ObjectCreationHandling = settings.ObjectCreationHandling;
            }
            if (settings._nullValueHandling != null)
            {
                serializer.NullValueHandling = settings.NullValueHandling;
            }
            if (settings._defaultValueHandling != null)
            {
                serializer.DefaultValueHandling = settings.DefaultValueHandling;
            }
            if (settings._constructorHandling != null)
            {
                serializer.ConstructorHandling = settings.ConstructorHandling;
            }
            if (settings._context != null)
            {
                serializer.Context = settings.Context;
            }
            if (settings._checkAdditionalContent != null)
            {
                serializer._checkAdditionalContent = settings._checkAdditionalContent;
            }

            if (settings.Error != null)
            {
                serializer.Error += settings.Error;
            }

            if (settings.ContractResolver != null)
            {
                serializer.ContractResolver = settings.ContractResolver;
            }
            if (settings.ReferenceResolverProvider != null)
            {
                serializer.ReferenceResolver = settings.ReferenceResolverProvider();
            }
            if (settings.TraceWriter != null)
            {
                serializer.TraceWriter = settings.TraceWriter;
            }
            if (settings.EqualityComparer != null)
            {
                serializer.EqualityComparer = settings.EqualityComparer;
            }
            if (settings.SerializationBinder != null)
            {
                serializer.SerializationBinder = settings.SerializationBinder;
            }
            if (settings._formatting != null)
            {
                serializer._formatting = settings._formatting;
            }
            if (settings._dateFormatHandling != null)
            {
                serializer._dateFormatHandling = settings._dateFormatHandling;
            }
            if (settings._dateTimeZoneHandling != null)
            {
                serializer._dateTimeZoneHandling = settings._dateTimeZoneHandling;
            }
            if (settings._dateParseHandling != null)
            {
                serializer._dateParseHandling = settings._dateParseHandling;
            }
            if (settings._dateFormatStringSet)
            {
                serializer._dateFormatString = settings._dateFormatString;
                serializer._dateFormatStringSet = settings._dateFormatStringSet;
            }
            if (settings._floatFormatHandling != null)
            {
                serializer._floatFormatHandling = settings._floatFormatHandling;
            }
            if (settings._floatParseHandling != null)
            {
                serializer._floatParseHandling = settings._floatParseHandling;
            }
            if (settings._stringEscapeHandling != null)
            {
                serializer._stringEscapeHandling = settings._stringEscapeHandling;
            }
            if (settings._culture != null)
            {
                serializer._culture = settings._culture;
            }
            if (settings._maxDepthSet)
            {
                serializer._maxDepth = settings._maxDepth;
                serializer._maxDepthSet = settings._maxDepthSet;
            }
        }
        [DebuggerStepThrough]
        public void Populate(TextReader reader, object target)
        {
            Populate(new JsonTextReader(reader), target);
        }
        [DebuggerStepThrough]
        public void Populate(JsonReader reader, object target)
        {
            PopulateInternal(reader, target);
        }

        internal virtual void PopulateInternal(JsonReader reader, object target)
        {
            ValidationUtils.ArgumentNotNull(reader, nameof(reader));
            ValidationUtils.ArgumentNotNull(target, nameof(target));

            SetupReader(
                reader,
                out CultureInfo? previousCulture,
                out DateTimeZoneHandling? previousDateTimeZoneHandling,
                out DateParseHandling? previousDateParseHandling,
                out FloatParseHandling? previousFloatParseHandling,
                out int? previousMaxDepth,
                out string? previousDateFormatString);

            TraceJsonReader? traceJsonReader = (TraceWriter != null && TraceWriter.LevelFilter >= TraceLevel.Verbose)
                ? CreateTraceJsonReader(reader)
                : null;

            JsonSerializerInternalReader serializerReader = new JsonSerializerInternalReader(this);
            serializerReader.Populate(traceJsonReader ?? reader, target);

            if (traceJsonReader != null)
            {
                TraceWriter!.Trace(TraceLevel.Verbose, traceJsonReader.GetDeserializedJsonMessage(), null);
            }

            ResetReader(reader, previousCulture, previousDateTimeZoneHandling, previousDateParseHandling, previousFloatParseHandling, previousMaxDepth, previousDateFormatString);
        }
        [DebuggerStepThrough]
        public object? Deserialize(JsonReader reader)
        {
            return Deserialize(reader, null);
        }
        [DebuggerStepThrough]
        public object? Deserialize(TextReader reader, Type objectType)
        {
            return Deserialize(new JsonTextReader(reader), objectType);
        }
        [DebuggerStepThrough]
        [return: MaybeNull]
        public T Deserialize<T>(JsonReader reader)
        {
#pragma warning disable CS8601 // Possible null reference assignment.
            return (T)Deserialize(reader, typeof(T));
#pragma warning restore CS8601 // Possible null reference assignment.
        }
        [DebuggerStepThrough]
        public object? Deserialize(JsonReader reader, Type? objectType)
        {
            return DeserializeInternal(reader, objectType);
        }

        internal virtual object? DeserializeInternal(JsonReader reader, Type? objectType)
        {
            ValidationUtils.ArgumentNotNull(reader, nameof(reader));

            SetupReader(
                reader,
                out CultureInfo? previousCulture,
                out DateTimeZoneHandling? previousDateTimeZoneHandling,
                out DateParseHandling? previousDateParseHandling,
                out FloatParseHandling? previousFloatParseHandling,
                out int? previousMaxDepth,
                out string? previousDateFormatString);

            TraceJsonReader? traceJsonReader = (TraceWriter != null && TraceWriter.LevelFilter >= TraceLevel.Verbose)
                ? CreateTraceJsonReader(reader)
                : null;

            JsonSerializerInternalReader serializerReader = new JsonSerializerInternalReader(this);
            object? value = serializerReader.Deserialize(traceJsonReader ?? reader, objectType, CheckAdditionalContent);

            if (traceJsonReader != null)
            {
                TraceWriter!.Trace(TraceLevel.Verbose, traceJsonReader.GetDeserializedJsonMessage(), null);
            }

            ResetReader(reader, previousCulture, previousDateTimeZoneHandling, previousDateParseHandling, previousFloatParseHandling, previousMaxDepth, previousDateFormatString);

            return value;
        }

        private void SetupReader(JsonReader reader, out CultureInfo? previousCulture, out DateTimeZoneHandling? previousDateTimeZoneHandling, out DateParseHandling? previousDateParseHandling, out FloatParseHandling? previousFloatParseHandling, out int? previousMaxDepth, out string? previousDateFormatString)
        {
            if (_culture != null && !_culture.Equals(reader.Culture))
            {
                previousCulture = reader.Culture;
                reader.Culture = _culture;
            }
            else
            {
                previousCulture = null;
            }

            if (_dateTimeZoneHandling != null && reader.DateTimeZoneHandling != _dateTimeZoneHandling)
            {
                previousDateTimeZoneHandling = reader.DateTimeZoneHandling;
                reader.DateTimeZoneHandling = _dateTimeZoneHandling.GetValueOrDefault();
            }
            else
            {
                previousDateTimeZoneHandling = null;
            }

            if (_dateParseHandling != null && reader.DateParseHandling != _dateParseHandling)
            {
                previousDateParseHandling = reader.DateParseHandling;
                reader.DateParseHandling = _dateParseHandling.GetValueOrDefault();
            }
            else
            {
                previousDateParseHandling = null;
            }

            if (_floatParseHandling != null && reader.FloatParseHandling != _floatParseHandling)
            {
                previousFloatParseHandling = reader.FloatParseHandling;
                reader.FloatParseHandling = _floatParseHandling.GetValueOrDefault();
            }
            else
            {
                previousFloatParseHandling = null;
            }

            if (_maxDepthSet && reader.MaxDepth != _maxDepth)
            {
                previousMaxDepth = reader.MaxDepth;
                reader.MaxDepth = _maxDepth;
            }
            else
            {
                previousMaxDepth = null;
            }

            if (_dateFormatStringSet && reader.DateFormatString != _dateFormatString)
            {
                previousDateFormatString = reader.DateFormatString;
                reader.DateFormatString = _dateFormatString;
            }
            else
            {
                previousDateFormatString = null;
            }

            if (reader is JsonTextReader textReader)
            {
                if (textReader.PropertyNameTable == null && _contractResolver is DefaultContractResolver resolver)
                {
                    textReader.PropertyNameTable = resolver.GetNameTable();
                }
            }
        }

        private void ResetReader(JsonReader reader, CultureInfo? previousCulture, DateTimeZoneHandling? previousDateTimeZoneHandling, DateParseHandling? previousDateParseHandling, FloatParseHandling? previousFloatParseHandling, int? previousMaxDepth, string? previousDateFormatString)
        {
            if (previousCulture != null)
            {
                reader.Culture = previousCulture;
            }
            if (previousDateTimeZoneHandling != null)
            {
                reader.DateTimeZoneHandling = previousDateTimeZoneHandling.GetValueOrDefault();
            }
            if (previousDateParseHandling != null)
            {
                reader.DateParseHandling = previousDateParseHandling.GetValueOrDefault();
            }
            if (previousFloatParseHandling != null)
            {
                reader.FloatParseHandling = previousFloatParseHandling.GetValueOrDefault();
            }
            if (_maxDepthSet)
            {
                reader.MaxDepth = previousMaxDepth;
            }
            if (_dateFormatStringSet)
            {
                reader.DateFormatString = previousDateFormatString;
            }

            if (reader is JsonTextReader textReader && textReader.PropertyNameTable != null &&
                _contractResolver is DefaultContractResolver resolver && textReader.PropertyNameTable == resolver.GetNameTable())
            {
                textReader.PropertyNameTable = null;
            }
        }
        public void Serialize(TextWriter textWriter, object? value)
        {
            Serialize(new JsonTextWriter(textWriter), value);
        }
        public void Serialize(JsonWriter jsonWriter, object? value, Type? objectType)
        {
            SerializeInternal(jsonWriter, value, objectType);
        }
        public void Serialize(TextWriter textWriter, object? value, Type objectType)
        {
            Serialize(new JsonTextWriter(textWriter), value, objectType);
        }
        public void Serialize(JsonWriter jsonWriter, object? value)
        {
            SerializeInternal(jsonWriter, value, null);
        }

        private TraceJsonReader CreateTraceJsonReader(JsonReader reader)
        {
            TraceJsonReader traceReader = new TraceJsonReader(reader);
            if (reader.TokenType != JsonToken.None)
            {
                traceReader.WriteCurrentToken();
            }

            return traceReader;
        }

        internal virtual void SerializeInternal(JsonWriter jsonWriter, object? value, Type? objectType)
        {
            ValidationUtils.ArgumentNotNull(jsonWriter, nameof(jsonWriter));
            Formatting? previousFormatting = null;
            if (_formatting != null && jsonWriter.Formatting != _formatting)
            {
                previousFormatting = jsonWriter.Formatting;
                jsonWriter.Formatting = _formatting.GetValueOrDefault();
            }

            DateFormatHandling? previousDateFormatHandling = null;
            if (_dateFormatHandling != null && jsonWriter.DateFormatHandling != _dateFormatHandling)
            {
                previousDateFormatHandling = jsonWriter.DateFormatHandling;
                jsonWriter.DateFormatHandling = _dateFormatHandling.GetValueOrDefault();
            }

            DateTimeZoneHandling? previousDateTimeZoneHandling = null;
            if (_dateTimeZoneHandling != null && jsonWriter.DateTimeZoneHandling != _dateTimeZoneHandling)
            {
                previousDateTimeZoneHandling = jsonWriter.DateTimeZoneHandling;
                jsonWriter.DateTimeZoneHandling = _dateTimeZoneHandling.GetValueOrDefault();
            }

            FloatFormatHandling? previousFloatFormatHandling = null;
            if (_floatFormatHandling != null && jsonWriter.FloatFormatHandling != _floatFormatHandling)
            {
                previousFloatFormatHandling = jsonWriter.FloatFormatHandling;
                jsonWriter.FloatFormatHandling = _floatFormatHandling.GetValueOrDefault();
            }

            StringEscapeHandling? previousStringEscapeHandling = null;
            if (_stringEscapeHandling != null && jsonWriter.StringEscapeHandling != _stringEscapeHandling)
            {
                previousStringEscapeHandling = jsonWriter.StringEscapeHandling;
                jsonWriter.StringEscapeHandling = _stringEscapeHandling.GetValueOrDefault();
            }

            CultureInfo? previousCulture = null;
            if (_culture != null && !_culture.Equals(jsonWriter.Culture))
            {
                previousCulture = jsonWriter.Culture;
                jsonWriter.Culture = _culture;
            }

            string? previousDateFormatString = null;
            if (_dateFormatStringSet && jsonWriter.DateFormatString != _dateFormatString)
            {
                previousDateFormatString = jsonWriter.DateFormatString;
                jsonWriter.DateFormatString = _dateFormatString;
            }

            TraceJsonWriter? traceJsonWriter = (TraceWriter != null && TraceWriter.LevelFilter >= TraceLevel.Verbose)
                ? new TraceJsonWriter(jsonWriter)
                : null;

            JsonSerializerInternalWriter serializerWriter = new JsonSerializerInternalWriter(this);
            serializerWriter.Serialize(traceJsonWriter ?? jsonWriter, value, objectType);

            if (traceJsonWriter != null)
            {
                TraceWriter!.Trace(TraceLevel.Verbose, traceJsonWriter.GetSerializedJsonMessage(), null);
            }
            if (previousFormatting != null)
            {
                jsonWriter.Formatting = previousFormatting.GetValueOrDefault();
            }
            if (previousDateFormatHandling != null)
            {
                jsonWriter.DateFormatHandling = previousDateFormatHandling.GetValueOrDefault();
            }
            if (previousDateTimeZoneHandling != null)
            {
                jsonWriter.DateTimeZoneHandling = previousDateTimeZoneHandling.GetValueOrDefault();
            }
            if (previousFloatFormatHandling != null)
            {
                jsonWriter.FloatFormatHandling = previousFloatFormatHandling.GetValueOrDefault();
            }
            if (previousStringEscapeHandling != null)
            {
                jsonWriter.StringEscapeHandling = previousStringEscapeHandling.GetValueOrDefault();
            }
            if (_dateFormatStringSet)
            {
                jsonWriter.DateFormatString = previousDateFormatString;
            }
            if (previousCulture != null)
            {
                jsonWriter.Culture = previousCulture;
            }
        }

        internal IReferenceResolver GetReferenceResolver()
        {
            if (_referenceResolver == null)
            {
                _referenceResolver = new DefaultReferenceResolver();
            }

            return _referenceResolver;
        }

        internal JsonConverter? GetMatchingConverter(Type type)
        {
            return GetMatchingConverter(_converters, type);
        }

        internal static JsonConverter? GetMatchingConverter(IList<JsonConverter>? converters, Type objectType)
        {
#if DEBUG
            ValidationUtils.ArgumentNotNull(objectType, nameof(objectType));
#endif

            if (converters != null)
            {
                for (int i = 0; i < converters.Count; i++)
                {
                    JsonConverter converter = converters[i];

                    if (converter.CanConvert(objectType))
                    {
                        return converter;
                    }
                }
            }

            return null;
        }

        internal void OnError(ErrorEventArgs e)
        {
            Error?.Invoke(this, e);
        }
    }
}
