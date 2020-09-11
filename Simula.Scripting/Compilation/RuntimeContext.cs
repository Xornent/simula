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

        public Dictionary<string, Reflection.Module> Modules = new Dictionary<string, Reflection.Module>();

        public Stack<TemperaryContext> CallStack = new Stack<TemperaryContext>();

        // 在此处, 或者是任意一个返回值的 Statement 中, 返回的值是 dynamic? . 即如果遇到所有的异常, 
        // 统一返回 C# 中定义的 null 值. 反之, 则返回如下类型中的一种:

        // 1.  Reflection.AbstractClass 如果对象是一个未编译的抽象类
        // 2.  Reflection.IdentityClass 如果对象是一个未编译的特化类
        // 3.  Reflection.Instance 如果对象是一个未编译的实例
        // 4.  Reflection.Module 如果对象是一个模块或子模块
        // 5.  Reflection.Variable 如果对象是一个已编译的命名语言内对象(包括抽象类, 函数, 和基础类型)
        // 6.  Type.Var 如果对象是匿名语言内对象

        public dynamic? GetMember(string name) {

            // 因为这个函数返回可调用的对象, 所以我们断言它一定是命名对象, 而不是匿名对象.
            // 我们在运算符嵌套中会使用匿名对象.

            // 先从调用栈的顶层(如果有)寻找对象.

            if (CallStack.Count > 0) {
                var current = CallStack.Peek();
                if (current.Classes.ContainsKey(name)) return current.Classes[name];
            }

            return null;
        }
    }
}
