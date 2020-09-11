using Simula.Scripting.Reflection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Compilation {
   
    public class TemperaryContext {

        // [IMP] 虽然我们在语言的现行内置定义中并没有定义以模块为对象, 但我们有可能会在接下来的版本中
        //       实现它. (如果有, 参见 Type.Module 内置类)

        public Dictionary<string, Module> SubModules = new Dictionary<string, Module>();
        public Dictionary<string, AbstractClass> Classes = new Dictionary<string, AbstractClass>();
        public Dictionary<string, IdentityClass> IdentityClasses = new Dictionary<string, IdentityClass>();
        public Dictionary<string, Function> Functions = new Dictionary<string, Function>();
        public Dictionary<string, Instance> Instances = new Dictionary<string, Instance>();
        public Dictionary<string, Variable> Variables = new Dictionary<string, Variable>();
    }
}
