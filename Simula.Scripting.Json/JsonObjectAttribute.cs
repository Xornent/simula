
using System;

namespace Simula.Scripting.Json
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = false)]
    public sealed class JsonObjectAttribute : JsonContainerAttribute
    {
        private MemberSerialization _memberSerialization = MemberSerialization.OptOut;
        internal MissingMemberHandling? _missingMemberHandling;
        internal Required? _itemRequired;
        internal NullValueHandling? _itemNullValueHandling;
        public MemberSerialization MemberSerialization
        {
            get => _memberSerialization;
            set => _memberSerialization = value;
        }
        public MissingMemberHandling MissingMemberHandling
        {
            get => _missingMemberHandling ?? default;
            set => _missingMemberHandling = value;
        }
        public NullValueHandling ItemNullValueHandling
        {
            get => _itemNullValueHandling ?? default;
            set => _itemNullValueHandling = value;
        }
        public Required ItemRequired
        {
            get => _itemRequired ?? default;
            set => _itemRequired = value;
        }
        public JsonObjectAttribute()
        {
        }
        public JsonObjectAttribute(MemberSerialization memberSerialization)
        {
            MemberSerialization = memberSerialization;
        }
        public JsonObjectAttribute(string id)
            : base(id)
        {
        }
    }
}