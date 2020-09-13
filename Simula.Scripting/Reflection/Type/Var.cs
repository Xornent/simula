using System;
using System.Collections.Generic;
using System.Text;
using static Simula.Scripting.Type.Global;

namespace Simula.Scripting.Type {

    // 以下是可实现的运算符重载函数的值

    // 2. 子类型运算符(_create), 子函数运算符(_call)
    // 3. 索引表达式运算符(_index)
    // 4. 指数运算符(_exp)
    // 5. 乘运算符(_multiply), 除运算符(_divide)
    // 6. 取余运算符(_mod)
    // 7. 加运算符(_add), 减运算符(_minus)
    // 8. 位左移运算符(_bitleft), 位右移运算符(_bitright)
    // 9. 比较运算符(_less, _more, _nomore, _noless, _equal, _notequal)
    // 10. 布尔逻辑运算符 (_or, _and)
    // 11. 位逻辑运算符 (_bitor, _bitand, _bitnot)

    public abstract class Var : Reflection.CompiledBase {
        public virtual Function _create() {
            return new Function(this.GetType().GetMethod("_init"));
        }

        public virtual _Null _init() {
            return Null;
        }
    }

    public class _Null : Var {

    }
}
