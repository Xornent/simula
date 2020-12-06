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
            var instance = Activator.CreateInstance(self.ClrType, System.Reflection.BindingFlags.Default, null, self.ClrArgument, null);
            if (!DynamicRuntime.FunctionCache.ContainsKey(instance.type))
                DynamicRuntime.CacheFunction(instance.type, instance.GetType());
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
        });

        internal new string type = "class";

        public static Class typeNull = new Class(typeof(Null));
        public static Class typeClass = new Class(typeof(Class));
        public static Class typeFunc = new Class(typeof(Function));
        public static Class typeSelector = new Class(typeof(Selector));
        public static Class typeInt = new Class(typeof(Integer));
        public static Class typeFloat = new Class(typeof(Float));
        public static Class typeBool = new Class(typeof(Boolean));
        public static Class typeString = new Class(typeof(String));
        public static Class typeArr = new Class(typeof(Array));

        public override string ToString()
        {
            return "class";
        }
    }
}
