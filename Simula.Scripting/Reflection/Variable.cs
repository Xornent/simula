using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;

namespace Simula.Scripting.Reflection {

    public class Variable {
        public string Name = "";
        public string FullName = "";

        // 关于对象实例的注解:

        // 就像我们在反射类的定义中看到的, 一个类型分为抽象类和特化类两种, 而变量的类型只能是特化的类
        // 抽象类的名称形如 abstract_class<int, int>, 有以尖括号包围的类型名限定符, 而特化类将类型名
        // 换成了类型的值, 例如 dimension<3> 而不是 dimension<int>

        // 变量的类型储存在 Type 中, 而它的值是一个 ExpandoObject. 我们约定一个合法的变量拥有如下所述
        // 的预定义的属性. 然而, 假若一个实例没有这样的属性, 也不要终止程序的运行, 而是抛出一个警告和
        // 一个默认的返回值 null. 我们定义 null 的所有调用的结果均为 null.

        // [dynamic] _inherit       : 这个实例的类型继承的类型的实例.
        // [class]   _type          : 这个实例的类型
        // [bool]    _equals        : 这个类型是否与给定的实例相等
        // [bool]    _eval          : 这个类型隐式转换成的 bool 类型的值. 
        // [bool]    _is_value      : 是否是值类型的实例
        // [dynamic] _value         : 值类型的值, 如果不是值类型则没有这个属性

        // 有一些对象除了调用方法之外, 本身还有一个预定义的值属性, 例如, 一个类 int<> 不能完全的被他的
        // 一套函数集所描述, 这些类型被称为值类型, 它们直接由 C#(IL) 接管, 它们分别有:
        
        // BigInteger, float, string, bool, char
        // BigInteger[], float[], string[], bool[], char[]

        // 对于已编译的对象, 我们采用一个继承 Simula.Scripting.Type.Var 的对象表示 Object, 默认情况下其
        // 值为 _Null.

        public Type.Var Object = new Simula.Scripting.Type._Null();
        public Variable? Conflict;
    }
}
