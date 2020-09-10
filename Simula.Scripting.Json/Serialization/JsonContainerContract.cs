
using System;
using System.Collections.Generic;
using System.Reflection;
using Simula.Scripting.Json.Utilities;
using System.Collections;
#if !HAVE_LINQ
using Simula.Scripting.Json.Utilities.LinqBridge;
#else
using System.Linq;

#endif

namespace Simula.Scripting.Json.Serialization
{
    public class JsonContainerContract : JsonContract
    {
        private JsonContract? _itemContract;
        private JsonContract? _finalItemContract;
        internal JsonContract? ItemContract
        {
            get => _itemContract;
            set
            {
                _itemContract = value;
                if (_itemContract != null)
                {
                    _finalItemContract = (_itemContract.UnderlyingType.IsSealed()) ? _itemContract : null;
                }
                else
                {
                    _finalItemContract = null;
                }
            }
        }
        internal JsonContract? FinalItemContract => _finalItemContract;
        public JsonConverter? ItemConverter { get; set; }
        public bool? ItemIsReference { get; set; }
        public ReferenceLoopHandling? ItemReferenceLoopHandling { get; set; }
        public TypeNameHandling? ItemTypeNameHandling { get; set; }
        internal JsonContainerContract(Type underlyingType)
            : base(underlyingType)
        {
            JsonContainerAttribute? jsonContainerAttribute = JsonTypeReflector.GetCachedAttribute<JsonContainerAttribute>(underlyingType);

            if (jsonContainerAttribute != null)
            {
                if (jsonContainerAttribute.ItemConverterType != null)
                {
                    ItemConverter = JsonTypeReflector.CreateJsonConverterInstance(
                        jsonContainerAttribute.ItemConverterType,
                        jsonContainerAttribute.ItemConverterParameters);
                }

                ItemIsReference = jsonContainerAttribute._itemIsReference;
                ItemReferenceLoopHandling = jsonContainerAttribute._itemReferenceLoopHandling;
                ItemTypeNameHandling = jsonContainerAttribute._itemTypeNameHandling;
            }
        }
    }
}