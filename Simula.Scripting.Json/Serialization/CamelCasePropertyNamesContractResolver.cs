using System;
using System.Collections.Generic;
using System.Globalization;
using Simula.Scripting.Json.Utilities;

namespace Simula.Scripting.Json.Serialization
{
    internal struct ResolverContractKey : IEquatable<ResolverContractKey>
    {
        private readonly Type _resolverType;
        private readonly Type _contractType;

        public ResolverContractKey(Type resolverType, Type contractType)
        {
            _resolverType = resolverType;
            _contractType = contractType;
        }

        public override int GetHashCode()
        {
            return _resolverType.GetHashCode() ^ _contractType.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ResolverContractKey))
            {
                return false;
            }

            return Equals((ResolverContractKey)obj);
        }

        public bool Equals(ResolverContractKey other)
        {
            return (_resolverType == other._resolverType && _contractType == other._contractType);
        }
    }

    /// <summary>
    /// Resolves member mappings for a type, camel casing property names.
    /// </summary>
    public class CamelCasePropertyNamesContractResolver : DefaultContractResolver
    {
        private static readonly object TypeContractCacheLock = new object();
        private static readonly PropertyNameTable NameTable = new PropertyNameTable();
        private static Dictionary<ResolverContractKey, JsonContract> _contractCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="CamelCasePropertyNamesContractResolver"/> class.
        /// </summary>
        public CamelCasePropertyNamesContractResolver()
        {
            NamingStrategy = new CamelCaseNamingStrategy
            {
                ProcessDictionaryKeys = true,
                OverrideSpecifiedNames = true
            };
        }

        /// <summary>
        /// Resolves the contract for a given type.
        /// </summary>
        /// <param name="type">The type to resolve a contract for.</param>
        /// <returns>The contract for a given type.</returns>
        public override JsonContract ResolveContract(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            // for backwards compadibility the CamelCasePropertyNamesContractResolver shares contracts between instances
            JsonContract contract;
            ResolverContractKey key = new ResolverContractKey(GetType(), type);
            Dictionary<ResolverContractKey, JsonContract> cache = _contractCache;
            if (cache == null || !cache.TryGetValue(key, out contract))
            {
                contract = CreateContract(type);

                // avoid the possibility of modifying the cache dictionary while another thread is accessing it
                lock (TypeContractCacheLock)
                {
                    cache = _contractCache;
                    Dictionary<ResolverContractKey, JsonContract> updatedCache = (cache != null)
                        ? new Dictionary<ResolverContractKey, JsonContract>(cache)
                        : new Dictionary<ResolverContractKey, JsonContract>();
                    updatedCache[key] = contract;

                    _contractCache = updatedCache;
                }
            }

            return contract;
        }

        internal override PropertyNameTable GetNameTable()
        {
            return NameTable;
        }
    }
}