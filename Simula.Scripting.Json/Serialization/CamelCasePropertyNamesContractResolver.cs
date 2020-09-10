
using System;
using System.Collections.Generic;
using System.Globalization;
using Simula.Scripting.Json.Utilities;

namespace Simula.Scripting.Json.Serialization
{
    public class CamelCasePropertyNamesContractResolver : DefaultContractResolver
    {
        private static readonly object TypeContractCacheLock = new object();
        private static readonly DefaultJsonNameTable NameTable = new DefaultJsonNameTable();
        private static Dictionary<StructMultiKey<Type, Type>, JsonContract>? _contractCache;
        public CamelCasePropertyNamesContractResolver()
        {
            NamingStrategy = new CamelCaseNamingStrategy
            {
                ProcessDictionaryKeys = true,
                OverrideSpecifiedNames = true
            };
        }
        public override JsonContract ResolveContract(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            StructMultiKey<Type, Type> key = new StructMultiKey<Type, Type>(GetType(), type);
            Dictionary<StructMultiKey<Type, Type>, JsonContract>? cache = _contractCache;
            if (cache == null || !cache.TryGetValue(key, out JsonContract contract))
            {
                contract = CreateContract(type);
                lock (TypeContractCacheLock)
                {
                    cache = _contractCache;
                    Dictionary<StructMultiKey<Type, Type>, JsonContract> updatedCache = (cache != null)
                        ? new Dictionary<StructMultiKey<Type, Type>, JsonContract>(cache)
                        : new Dictionary<StructMultiKey<Type, Type>, JsonContract>();
                    updatedCache[key] = contract;

                    _contractCache = updatedCache;
                }
            }

            return contract;
        }

        internal override DefaultJsonNameTable GetNameTable()
        {
            return NameTable;
        }
    }
}