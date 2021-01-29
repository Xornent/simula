﻿
using System;

namespace Simula.Scripting.Json
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false)]
    public sealed class JsonArrayAttribute : JsonContainerAttribute
    {
        private bool _allowNullItems;
        public bool AllowNullItems {
            get => _allowNullItems;
            set => _allowNullItems = value;
        }
        public JsonArrayAttribute()
        {
        }
        public JsonArrayAttribute(bool allowNullItems)
        {
            _allowNullItems = allowNullItems;
        }
        public JsonArrayAttribute(string id)
            : base(id)
        {
        }
    }
}