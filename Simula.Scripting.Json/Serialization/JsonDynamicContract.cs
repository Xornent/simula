
#if HAVE_DYNAMIC
using Simula.Scripting.Json.Utilities;
using System;
using System.Dynamic;
using System.Runtime.CompilerServices;

namespace Simula.Scripting.Json.Serialization
{
    public class JsonDynamicContract : JsonContainerContract
    {
        public JsonPropertyCollection Properties { get; }
        public Func<string, string>? PropertyNameResolver { get; set; }

        private readonly ThreadSafeStore<string, CallSite<Func<CallSite, object, object>>> _callSiteGetters =
            new ThreadSafeStore<string, CallSite<Func<CallSite, object, object>>>(CreateCallSiteGetter);

        private readonly ThreadSafeStore<string, CallSite<Func<CallSite, object, object?, object>>> _callSiteSetters =
            new ThreadSafeStore<string, CallSite<Func<CallSite, object, object?, object>>>(CreateCallSiteSetter);

        private static CallSite<Func<CallSite, object, object>> CreateCallSiteGetter(string name)
        {
            GetMemberBinder getMemberBinder = (GetMemberBinder)DynamicUtils.BinderWrapper.GetMember(name, typeof(DynamicUtils));

            return CallSite<Func<CallSite, object, object>>.Create(new NoThrowGetBinderMember(getMemberBinder));
        }

        private static CallSite<Func<CallSite, object, object?, object>> CreateCallSiteSetter(string name)
        {
            SetMemberBinder binder = (SetMemberBinder)DynamicUtils.BinderWrapper.SetMember(name, typeof(DynamicUtils));

            return CallSite<Func<CallSite, object, object?, object>>.Create(new NoThrowSetBinderMember(binder));
        }
        public JsonDynamicContract(Type underlyingType)
            : base(underlyingType)
        {
            ContractType = JsonContractType.Dynamic;

            Properties = new JsonPropertyCollection(UnderlyingType);
        }

        internal bool TryGetMember(IDynamicMetaObjectProvider dynamicProvider, string name, out object? value)
        {
            ValidationUtils.ArgumentNotNull(dynamicProvider, nameof(dynamicProvider));

            CallSite<Func<CallSite, object, object>> callSite = _callSiteGetters.Get(name);

            object result = callSite.Target(callSite, dynamicProvider);

            if (!ReferenceEquals(result, NoThrowExpressionVisitor.ErrorResult)) {
                value = result;
                return true;
            } else {
                value = null;
                return false;
            }
        }

        internal bool TrySetMember(IDynamicMetaObjectProvider dynamicProvider, string name, object? value)
        {
            ValidationUtils.ArgumentNotNull(dynamicProvider, nameof(dynamicProvider));

            CallSite<Func<CallSite, object, object?, object>> callSite = _callSiteSetters.Get(name);

            object result = callSite.Target(callSite, dynamicProvider, value);

            return !ReferenceEquals(result, NoThrowExpressionVisitor.ErrorResult);
        }
    }
}

#endif