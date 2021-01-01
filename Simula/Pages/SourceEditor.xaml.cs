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
            src.Body.Execute(runtime);
            sw.Stop();
            System.Windows.MessageBox.Show(sw.ElapsedMilliseconds.ToString()+" ms");
        }
    }
}
