using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Contexts
{
    public class TypeInference
    {
        public TypeInference()
        {
            this.Types = new HashSet<string>() { "null" };
            this.Object = null;
        }

        public TypeInference(CompletionRecord obj)
        {
            this.Types = obj.Type;
            this.Object = obj;
        }

        public TypeInference(HashSet<string> type, CompletionRecord? obj)
        {
            this.Types = type;
            this.Object = obj;
        }

        public HashSet<string> Types;
        public CompletionRecord? Object;
    }
    
    public static class HashSetExtension
    {
        public static void AddRange<T>(this HashSet<T> hashset, List<T> list)
        {
            foreach (var item in list) {
                hashset.Add(item);
            }
        }

        public static void AddRange<T>(this HashSet<T> hashset, HashSet<T> addition)
        {
            foreach (var item in addition) {
                hashset.Add(item);
            }
        }

        public static string JoinString(this HashSet<string> stringSet, string connector)
        {
            string str = "";
            int i = 0;
            foreach (var item in stringSet) {
                if (i == 0) {
                    str += item;
                } else {
                    str += (connector + item);
                }
                i++;
            }
            return str;
        }
    }
}
