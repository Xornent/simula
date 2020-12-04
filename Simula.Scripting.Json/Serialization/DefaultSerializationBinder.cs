
using Simula.Scripting.Json.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace Simula.Scripting.Json.Serialization
{
    public class DefaultSerializationBinder :
#pragma warning disable 618
        SerializationBinder,
#pragma warning restore 618
        ISerializationBinder
    {
        internal static readonly DefaultSerializationBinder Instance = new DefaultSerializationBinder();

        private readonly ThreadSafeStore<StructMultiKey<string?, string>, Type> _typeCache;
        public DefaultSerializationBinder()
        {
            _typeCache = new ThreadSafeStore<StructMultiKey<string?, string>, Type>(GetTypeFromTypeNameKey);
        }

        private Type GetTypeFromTypeNameKey(StructMultiKey<string?, string> typeNameKey)
        {
            string? assemblyName = typeNameKey.Value1;
            string typeName = typeNameKey.Value2;

            if (assemblyName != null) {
                Assembly assembly;

#if !(DOTNET || PORTABLE40 || PORTABLE)
#pragma warning disable 618,612
                assembly = Assembly.LoadWithPartialName(assemblyName);
#pragma warning restore 618,612
#elif DOTNET || PORTABLE
                assembly = Assembly.Load(new AssemblyName(assemblyName));
#else
                assembly = Assembly.Load(assemblyName);
#endif

#if HAVE_APP_DOMAIN
                if (assembly == null)
                {
                    Assembly[] loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
                    foreach (Assembly a in loadedAssemblies)
                    {
                        if (a.FullName == assemblyName || a.GetName().Name == assemblyName)
                        {
                            assembly = a;
                            break;
                        }
                    }
                }
#endif

                if (assembly == null) {
                    throw new JsonSerializationException("Could not load assembly '{0}'.".FormatWith(CultureInfo.InvariantCulture, assemblyName));
                }

                Type? type = assembly.GetType(typeName);
                if (type == null) {
                    if (typeName.IndexOf('`') >= 0) {
                        try {
                            type = GetGenericTypeFromTypeName(typeName, assembly);
                        } catch (Exception ex) {
                            throw new JsonSerializationException("Could not find type '{0}' in assembly '{1}'.".FormatWith(CultureInfo.InvariantCulture, typeName, assembly.FullName), ex);
                        }
                    }

                    if (type == null) {
                        throw new JsonSerializationException("Could not find type '{0}' in assembly '{1}'.".FormatWith(CultureInfo.InvariantCulture, typeName, assembly.FullName));
                    }
                }

                return type;
            } else {
                return Type.GetType(typeName);
            }
        }

        private Type? GetGenericTypeFromTypeName(string typeName, Assembly assembly)
        {
            Type? type = null;
            int openBracketIndex = typeName.IndexOf('[');
            if (openBracketIndex >= 0) {
                string genericTypeDefName = typeName.Substring(0, openBracketIndex);
                Type genericTypeDef = assembly.GetType(genericTypeDefName);
                if (genericTypeDef != null) {
                    List<Type> genericTypeArguments = new List<Type>();
                    int scope = 0;
                    int typeArgStartIndex = 0;
                    int endIndex = typeName.Length - 1;
                    for (int i = openBracketIndex + 1; i < endIndex; ++i) {
                        char current = typeName[i];
                        switch (current) {
                            case '[':
                                if (scope == 0) {
                                    typeArgStartIndex = i + 1;
                                }
                                ++scope;
                                break;
                            case ']':
                                --scope;
                                if (scope == 0) {
                                    string typeArgAssemblyQualifiedName = typeName.Substring(typeArgStartIndex, i - typeArgStartIndex);

                                    StructMultiKey<string?, string> typeNameKey = ReflectionUtils.SplitFullyQualifiedTypeName(typeArgAssemblyQualifiedName);
                                    genericTypeArguments.Add(GetTypeByName(typeNameKey));
                                }
                                break;
                        }
                    }

                    type = genericTypeDef.MakeGenericType(genericTypeArguments.ToArray());
                }
            }

            return type;
        }

        private Type GetTypeByName(StructMultiKey<string?, string> typeNameKey)
        {
            return _typeCache.Get(typeNameKey);
        }
        public override Type BindToType(string? assemblyName, string typeName)
        {
            return GetTypeByName(new StructMultiKey<string?, string>(assemblyName, typeName));
        }
        public
#if HAVE_SERIALIZATION_BINDER_BIND_TO_NAME
        override
#endif
        void BindToName(Type serializedType, out string? assemblyName, out string? typeName)
        {
#if !HAVE_FULL_REFLECTION
            assemblyName = serializedType.GetTypeInfo().Assembly.FullName;
            typeName = serializedType.FullName;
#else
            assemblyName = serializedType.Assembly.FullName;
            typeName = serializedType.FullName;
#endif
        }
    }
}