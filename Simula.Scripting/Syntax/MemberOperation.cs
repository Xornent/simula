using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Syntax {
   
    public class MemberOperation : OperatorStatement {

        // 我们将 A.B 的 . 号代表的运算成为成员运算. 它表示一个大的(分类, 对象 单元)中包含一个
        // 小的对象, 这个运算返回被包含的对象.

        // 在实现之前, 我们首先确定那些对象和能包含其他对象的. 显然, module 和 任意的类型
        // 的实例都可以包含对象. 而除此之外, 一个抽象的类(class, AbstractClass), 和特化的类 
        // (function, IdentityClass) 则不能包含其他对象.

        // 我们允许编写者定义重名的类和模块, 比如说, 你可以定义这样的程序

        // [FILE A.s]
        //
        // module a
        // def class c{}
        //     ....
        // end

        // [FILE B.s]
        //
        // module a.c
        // def func foo()
        //     ....
        // end

        // 当两个程序文件一起被加载时, 我们就能发现名称 a.c 既可以指一个抽象类, 也可以指一个模块, 此时
        // 返回类型却要么是一个类, 要么是一个模块, 如果返回抽象类, 我们定义在模块中的 func foo() 就永远
        // 无法访问了. 我们因此需要定义另一个版本的 GetMember, 它优先返回 module 而不是 class. 我们将它
        // 定义在 GetMemberModulePrior 中. 为了方便起见, 我们禁止与 module 同名的 func 和 var.

        public override dynamic Operate(Compilation.RuntimeContext ctx) {
            if (this.Left == null) return Type.Global.Null;
            if (this.Right == null) return Type.Global.Null;

            dynamic evalLeft = this.Left.Operate(ctx);
            string member = ((SelfOperation)this.Right).Self;

            if(evalLeft is Type.Var) {
                Reflection.Variable varia = new Reflection.Variable() { Object = evalLeft };
                return varia.GetMember(member);
            } else if(evalLeft is Reflection.Variable) {
                return ((Reflection.Variable)evalLeft).GetMember(member);
            } else if(evalLeft is Reflection.Instance) {
                return ((Reflection.Instance)evalLeft).GetMember(member);
            } else if(evalLeft is Reflection.Module) {
                return ((Reflection.Module)evalLeft).GetMemberModulePrior(member);
            }

            return Type.Global.Null;
        }
    }
}
