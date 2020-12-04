using System.Diagnostics;
using System;
using System.Windows.Controls;
using Simula.Scripting.Syntax;
using Simula.Scripting.Token;
using Simula.Scripting.Contexts;

namespace Simula.Pages
{
    /// <summary>
    /// SourceEditor.xaml 的交互逻辑
    /// </summary>
    public partial class SourceEditor : UserControl
    {
        public SourceEditor()
        {
            InitializeComponent();
        }

        public void HandleRun(object sender, EventArgs e)
        {
            DynamicRuntime runtime = new DynamicRuntime();
            BlockStatement block = new BlockStatement();
            TokenDocument doc = new TokenDocument();
            doc.Tokenize(this.editor.Text);
            block.Parse(doc.Tokens);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            block.Execute(runtime);
            sw.Stop();
            System.Windows.MessageBox.Show(sw.ElapsedMilliseconds.ToString()+" ms");
        }
    }
}
