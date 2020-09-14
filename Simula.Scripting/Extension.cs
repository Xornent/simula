using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting {

    public static class Extension {

        public static string JoinString<T>(this List<T> list, string connector) {
            if (list.Count == 0) return "";
            else {
                string s = list[0]?.ToString() ?? "null";
                for(int i = 1; i< list.Count; i++) {
                    s += (connector + (list[i]?.ToString() ?? "null"));
                }
                return s;
            }
        }

        public static void OverflowAdd<TKey, TValue>(this Dictionary<TKey,TValue> dict, TKey key, TValue val) {
            if (dict.ContainsKey(key))
                dict[key] = val;
            else dict.Add(key, val);
        }

        public static void OverflowAddAbstractClass<TKey>(this Dictionary<TKey, Reflection.AbstractClass> dict, TKey key, Reflection.AbstractClass val) {
            if (dict.ContainsKey(key)) {
                val.Conflict = dict[key];
                dict[key] = val;
            } else dict.Add(key, val);
        }

        public static void OverflowAddIdentityClass<TKey>(this Dictionary<TKey, Reflection.IdentityClass> dict, TKey key, Reflection.IdentityClass val) {
            if (dict.ContainsKey(key)) {
                val.Conflict = dict[key];
                dict[key] = val;
            } else dict.Add(key, val);
        }

        public static void OverflowAddInstance<TKey>(this Dictionary<TKey, Reflection.Instance> dict, TKey key, Reflection.Instance val) {
            if (dict.ContainsKey(key)) {
                val.Conflict = dict[key];
                dict[key] = val;
            } else dict.Add(key, val);
        }

        public static void OverflowAddFunction<TKey>(this Dictionary<TKey, Reflection.Function> dict, TKey key, Reflection.Function val) {
            if (dict.ContainsKey(key)) {
                val.Conflict = dict[key];
                dict[key] = val;
            } else dict.Add(key, val);
        }

        public static void OverflowAddModule<TKey>(this Dictionary<TKey, Reflection.Module> dict, TKey key, Reflection.Module val) {
            if (dict.ContainsKey(key)) {
                val.Conflict = dict[key];
                dict[key] = val;
            } else dict.Add(key, val);
        }

        public static void OverflowAddVariable<TKey>(this Dictionary<TKey, Reflection.Variable> dict, TKey key, Reflection.Variable val) {
            if (dict.ContainsKey(key)) {
                val.Conflict = dict[key];
                dict[key] = val;
            } else dict.Add(key, val);
        }
    }
}
