using Simula.Editor.CodeCompletion;
using Simula.Editor.Document;
using Simula.Editor.Editing;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Simula.Scripting.Contexts.Data
{
    public class KeywordData : ICompletionData
    {
        public static List<KeywordData> Registry = new List<KeywordData>()
        {
            new KeywordData("if") // 0
            {
                Definition = "if (...)",
                Documentation = "if 逻辑块接受一个其后跟随的表达式, 计算出其等效的布尔类型的值. 如果该值为真, 则相应 if 块中的代码行是有效的, 反之, 它们是无效的"
            }, new KeywordData("eif") // 1
            {
                Definition = "eif (...)",
                Documentation = "eif 逻辑块接受一个其后跟随的表达式, 计算出其等效的布尔类型的值. 在这个 eif 块从属的 if 块和之前的所有 eif 的表达式值均为 false 时, 如果该值为真, 则相应 eif 块中的代码行是有效的, 反之, 它们是无效的"
            }, new KeywordData("else") // 2
            {
                Definition = "else",
                Documentation = "如果 else 逻辑块前的所有 if 和 eif 表达式都无法执行, else 表达式中的代码段是有效的, 反之是无效的"
            }, new KeywordData("end") // 3
            {
                Definition = "end",
                Documentation = "结束一个代码块. if, eif, else, for, while, def 语句均可创建一个代码块, 它们需要以 end 结尾"
            }, new KeywordData("option") // 4
            {
                Definition = "option ... [...]",
                Documentation = "指示编译器, 解释器, 或者命令行窗口执行特定的选项. 从以下的列表项中选取支持的命令语句: \n" +
                "log         [on, off] \n" +
                "override    [on, off] \n" +
                "warning     [on, off] [id] \n" +
                "error       [on, off] [id] "
            }, new KeywordData("use") // 5
            {
                Definition = "use ...",
                Documentation = "从已注册的全局中引用一个包的所有内容"
            }, new KeywordData("module") // 6
            {
                Definition = "module ...",
                Documentation = "定义这个编译单元中所有内容从属在某个包下. 如果一个编译单元没有一个 module 语句, 它被默认定义在全局变量中, 如果一个编译单元有超过一个 module 语句, 只有从上至下的第一个是有效的"
            }, new KeywordData("def") // 7
            {
                Definition = "[expose, hidden] def class ... [: ...] [derives ...] \n" +
                "[expose, hidden] def func ... ([..., ...]) \n" +
                "[expose, hidden] def var ... = ...",
                Documentation = "定义一个类, 函数, 或变量的开始"
            }, new KeywordData("class") // 8
            {
                Definition = "[expose, hidden] def class ... [: ...] [derives ...]",
                Documentation = "定义一个类的开始"
            }, new KeywordData("func") // 9
            {
                Definition = "[expose, hidden] def func ... ([..., ...])",
                Documentation = "定义一个函数的开始"
            }, new KeywordData("var") // 10
            {
                Definition = "[expose, hidden] def var ... = ...",
                Documentation = "定义一个变量的开始"
            }, new KeywordData("expose") // 11
            {
                Definition = "expose def ...",
                Documentation = "全局可见修饰符. 在默认情况下, 一个成员只在定义它的包中可见, 而这个修饰符定义了需要在其它包中同样可见的导出对象"
            }, new KeywordData("hidden") // 12
            {
                Definition = "hidden def ...",
                Documentation = "默认情况下, 一个成员只在定义它的包中可见"
            }, new KeywordData("label") // 13
            {
                Definition = "label ...",
                Documentation = "定义一个标签, 它含有一个代码块. 在程序执行到 go 跳转指令时, 执行标签中的所有语句, 再回到 go 定义的位置."
            }, new KeywordData("pass") // 14
            {
                Definition = "pass",
                Documentation = "跳出正在执行的代码块, 或者是一个循环的一轮, 如果当前不在任何一个代码块中, 就终止执行程序"
            }, new KeywordData("go") // 15
            {
                Definition = "go ...",
                Documentation = "跳转到指定的标签, 如果标签没有定义, 本语句是无效的"
            }, new KeywordData("return") // 16
            {
                Definition = "return ...",
                Documentation = "跳出当前的函数, 并可选的返回一个值. 如果当前不在函数块中, 本语句是无效的"
            }, new KeywordData("break") // 17
            {
                Definition = "break",
                Documentation = "跳出当前的循环. 如果当前不在循环块中, 本语句是无效的"
            }, new KeywordData("while") // 18
            {
                Definition = "while (...)",
                Documentation = "当检测语句的值为 true 时, 执行代码块中的语句, 并进行下一次检测"
            }, new KeywordData("iter") // 19
            {
                Definition = "iter ... in ... [at ...]",
                Documentation = "在一个枚举器中遍历枚举值, 可选的获取此时在枚举器中的位置, 并执行相应的代码段"
            }, new KeywordData("in") // 20
            {
                Definition = "iter ... in ... [at ...]",
                Documentation = "声明枚举器"
            }, new KeywordData("at") // 21
            {
                Definition = "iter ... in ... [at ...]",
                Documentation = "获取枚举器坐标"
            }, new KeywordData("continue") // 22
            {
                Definition = "continue",
                Documentation = "跳出当前一轮循环并进入下一轮"
            }, new KeywordData("derives") // 23
            {
                Definition = "[expose, hidden] def class ... derives ...",
                Documentation = "声明类型的属性类标记"
            }
        };

        public KeywordData(string text)
        {
            this.Text = text ?? "";
        }

        public char Image {
            get { return '\ue910'; }
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

                Editor.TextEditor def = new Editor.TextEditor();
                def.FontSize = 13;
                def.SyntaxHighlighting = Simula.Editor.Highlighting.HighlightingManager.Instance.GetDefinition("Simula");
                def.IsReadOnly = true;
                def.Text = Definition;
                def.WordWrap = true;

                TextBlock doc = new TextBlock();
                doc.FontFamily = new FontFamily(SimulaTextEditor.fontFamily);
                def.FontFamily = new FontFamily(SimulaTextEditor.fontFamily);
                doc.FontSize = 13;
                doc.LineHeight = 17;
                doc.Text = Documentation;
                doc.Margin = new System.Windows.Thickness(0, 8, 0, 0);
                doc.TextWrapping = System.Windows.TextWrapping.Wrap;

                cp.Children.Add(def);
                if (!string.IsNullOrEmpty(Documentation))
                    cp.Children.Add(doc);

                scroll.Content = cp;
                border.Child = scroll;
                return contain;
            }
        }

        public double Priority => 0;

        public void Complete(TextArea textArea, ISegment completionSegment,
            EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment, this.Text);
        }
    }

    public class FunctionData : ICompletionData
    {
        public FunctionData(string name, string definition, string doc)
        {
            this.Text = name ?? "";
            this.Definition = definition;
            this.Documentation = doc;
        }

        public char Image {
            get { return '\ue943'; }
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

                Editor.TextEditor def = new Editor.TextEditor();
                def.FontSize = 13;
                def.SyntaxHighlighting = Simula.Editor.Highlighting.HighlightingManager.Instance.GetDefinition("Simula");
                def.IsReadOnly = true;
                def.Text = Definition;
                def.WordWrap = true;

                TextBlock doc = new TextBlock();
                doc.FontFamily = new FontFamily(SimulaTextEditor.fontFamily);
                def.FontFamily = new FontFamily(SimulaTextEditor.fontFamily);
                doc.FontSize = 13;
                doc.LineHeight = 17;
                doc.Text = Documentation;
                doc.Margin = new System.Windows.Thickness(0, 8, 0, 0);
                doc.TextWrapping = System.Windows.TextWrapping.Wrap;

                cp.Children.Add(def);
                if (!string.IsNullOrEmpty(Documentation))
                    cp.Children.Add(doc);

                scroll.Content = cp;
                border.Child = scroll;
                return contain;
            }
        }

        public double Priority => 0;

        public void Complete(TextArea textArea, ISegment completionSegment,
            EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment, this.Text);
        }
    }

    public class ClassData : ICompletionData
    {
        public ClassData(string name, string definition, string doc)
        {
            this.Text = name ?? "";
            this.Definition = definition;
            this.Documentation = doc;
        }

        public char Image {
            get { return '\ue82d'; }
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

                Editor.TextEditor def = new Editor.TextEditor();
                def.FontSize = 13;
                def.SyntaxHighlighting = Simula.Editor.Highlighting.HighlightingManager.Instance.GetDefinition("Simula");
                def.IsReadOnly = true;
                def.Text = Definition;
                def.WordWrap = true;

                TextBlock doc = new TextBlock();
                doc.FontFamily = new FontFamily(SimulaTextEditor.fontFamily);
                def.FontFamily = new FontFamily(SimulaTextEditor.fontFamily);
                doc.FontSize = 13;
                doc.LineHeight = 17;
                doc.Text = Documentation;
                doc.Margin = new System.Windows.Thickness(0, 8, 0, 0);
                doc.TextWrapping = System.Windows.TextWrapping.Wrap;

                cp.Children.Add(def);
                if (!string.IsNullOrEmpty(Documentation))
                    cp.Children.Add(doc);

                scroll.Content = cp;
                border.Child = scroll;
                return contain;
            }
        }

        public double Priority => 0;

        public void Complete(TextArea textArea, ISegment completionSegment,
            EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment, this.Text);
        }
    }

    public class LocalData : ICompletionData
    {
        public LocalData(string name, string definition, string doc)
        {
            this.Text = name ?? "";
            this.Definition = definition;
            this.Documentation = doc;
        }

        public char Image {
            get { return '\uf158'; }
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

                Editor.TextEditor def = new Editor.TextEditor();
                def.FontSize = 13;
                def.SyntaxHighlighting = Simula.Editor.Highlighting.HighlightingManager.Instance.GetDefinition("Simula");
                def.IsReadOnly = true;
                def.Text = Definition;
                def.WordWrap = true;

                TextBlock doc = new TextBlock();
                doc.FontFamily = new FontFamily(SimulaTextEditor.fontFamily);
                def.FontFamily = new FontFamily(SimulaTextEditor.fontFamily);
                doc.FontSize = 13;
                doc.LineHeight = 17;
                doc.Text = Documentation;
                doc.Margin = new System.Windows.Thickness(0, 8, 0, 0);
                doc.TextWrapping = System.Windows.TextWrapping.Wrap;

                cp.Children.Add(def);
                if (!string.IsNullOrEmpty(Documentation))
                    cp.Children.Add(doc);

                scroll.Content = cp;
                border.Child = scroll;
                return contain;
            }
        }

        public double Priority => 0;

        public void Complete(TextArea textArea, ISegment completionSegment,
            EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment, this.Text);
        }
    }

    public class ContainerData : ICompletionData
    {
        public ContainerData(string name, string definition, string doc)
        {
            this.Text = name ?? "";
            this.Definition = definition;
            this.Documentation = doc;
        }

        public char Image {
            get { return '\uecaa'; }
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

                Editor.TextEditor def = new Editor.TextEditor();
                def.FontSize = 13;
                def.SyntaxHighlighting = Simula.Editor.Highlighting.HighlightingManager.Instance.GetDefinition("Simula");
                def.IsReadOnly = true;
                def.Text = Definition;
                def.WordWrap = true;

                TextBlock doc = new TextBlock();
                doc.FontFamily = new FontFamily(SimulaTextEditor.fontFamily);
                def.FontFamily = new FontFamily(SimulaTextEditor.fontFamily);
                doc.FontSize = 13;
                doc.LineHeight = 17;
                doc.Text = Documentation;
                doc.Margin = new System.Windows.Thickness(0, 8, 0, 0);
                doc.TextWrapping = System.Windows.TextWrapping.Wrap;

                cp.Children.Add(def);
                if (!string.IsNullOrEmpty(Documentation))
                    cp.Children.Add(doc);

                scroll.Content = cp;
                border.Child = scroll;
                return contain;
            }
        }

        public double Priority => 0;

        public void Complete(TextArea textArea, ISegment completionSegment,
            EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment, this.Text);
        }
    }
}
