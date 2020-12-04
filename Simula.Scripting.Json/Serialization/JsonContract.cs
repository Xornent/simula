
using Simula.Scripting.Json.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;

namespace Simula.Scripting.Json.Serialization
{
    internal enum JsonContractType
    {
        None = 0,
        Object = 1,
        Array = 2,
        Primitive = 3,
        String = 4,
        Dictionary = 5,
        Dynamic = 6,
        Serializable = 7,
        Linq = 8
    }
    public delegate void SerializationCallback(object o, StreamingContext context);
    public delegate void SerializationErrorCallback(object o, StreamingContext context, ErrorContext errorContext);
    public delegate void ExtensionDataSetter(object o, string key, object? value);
    public delegate IEnumerable<KeyValuePair<object, object>>? ExtensionDataGetter(object o);
    public abstract class JsonContract
    {
        internal bool IsNullable;
        internal bool IsConvertable;
        internal bool IsEnum;
        internal Type NonNullableUnderlyingType;
        internal ReadType InternalReadType;
        internal JsonContractType ContractType;
        internal bool IsReadOnlyOrFixedSize;
        internal bool IsSealed;
        internal bool IsInstantiable;

        private List<SerializationCallback>? _onDeserializedCallbacks;
        private List<SerializationCallback>? _onDeserializingCallbacks;
        private List<SerializationCallback>? _onSerializedCallbacks;
        private List<SerializationCallback>? _onSerializingCallbacks;
        private List<SerializationErrorCallback>? _onErrorCallbacks;
        private Type _createdType;
        public Type UnderlyingType { get; }
        public Type CreatedType {
            get => _createdType;
            set {
                ValidationUtils.ArgumentNotNull(value, nameof(value));
                _createdType = value;

                IsSealed = _createdType.IsSealed();
                IsInstantiable = !(_createdType.IsInterface() || _createdType.IsAbstract());
            }
        }
        public bool? IsReference { get; set; }
        public JsonConverter? Converter { get; set; }
        public JsonConverter? InternalConverter { get; internal set; }
        public IList<SerializationCallback> OnDeserializedCallbacks {
            get {
                if (_onDeserializedCallbacks == null) {
                    _onDeserializedCallbacks = new List<SerializationCallback>();
                }

                return _onDeserializedCallbacks;
            }
        }
        public IList<SerializationCallback> OnDeserializingCallbacks {
            get {
                if (_onDeserializingCallbacks == null) {
                    _onDeserializingCallbacks = new List<SerializationCallback>();
                }

                return _onDeserializingCallbacks;
            }
        }
        public IList<SerializationCallback> OnSerializedCallbacks {
            get {
                if (_onSerializedCallbacks == null) {
                    _onSerializedCallbacks = new List<SerializationCallback>();
                }

                return _onSerializedCallbacks;
            }
        }
        public IList<SerializationCallback> OnSerializingCallbacks {
            get {
                if (_onSerializingCallbacks == null) {
                    _onSerializingCallbacks = new List<SerializationCallback>();
                }

                return _onSerializingCallbacks;
            }
        }
        public IList<SerializationErrorCallback> OnErrorCallbacks {
            get {
                if (_onErrorCallbacks == null) {
                    _onErrorCallbacks = new List<SerializationErrorCallback>();
                }

                return _onErrorCallbacks;
            }
        }
        public Func<object>? DefaultCreator { get; set; }
        public bool DefaultCreatorNonPublic { get; set; }

        internal JsonContract(Type underlyingType)
        {
            ValidationUtils.ArgumentNotNull(underlyingType, nameof(underlyingType));

            UnderlyingType = underlyingType;
            underlyingType = ReflectionUtils.EnsureNotByRefType(underlyingType);

            IsNullable = ReflectionUtils.IsNullable(underlyingType);

            NonNullableUnderlyingType = (IsNullable && ReflectionUtils.IsNullableType(underlyingType)) ? Nullable.GetUnderlyingType(underlyingType) : underlyingType;

            _createdType = CreatedType = NonNullableUnderlyingType;

            IsConvertable = ConvertUtils.IsConvertible(NonNullableUnderlyingType);
            IsEnum = NonNullableUnderlyingType.IsEnum();

            InternalReadType = ReadType.Read;
        }

        internal void InvokeOnSerializing(object o, StreamingContext context)
        {
            if (_onSerializingCallbacks != null) {
                foreach (SerializationCallback callback in _onSerializingCallbacks) {
                    callback(o, context);
                }
            }
        }

        internal void InvokeOnSerialized(object o, StreamingContext context)
        {
            if (_onSerializedCallbacks != null) {
                foreach (SerializationCallback callback in _onSerializedCallbacks) {
                    callback(o, context);
                }
            }
        }

        internal void InvokeOnDeserializing(object o, StreamingContext context)
        {
            if (_onDeserializingCallbacks != null) {
                foreach (SerializationCallback callback in _onDeserializingCallbacks) {
                    callback(o, context);
                }
            }
        }

        internal void InvokeOnDeserialized(object o, StreamingContext context)
        {
            if (_onDeserializedCallbacks != null) {
                foreach (SerializationCallback callback in _onDeserializedCallbacks) {
                    callback(o, context);
                }
            }
        }

        internal void InvokeOnError(object o, StreamingContext context, ErrorContext errorContext)
        {
            if (_onErrorCallbacks != null) {
                foreach (SerializationErrorCallback callback in _onErrorCallbacks) {
                    callback(o, context, errorContext);
                }
            }
        }

        internal static SerializationCallback CreateSerializationCallback(MethodInfo callbackMethodInfo)
        {
            return (o, context) => callbackMethodInfo.Invoke(o, new object[] { context });
        }

        internal static SerializationErrorCallback CreateSerializationErrorCallback(MethodInfo callbackMethodInfo)
        {
            return (o, context, econtext) => callbackMethodInfo.Invoke(o, new object[] { context, econtext });
        }
    }
}