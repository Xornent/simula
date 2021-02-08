﻿using System.Collections.Generic;

namespace Simula.Scripting
{
    public class ScriptException
    {
        public ScriptException(string id)
        {
            Id = id;
        }

        public string Message {
            get {
                (string, string) value;
                bool success = Helper.TryGetValue(Id.ToLower(), out value);
                if (string.IsNullOrEmpty(value.Item1)) return "";
                else return value.Item1;
            }
        }
        public string Id { get; set; }
        public string Help {
            get {
                (string, string) value;
                bool success = Helper.TryGetValue(Id.ToLower(), out value);
                if (string.IsNullOrEmpty(value.Item2)) return "";
                else return value.Item2;
            }
        }

        public static Dictionary<string, (string, string)> Helper = new Dictionary<string, (string, string)>()
        {
            {"ss0001", ("符号和运算符未定义","")},
            {"ss0002", ("一个赋值语句中出现两个等于分隔符", "") },
            {"ss0003", ("use 语句需要跟随一个模块的全名（或者支持的通配符的格式）", "以下示例是许可的：\n```\nuse simula\nuse simula.*\nuse simula.core\n```") },
            {"ss0004", ("引用的包全名和通配符不合法", "包全名是一个合法的单词, 通配符 '*', 或者是由单词或通配符由成员运算符连接的列表. 以下是一些合法的示例\n```\nuse simula\n```\n一个合法的单词表示一个包\n```\nuse *\n```\n指代引用所有加载的可用的包\n```\nuse simula.*.regex\n```\n可以引用类似 simula.text.regex, simula.js.regex 的所有符合通配符的包") },
            {"ss0005", ("module 语句需要跟随一个模块的全名", "以下示例是许可的：\n```\nmodule simula\nmodule simula.core\n```") },
            {"ss0006", ("包全名不合法", "包全名是一个合法的单词, 或者是由单词和成员运算符连接的列表. 以下是一些合法的示例\n```\nmodule simula\n```\n一个合法的单词表示一个包") },
            {"ss0007", ("eif 语句出现在非 if 块下", "")},
            {"ss0008", ("else 语句出现在非 if 或者 eif 块下", "") },
            {"ss0009", ("if, eif 和 while 语句后需要连接返回值为 bool 的表达式项", "") },
            {"ss0010", ("enum 语句出现未预料的子句", "") },
            {"ss0011", ("enum 语句没有出现对应的 in 子句", "") },
            {"ss0012", ("expose, hidden 后期待 func, class, var, def", "") },
            {"ss0013", ("def 后期待 func, class, var", "") },
            {"ss0014", ("def var 应赋予初始值", "") },
            {"ss0015", ("类继承的语法错误", "") },
            {"ss0016", ("def 声明语句的语法错误", "") },
            {"ss0017", ("二元运算符缺少左右的值","") },
            {"ss0018", ("二元运算符的左右参数不能是运算符语句","") },
            {"ss0019", ("当前的运算对象不支持运算", "") },
            {"ss0020", ("当前的运算对象没有相应运算符的定义","") },
            {"ss0021", ("缺少声明类, 函数或变量的定义","") },
            {"ss0022", ("类型的声明必须附带相应的特化参数, 如果没有参数使用空大括号","期望合法的类型特化参数定义:\n```\ndef class a{}\ndef class a{int i1, int i2}\ndef class a{int i, float f} : base{}\ndef class a{int i} : base{1, 5, 3.12, \"value\"}\n```") },
            {"ss0023", ("函数的声明必须跟随一个完整的参数列表","期望合法的参数定义:\n```\ndef func foo()\ndef func foo_with_parameter(int i, float f)\n```") },
            {"ss0024", ("不合法的函数语法", "") },

            {"ss1001", ("调用小括号运算符的左侧不是函数或类型", "") }
        };
    }
}