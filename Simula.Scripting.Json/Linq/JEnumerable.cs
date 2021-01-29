
using System;
using System.Collections.Generic;
#if !HAVE_LINQ
using Simula.Scripting.Json.Utilities.LinqBridge;
#else
using System.Linq;
#endif
using Simula.Scripting.Json.Utilities;
using System.Collections;

namespace Simula.Scripting.Json.Linq
{
    public readonly struct JEnumerable<T> : IJEnumerable<T>, IEquatable<JEnumerable<T>> where T : JToken
    {
        public static readonly JEnumerable<T> Empty = new JEnumerable<T>(Enumerable.Empty<T>());

        private readonly IEnumerable<T> _enumerable;
        public JEnumerable(IEnumerable<T> enumerable)
        {
            ValidationUtils.ArgumentNotNull(enumerable, nameof(enumerable));

            _enumerable = enumerable;
        }
        public IEnumerator<T> GetEnumerator()
        {
            return (_enumerable ?? Empty).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public IJEnumerable<JToken> this[object key] {
            get {
                if (_enumerable == null) {
                    return JEnumerable<JToken>.Empty;
                }

                return new JEnumerable<JToken>(_enumerable.Values<T, JToken>(key));
            }
        }
        public bool Equals(JEnumerable<T> other)
        {
            return Equals(_enumerable, other._enumerable);
        }
        public override bool Equals(object obj)
        {
            if (obj is JEnumerable<T> enumerable) {
                return Equals(enumerable);
            }

            return false;
        }
        public override int GetHashCode()
        {
            if (_enumerable == null) {
                return 0;
            }

            return _enumerable.GetHashCode();
        }
    }
}