using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Dynamic;
using Simula.Scripting.Contexts;
using System.Text;

namespace Simula.Scripting.Types
{
    public class Class : Var
    {
        public Class() { }
        public Class(Type type, dynamic[]? native = null)
        {
            this.ClrType = type;
            this.ClrArguments = native;
        }

        private Type ClrType;
        private dynamic[]? ClrArguments;

        public static Function _create = new Function((self, args) => {
            dynamic expando = new ExpandoObject();
            var instance = Activator.CreateInstance(self.ClrType, System.Reflection.BindingFlags.Default, null, self.ClrArguments, null);
            if (!args[0].FunctionCache.ContainsKey(instance.type))
                args[0].CacheFunction(instance.type, instance.GetType());
            return instance ?? Null.NULL;

            if (instance == null) return Null.NULL;

            expando._instance = instance;
            var dictionary = (IDictionary<string, object>)expando;
            foreach (var field in self.ClrType.GetFields()) {
                dictionary.Add(field.Name, field.GetValue(expando._instance) ?? Null.NULL);
            }

            foreach (var props in self.ClrType.GetProperties()) {
                dictionary.Add(props.Name, props.GetValue(expando._instance) ?? Null.NULL);
            }

            if (dictionary.ContainsKey("_init"))
                expando._init(args);

            return expando;
        }, new List<Pair>() { });

        internal new string type = "sys.class";

        public override string ToString()
        {
            return "class";
        }
    }
}
