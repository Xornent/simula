using System;
using System.Collections.Generic;
using Simula.Scripting.Dom;

namespace Simula.Scripting.Utils
{
    public static class Tex
    {
        [FunctionExport("tex", "expression:sys.string", "util")]
        public static Func<dynamic, dynamic[], dynamic> tex = (self, args) => {
            TeX.Controls.FormulaControl ctrl = new TeX.Controls.FormulaControl();
            ctrl.Formula = args[0].ToString();
            ctrl.Margin = new System.Windows.Thickness(16);
            ctrl.InvalidateVisual();

            System.Windows.Window wnd = new System.Windows.Window();
            wnd.Title = "TeX Renderer";
            wnd.SizeToContent = System.Windows.SizeToContent.WidthAndHeight;
            wnd.Margin = new System.Windows.Thickness(16);
            wnd.Content = ctrl;

            wnd.ShowDialog();
            return args[0];
        };
    }
}
