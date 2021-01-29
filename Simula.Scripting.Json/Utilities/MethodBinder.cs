
using System;
using System.Collections.Generic;
#if !HAVE_LINQ
using Simula.Scripting.Json.Utilities.LinqBridge;
#else
using System.Linq;
#endif
using System.Reflection;

namespace Simula.Scripting.Json.Utilities
{
#if PORTABLE
    internal static class MethodBinder
    {
        private static readonly Type[] PrimitiveTypes = new Type[]
        {
            typeof(bool),  typeof(char),   typeof(sbyte), typeof(byte),
            typeof(short), typeof(ushort), typeof(int),   typeof(uint),
            typeof(long),  typeof(ulong),  typeof(float), typeof(double)
        };
        private static readonly int[] WideningMasks = new int[]
        {
            0x0001,        0x0FE2,         0x0D54,        0x0FFA,
            0x0D50,        0x0FE2,         0x0D40,        0x0F80,
            0x0D00,        0x0E00,         0x0C00,        0x0800
        };
        private static bool CanConvertPrimitive(Type from, Type to)
        {
            if (from == to) {
                return true;
            }

            int fromMask = 0;
            int toMask = 0;

            for (int i = 0; i < PrimitiveTypes.Length; i++) {
                if (PrimitiveTypes[i] == from) {
                    fromMask = WideningMasks[i];
                } else if (PrimitiveTypes[i] == to) {
                    toMask = 1 << i;
                }

                if (fromMask != 0 && toMask != 0) {
                    break;
                }
            }

            return (fromMask & toMask) != 0;
        }
        private static bool FilterParameters(ParameterInfo[] parameters, IList<Type> types, bool enableParamArray)
        {
            ValidationUtils.ArgumentNotNull(parameters, nameof(parameters));
            ValidationUtils.ArgumentNotNull(types, nameof(types));

            if (parameters.Length == 0) {
                return types.Count == 0;
            }
            if (parameters.Length > types.Count) {
                return false;
            }
            Type? paramArrayType = null;

            if (enableParamArray) {
                ParameterInfo lastParam = parameters[parameters.Length - 1];
                if (lastParam.ParameterType.IsArray && lastParam.IsDefined(typeof(ParamArrayAttribute))) {
                    paramArrayType = lastParam.ParameterType.GetElementType();
                }
            }

            if (paramArrayType == null && parameters.Length != types.Count) {
                return false;
            }

            for (int i = 0; i < types.Count; i++) {
                Type paramType = (paramArrayType != null && i >= parameters.Length - 1) ? paramArrayType : parameters[i].ParameterType;

                if (paramType == types[i]) {
                    continue;
                }

                if (paramType == typeof(object)) {
                    continue;
                }

                if (paramType.IsPrimitive()) {
                    if (!types[i].IsPrimitive() || !CanConvertPrimitive(types[i], paramType)) {
                        return false;
                    }
                } else {
                    if (!paramType.IsAssignableFrom(types[i])) {
                        return false;
                    }
                }
            }

            return true;
        }
        private class ParametersMatchComparer : IComparer<ParameterInfo[]>
        {
            private readonly IList<Type> _types;
            private readonly bool _enableParamArray;

            public ParametersMatchComparer(IList<Type> types, bool enableParamArray)
            {
                ValidationUtils.ArgumentNotNull(types, nameof(types));

                _types = types;
                _enableParamArray = enableParamArray;
            }

            public int Compare(ParameterInfo[] parameters1, ParameterInfo[] parameters2)
            {
                ValidationUtils.ArgumentNotNull(parameters1, nameof(parameters1));
                ValidationUtils.ArgumentNotNull(parameters2, nameof(parameters2));
                if (parameters1.Length == 0) {
                    return -1;
                }
                if (parameters2.Length == 0) {
                    return 1;
                }

                Type? paramArrayType1 = null, paramArrayType2 = null;

                if (_enableParamArray) {
                    ParameterInfo lastParam1 = parameters1[parameters1.Length - 1];
                    if (lastParam1.ParameterType.IsArray && lastParam1.IsDefined(typeof(ParamArrayAttribute))) {
                        paramArrayType1 = lastParam1.ParameterType.GetElementType();
                    }

                    ParameterInfo lastParam2 = parameters2[parameters2.Length - 1];
                    if (lastParam2.ParameterType.IsArray && lastParam2.IsDefined(typeof(ParamArrayAttribute))) {
                        paramArrayType2 = lastParam2.ParameterType.GetElementType();
                    }
                    if (paramArrayType1 != null && paramArrayType2 == null) {
                        return 1;
                    }
                    if (paramArrayType2 != null && paramArrayType1 == null) {
                        return -1;
                    }
                }

                for (int i = 0; i < _types.Count; i++) {
                    Type type1 = (paramArrayType1 != null && i >= parameters1.Length - 1) ? paramArrayType1 : parameters1[i].ParameterType;
                    Type type2 = (paramArrayType2 != null && i >= parameters2.Length - 1) ? paramArrayType2 : parameters2[i].ParameterType;

                    if (type1 == type2) {
                        continue;
                    }
                    if (type1 == _types[i]) {
                        return -1;
                    }
                    if (type2 == _types[i]) {
                        return 1;
                    }

                    int r = ChooseMorePreciseType(type1, type2);
                    if (r != 0) {
                        return r;
                    }
                }

                return 0;
            }

            private static int ChooseMorePreciseType(Type type1, Type type2)
            {
                if (type1.IsByRef || type2.IsByRef) {
                    if (type1.IsByRef && type2.IsByRef) {
                        type1 = type1.GetElementType();
                        type2 = type2.GetElementType();
                    } else if (type1.IsByRef) {
                        type1 = type1.GetElementType();
                        if (type1 == type2) {
                            return 1;
                        }
                    } else {
                        type2 = type2.GetElementType();
                        if (type2 == type1) {
                            return -1;
                        }
                    }
                }

                bool c1FromC2, c2FromC1;

                if (type1.IsPrimitive() && type2.IsPrimitive()) {
                    c1FromC2 = CanConvertPrimitive(type2, type1);
                    c2FromC1 = CanConvertPrimitive(type1, type2);
                } else {
                    c1FromC2 = type1.IsAssignableFrom(type2);
                    c2FromC1 = type2.IsAssignableFrom(type1);
                }

                if (c1FromC2 == c2FromC1) {
                    return 0;
                }

                return c1FromC2 ? 1 : -1;
            }

        }
        public static TMethod SelectMethod<TMethod>(IEnumerable<TMethod> candidates, IList<Type> types) where TMethod : MethodBase
        {
            ValidationUtils.ArgumentNotNull(candidates, nameof(candidates));
            ValidationUtils.ArgumentNotNull(types, nameof(types));
            const bool enableParamArray = false;

            return candidates
                .Where(m => FilterParameters(m.GetParameters(), types, enableParamArray))
                .OrderBy(m => m.GetParameters(), new ParametersMatchComparer(types, enableParamArray))
                .FirstOrDefault();
        }

    }
#endif
}