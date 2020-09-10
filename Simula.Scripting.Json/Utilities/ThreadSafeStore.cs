
using System;
using System.Collections.Generic;
#if !HAVE_LINQ
using Simula.Scripting.Json.Utilities.LinqBridge;
#endif
#if HAVE_CONCURRENT_DICTIONARY
using System.Collections.Concurrent;
#endif
using System.Threading;
using Simula.Scripting.Json.Serialization;

namespace Simula.Scripting.Json.Utilities
{
    internal class ThreadSafeStore<TKey, TValue>
    {
#if HAVE_CONCURRENT_DICTIONARY
        private readonly ConcurrentDictionary<TKey, TValue> _concurrentStore;
#else
        private readonly object _lock = new object();
        private Dictionary<TKey, TValue> _store;
#endif
        private readonly Func<TKey, TValue> _creator;

        public ThreadSafeStore(Func<TKey, TValue> creator)
        {
            ValidationUtils.ArgumentNotNull(creator, nameof(creator));

            _creator = creator;
#if HAVE_CONCURRENT_DICTIONARY
            _concurrentStore = new ConcurrentDictionary<TKey, TValue>();
#else
            _store = new Dictionary<TKey, TValue>();
#endif
        }

        public TValue Get(TKey key)
        {
#if HAVE_CONCURRENT_DICTIONARY
            return _concurrentStore.GetOrAdd(key, _creator);
#else
            if (!_store.TryGetValue(key, out TValue value))
            {
                return AddValue(key);
            }

            return value;
#endif
        }

#if !HAVE_CONCURRENT_DICTIONARY
        private TValue AddValue(TKey key)
        {
            TValue value = _creator(key);

            lock (_lock)
            {
                if (_store == null)
                {
                    _store = new Dictionary<TKey, TValue>();
                    _store[key] = value;
                }
                else
                {
                    if (_store.TryGetValue(key, out TValue checkValue))
                    {
                        return checkValue;
                    }

                    Dictionary<TKey, TValue> newStore = new Dictionary<TKey, TValue>(_store);
                    newStore[key] = value;

#if HAVE_MEMORY_BARRIER
                    Thread.MemoryBarrier();
#endif
                    _store = newStore;
                }

                return value;
            }
        }
#endif
    }
}