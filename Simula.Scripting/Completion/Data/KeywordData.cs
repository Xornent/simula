using Simula.Editor.CodeCompletion;
using Simula.Editor.Document;
using Simula.Editor.Editing;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Simula.Scripting.Completion.Data {

    class KeywordData : ICompletionData {

        public static List<KeywordData> Registry = new List<KeywordData>()
        {
             new KeywordData("if")
            {
                Definition = "if <bool> ...",
                Documentation = "if 逻辑块接受一个其后跟随的表达式, 计算出其等效的布尔类型的值. 如果该值为真, 则相应 if 块中的代码行是有效的, 反之, 它们是无效的"
            }, new KeywordData("eif")
            {
                Definition = "eif <bool> ...",
                Documentation = "eif 逻辑块接受一个其后跟随的表达式, 计算出其等效的布尔类型的值. 在这个 eif 块从属的 if 块和之前的所有 eif 的表达式值均为 false 时, 如果该值为真, 则相应 eif 块中的代码行是有效的, 反之, 它们是无效的"
            }, new KeywordData("else")
            {
                Definition = "else ...",
                Documentation = "如果 else 逻辑块前的所有 if 和 eif 表达式都无法执行, else 表达式中的代码段是有效的, 反之是无效的"
            }, new KeywordData("end")
            {
                Definition = "end",
                Documentation = "结束一个代码块. if, eif, else, for, while, def 语句均可创建一个代码块, 它们需要以 end 结尾"
            }, new KeywordData("option")
            {
                Definition = "option [选项名称] [选项值]",
                Documentation = "指示编译器, 解释器, 或者命令行窗口执行特定的选项. 从以下的列表项中选取支持的命令语句: \n" +
                "log         [on, off] \n" +
                "override    [on, off] \n" +
                "warning     [on, off] [id] \n" +
                "error       [on, off] [id] "
            }, new KeywordData("use")
            {
                Definition = "use [包全名]",
                Documentation = "从已注册的全局中引用一个包的所有内容"
            }, new KeywordData("module")
            {
                Definition = "module [包全名]",
                Documentation = "定义这个编译单元中所有内容从属在某个包下. 如果一个编译单元没有一个 module 语句, 它被默认定义在全局变量中, 如果一个编译单元有超过一个 module 语句, 只有从上至下的第一个是有效的"
            }, new KeywordData("def")
            {
                Definition = "[expose, hidden] def class 类型名<[分类型参数, ...]> [: 基类型] \n" +
                "[expose, hidden] def const 常量名 [= ...] \n" +
                "[expose, hidden] def func 函数名 ([函数参数 ...])",
                Documentation = "定义一个类, 函数, 或常量的开始"
            }, new KeywordData("class")
            {
                Definition = "[expose, hidden] def class 类型名<[分类型参数, ...]> [: 基类型]",
                Documentation = "定义一个类的开始"
            }, new KeywordData("func")
            {
                Definition = "[expose, hidden] def func 函数名 ([函数参数 ...])",
                Documentation = "定义一个函数的开始"
            }, new KeywordData("const")
            {
                Definition = "[expose, hidden] def const 常量名 [= ...] ",
                Documentation = "定义一个常量的开始"
            }, new KeywordData("expose")
            {
                Definition = "expose def ...",
                Documentation = "全局可见修饰符. 在默认情况下, 一个成员只在定义它的包中可见, 而这个修饰符定义了需要在其它包中同样可见的导出对象"
            }, new KeywordData("hidden")
            {
                Definition = "hidden def ...",
                Documentation = "默认情况下, 一个成员只在定义它的包中可见"
            }, new KeywordData("label")
            {
                Definition = "label 标签名",
                Documentation = "定义一个标签, 它含有一个代码块. 在程序执行到 go 跳转指令时, 执行标签中的所有语句, 再回到 go 定义的位置."
            }, new KeywordData("pass")
            {
                Definition = "pass",
                Documentation = "跳出正在执行的代码块, 或者是一个循环的一轮, 如果当前不在任何一个代码块中, 就终止执行程序"
            }, new KeywordData("go")
            {
                Definition = "go 标签名",
                Documentation = "跳转到指定的标签, 如果标签没有定义, 本语句是无效的"
            }, new KeywordData("return")
            {
                Definition = "return [返回值]",
                Documentation = "跳出当前的函数, 并可选的返回一个值. 如果当前不在函数块中, 本语句是无效的"
            }, new KeywordData("break")
            {
                Definition = "break",
                Documentation = "跳出当前的循环. 如果当前不在循环块中, 本语句是无效的"
            }, new KeywordData("while")
            {
                Definition = "while 检测语句 ...",
                Documentation = "当检测语句的值为 true 时, 执行代码块中的语句, 并进行下一次检测"
            }, new KeywordData("enum")
            {
                Definition = "enum [枚举值] in [枚举器] [at <dimension<n>>]",
                Documentation = "在一个枚举器中遍历枚举值, 可选的获取此时在枚举器中的位置, 并执行相应的代码段"
            }, new KeywordData("in")
            {
                Definition = "enum [枚举值] in [枚举器] [at <dimension<n>>]",
                Documentation = "声明枚举器"
            }, new KeywordData("at")
            {
                Definition = "enum [枚举值] in [枚举器] [at <dimension<n>>]",
                Documentation = "获取枚举器坐标"
            }, new KeywordData("bool")
            {
                Definition = "expose def class bool<> : object<>",
                Documentation = "表示一个布尔值 (true 或者 false)"
            }, new KeywordData("string")
            {
                Definition = "expose def class string<> : dimension<1>",
                Documentation = "将文本表示为 UTF-16 代码单元的序列"
            }, new KeywordData("char")
            {
                Definition = "expose def class char<> : object<>",
                Documentation = "UTF-16 代码单元"
            }
        };

        public KeywordData(string text) {
            this.Text = text;
        }

        public char Image {
            get { return '\ue1da'; }
        }

        public string Text { get; private set; }
        public string Definition { get; set; }
        public string Documentation { get; set; }

        public object Content {
            get { return this.Text; }
        }

        public object Description {
            get {
                Border border = new Border();
                Grid contain = new Grid();
                border.SnapsToDevicePixels = true;
                border.BorderThickness = new System.Windows.Thickness(1, 1, 1, 1);
                border.Background = Brushes.White;
                border.CornerRadius = new System.Windows.CornerRadius(2);
                border.Margin = new System.Windows.Thickness(2, 2, 6, 6);
                border.BorderBrush = new SolidColorBrush(Color.FromArgb(230, 230, 230, 230));
                border.Effect = new DropShadowEffect()
                {
                    ShadowDepth = 1,
                    BlurRadius = 4,
                    Opacity = 0.15
                };
                contain.Children.Add(border);

                ScrollViewer scroll = new ScrollViewer();
                scroll.SnapsToDevicePixels = true;
                scroll.MaxWidth = 400;
                scroll.MaxHeight = 400;
                StackPanel cp = new StackPanel();
                cp.Margin = new System.Windows.Thickness(4);

                cp.Orientation = Orientation.Vertical;

                SimulaTextEditor def = new SimulaTextEditor();
                def.IsReadOnly = true;
                def.Text = Definition;
                def.WordWrap = true;

                TextBlock doc = new TextBlock();
                doc.FontFamily = new FontFamily("Consolas, Simsun");
                doc.FontSize = 13;
                doc.Text = Documentation;
                doc.Margin = new System.Windows.Thickness(0, 8, 0, 0);
                doc.TextWrapping = System.Windows.TextWrapping.Wrap;

                cp.Children.Add(def);
                cp.Children.Add(doc);

                scroll.Content = cp;
                border.Child = scroll;
                return contain;
            }
        }

        public double Priority => 0;

        public void Complete(TextArea textArea, ISegment completionSegment,
            EventArgs insertionRequestEventArgs) {
            textArea.Document.Replace(completionSegment, this.Text);
        }
    }
}
