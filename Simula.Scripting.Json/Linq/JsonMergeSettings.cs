
using System;

namespace Simula.Scripting.Json.Linq
{
    public class JsonMergeSettings
    {
        private MergeArrayHandling _mergeArrayHandling;
        private MergeNullValueHandling _mergeNullValueHandling;
        private StringComparison _propertyNameComparison;
        public JsonMergeSettings()
        {
            _propertyNameComparison = StringComparison.Ordinal;
        }
        public MergeArrayHandling MergeArrayHandling
        {
            get => _mergeArrayHandling;
            set
            {
                if (value < MergeArrayHandling.Concat || value > MergeArrayHandling.Merge)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _mergeArrayHandling = value;
            }
        }
        public MergeNullValueHandling MergeNullValueHandling
        {
            get => _mergeNullValueHandling;
            set
            {
                if (value < MergeNullValueHandling.Ignore || value > MergeNullValueHandling.Merge)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _mergeNullValueHandling = value;
            }
        }
        public StringComparison PropertyNameComparison
        {
            get => _propertyNameComparison;
            set
            {
                if (value < StringComparison.CurrentCulture || value > StringComparison.OrdinalIgnoreCase)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _propertyNameComparison = value;
            }
        }
    }
}