using System.Diagnostics;
using System;
using System.Windows.Controls;
using Simula.Scripting.Syntax;
using Simula.Scripting.Token;
using Simula.Scripting.Contexts;

namespace Simula.Pages
{
    public partial class SourceEditor : UserControl
    {
        public SourceEditor()
        {
            InitializeComponent();
        }

        public void HandleRun(object sender, EventArgs e)
        {
            DynamicRuntime runtime = new DynamicRuntime();
            Simula.Scripting.Dom.Source src = Scripting.Dom.Source.FromSourceCode(editor.Text);
            src.LoadDefinition(runtime);

            Stopwatch sw = new Stopwatch();
            sw.Start();
            src.Run(runtime);
            sw.Stop();
            System.Windows.MessageBox.Show(sw.ElapsedMilliseconds.ToString()+" ms");
        }

        public void ConvertToCs(object sender, EventArgs e)
        {
            Scripting.Build.GenerationContext gc = new Scripting.Build.GenerationContext();
            Simula.Scripting.Dom.Source src = Scripting.Dom.Source.FromSourceCode(editor.Text);

            src.Body.IsParental = true;
            string cscode = src.Body.Generate(gc);
            System.Windows.MessageBox.Show(cscode);

            Scripting.Build.Compiler.Run(cscode);

            this.editor.Text += "\n\n\n";
            string[] lines = cscode.Split('\n');
            foreach (var item in lines) {
                this.editor.Text += "' " + item + "\n";
            }
        }
    }
}
