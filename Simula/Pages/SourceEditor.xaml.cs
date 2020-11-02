using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Simula.Pages {
    /// <summary>
    /// SourceEditor.xaml 的交互逻辑
    /// </summary>
    public partial class SourceEditor : UserControl {
        public SourceEditor() {
            InitializeComponent();
        }

        public void HandleRun(object sender, EventArgs e) {
            Scripting.Compilation.RuntimeContext ctx = new Scripting.Compilation.RuntimeContext();

            // Scripting.Compilation.LibraryCompilationUnit lib = new Scripting.Compilation.LibraryCompilationUnit(
            //     Environment.CurrentDirectory + @"\simula.scripting.dll", System.IO.FileMode.Open);
            // lib.Register(ctx);

            Scripting.Compilation.SourceCompilationUnit src = new Scripting.Compilation.SourceCompilationUnit(editor.Text);
            src.Register(ctx);

            src.Run(ctx);
        }
    }
}
