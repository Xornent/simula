using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Simula.Pages
{

    public partial class Startup : UserControl, IBrowserPage
    {
        public Startup()
        {
            InitializeComponent();
        }

        public void WindowDispose() { }
        public void Signal(string prop, object value) { }
    }

    public class DrawingCanvas : Canvas
    {
        private readonly List<Visual> visuals = new List<Visual>();

        //获取Visual的个数
        protected override int VisualChildrenCount {
            get { return visuals.Count; }
        }

        //获取Visual
        protected override Visual GetVisualChild(int index)
        {
            return visuals[index];
        }

        //添加Visual
        public void AddVisual(Visual visual)
        {
            visuals.Add(visual);

            base.AddVisualChild(visual);
            base.AddLogicalChild(visual);
        }

        //删除Visual
        public void RemoveVisual(Visual visual)
        {
            visuals.Remove(visual);

            base.RemoveVisualChild(visual);
            base.RemoveLogicalChild(visual);
        }

        //命中测试
        public DrawingVisual GetVisual(Point point)
        {
            HitTestResult hitResult = VisualTreeHelper.HitTest(this, point);
            return hitResult.VisualHit as DrawingVisual;
        }

        //使用DrawVisual画Polyline
        public Visual Polyline(PointCollection points, Brush color, double thinkness)
        {
            DrawingVisual visual = new DrawingVisual();
            DrawingContext dc = visual.RenderOpen();
            Pen pen = new Pen(Brushes.Red, 3);
            pen.Freeze();  //冻结画笔，这样能加快绘图速度

            for (int i = 0; i < points.Count - 1; i++) {
                dc.DrawLine(pen, points[i], points[i + 1]);
            }

            dc.Close();
            return visual;
        }
    }

    internal interface IBrowserPage
    {
        void WindowDispose();
        void Signal(string props, object value);
    }
}
