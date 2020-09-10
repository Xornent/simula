using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms.Integration;

namespace Simula.DirectX {

    public class InterferelessContainer : WindowsFormsHost {
        public InterferelessContainer(int totalWidth, int totalHeight, System.Windows.Forms.Control control) {
            this.Width = totalWidth;
            this.Height = totalHeight;
            this.Child = control;
            control.Location = new System.Drawing.Point(0, 0);
            control.Width =(int) this.ActualWidth;
            control.Height =(int) this.ActualHeight;
        }
    }
}
