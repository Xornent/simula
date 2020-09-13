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

    }
}
