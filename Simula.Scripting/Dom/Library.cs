using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Reflection;
using Simula.Scripting.Contexts;
using System.Dynamic;

namespace Simula.Scripting.Dom
{
    public class Library
    {
        public Library() { }
        public Library(string fileName)
        {
            this.Body = Assembly.LoadFrom(fileName);
            this.Location = fileName;
        }

        public string Location { get; set; }

        public Assembly Body { get; private set; }

        public void LoadDefinition(DynamicRuntime ctx)
        {
            // find the 'module' directive to locate the module the following
            // definition elements should be added to.

            ExpandoObject store = ctx.Store;
            string moduleFullName = "";

            foreach (var types in Body.GetTypes()) {
                var classAttr = types.GetCustomAttribute<ClassExportAttribute>();
                if ( classAttr != null ) {
                    string[] hierachy = classAttr.Module.Split(".");
                    moduleFullName = classAttr.Module;
                    store = ctx.Store;
                    foreach (string str in hierachy) {
                        IDictionary<string, object> dict = (IDictionary<string, object>)store;
                        if (dict.ContainsKey(str)) store = (ExpandoObject)dict[str];
                        else {
                            dynamic obj = new ExpandoObject();
                            obj.fullName = new List<string>() { str };
                            dict[str] = obj;
                            store = obj;
                        }
                    }

                    Types.Class cls = new Types.Class(types)
                    {
                        fullName = { classAttr.FullName },
                        name = classAttr.Name,
                        desc = classAttr.Documentation
                    };

                    IDictionary<string, object> container = (IDictionary<string, object>)store;
                    container[classAttr.Name] = cls;
                    ctx.CacheFunction(classAttr.FullName, types);

                } else {
                    foreach (var function in types.GetFields()) {
                        var funcAttr = function.GetCustomAttribute<FunctionExportAttribute>();
                        store = ctx.Store;
                        if (funcAttr != null) {
                            string[] hierachy = funcAttr.Module.Split(".");
                            moduleFullName = funcAttr.Module;
                            foreach (string str in hierachy) {
                                IDictionary<string, object> dict = (IDictionary<string, object>)store;
                                if (dict.ContainsKey(str)) store = (ExpandoObject)dict[str];
                                else {
                                    dynamic obj = new ExpandoObject();
                                    obj.fullName = new List<string>() { str };
                                    dict[str] = obj;
                                    store = obj;
                                }
                            }

                            Types.Function func = new Types.Function((Func<dynamic, dynamic[], dynamic>)(function.GetValue(null)), funcAttr.Pairs, funcAttr.Returns)
                            {
                                fullName = { funcAttr.FullName },
                                name = funcAttr.Name,
                                desc = funcAttr.Description
                            };

                            IDictionary<string, object> container = (IDictionary<string, object>)store;
                            container[funcAttr.Name] = func;
                        }
                    }
                }
            }
        }
    }
}
