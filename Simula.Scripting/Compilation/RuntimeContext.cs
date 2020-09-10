using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Simula.Scripting.Compilation {

    public class RuntimeContext {
        public RuntimeContext() { }

        public List<CompilationUnit> Registry = new List<CompilationUnit>();

        // 值得注意的是, 对于一个已编译的程序集, 里面的所有抽象类, 特化类, 实例(变量) 和函数都对应一个
        // Variable 对象, 其中 Variable.Object 是一个 Var 类型的量, 这些量的属性是在加载的程序集中的.

        // 而对于一个使用代码即时运行的对象来说, 它的语句没有生成为代码, 所以我们没有一个可以直接使用的
        // 位于程序集中的来源, 只能现场解析不同的内容, 使用反射接口(位于 Simula.Scripting.Reflection 而
        // 不是 System.Reflection) 和抽象语法树执行, 它们被保存在 AbstractClass, IdentityClass, Function, 
        // Module 和 Instance 中.

        // 对于已编译的对象 (从 LiraryCompilationUnit 注册, 后缀名 .scl Simula 已编译库)
        // 对于动态的对象 (从 ObjectCompilationUnit 注册, 后缀名 .sop Simula 对象包 或 SourceCompilationUnit
        //     注册, 后缀名为 .s Simula 源文件) 有区别如下:

        // 表达式                        | 已编译的                      | 未编译的
        // --------------------------------------------------------------------------------------------
        // dimension                     | Class                         | AbstractClass [dimension]
        // dimension<1>                  | Function                      | IdentityClass [dimension<1>]
        // a = dimension<1>()            | Dimension                     | Instance
        // simula                        | 这是 C# 的命名空间            | Module

        // 已编译的对象

        public Dictionary<string, Reflection.Variable> Variables = new Dictionary<string, Reflection.Variable>();

        // 动态的对象

        public Dictionary<string, Reflection.AbstractClass> Classes = new Dictionary<string, Reflection.AbstractClass>();
        public Dictionary<string, Reflection.Module> Modules = new Dictionary<string, Reflection.Module>();
        public Dictionary<string, Reflection.Function> Functions = new Dictionary<string, Reflection.Function>();
        public Dictionary<string, Reflection.IdentityClass> IdentityClasses = new Dictionary<string, Reflection.IdentityClass>();
        public Dictionary<string, Reflection.Instance> Instances = new Dictionary<string, Reflection.Instance>();
    }
}
