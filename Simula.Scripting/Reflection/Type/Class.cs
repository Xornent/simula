using Simula.Scripting.Reflection.Markup;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using _global = Simula.Scripting.Type.Global;
using Simula.Scripting.Compilation;
using _util = Simula.Scripting.Compilation.Utilities;

namespace Simula.Scripting.Type {

    [Expose("class")]
    public class Class : Var {
        public String name = "";
        public System.Type? type;

        internal Class(System.Type classType) {
            this.type = classType;
        }

        public override string ToString() {
            return "class : "+ _util.GetModuleHirachy(type?.Namespace ?? "").Connect(".")+"."+(type?.GetCustomAttribute<ExposeAttribute>()?.Alias ?? "null");
        }

        public Var create_instance(Var[] id, Var[] init) {
            object? obj = create_identity(id).call(init);
            if (obj == null) return _global.Null;
            return (Var)obj;
        }

        public Function create_identity(Var[] id) {
            if (type == null) return new Function(null, null);
            object? obj = Activator.CreateInstance(type);
            if (obj == null) return new Function(null, null);

            if (id.Length > 0) {
                object? func = type.GetMethod("_create")?.Invoke(obj, id);
                if (func == null) return new Function(null, null);
                else return (Function)func;
            } else {
                object? func = type.GetMethod("_create")?.Invoke(obj, null);
                if (func == null) return new Function(null, null);
                else return (Function)func;
            }
        }
    }
}
