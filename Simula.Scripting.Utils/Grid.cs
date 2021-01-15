using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Simula.Scripting.Dom;
using Simula.Scripting.Types;

namespace Simula.Scripting.Utils
{
    [ClassExport("grid", "util")]
    public class Grid : Types.Var
    {
        public Grid() { }

        private double scaleLeft = -10;
        private double scaleRight = 10;
        private Canvas target = new Canvas();
        private TransformGroup transform = new TransformGroup();
        private TranslateTransform translate = new TranslateTransform();
        private ScaleTransform scale = new ScaleTransform();

        private List<EllipseGeometry> points = new List<EllipseGeometry>();

        [FunctionExport("_init", "scaleLeft:sys.float|scaleRight:sys.float")]
        public static Function _init = new Function((self, args) => {
            self.scaleLeft = args[0];
            self.scaleRight = args[1];
            var canvas = new Canvas();
            canvas.Width = 400; canvas.Height = 400;
            canvas.SizeChanged += (obj, eventArgs) => {
                TranslateTransform trans = new TranslateTransform(canvas.Width / 2, canvas.Height / 2);
                ScaleTransform scale = new ScaleTransform();

                scale.CenterX = 0;
                scale.CenterY = 0;
                scale.ScaleX = canvas.Width / (self.scaleRight - self.scaleLeft);
                scale.ScaleY = -scale.ScaleX;
                TransformGroup group = new TransformGroup();
                group.Children.Add(scale);
                group.Children.Add(trans);

                foreach (UIElement item in canvas.Children) {
                    item.RenderTransform = group;
                }

                self.translate = trans;
                self.scale = scale;
                self.transform = group;
            };

            TranslateTransform trans = new TranslateTransform(canvas.Width / 2, canvas.Height / 2);
            ScaleTransform scale = new ScaleTransform();
            scale.CenterX = 0;
            scale.CenterY = 0;
            scale.ScaleX = canvas.Width / (self.scaleRight - self.scaleLeft);
            scale.ScaleY = -scale.ScaleX;
            TransformGroup group = new TransformGroup();
            group.Children.Add(scale);
            group.Children.Add(trans);

            foreach (UIElement item in canvas.Children) {
                item.RenderTransform = group;
            }

            self.translate = trans;
            self.scale = scale;
            self.transform = group;
            return self;
        }, new List<Pair>());

        [FunctionExport("display", "")]
        public static Function display = new Function((self, args) => {
            Window wnd = new Window();
            wnd.Title = "Figure";
            self.target.Width = 400;
            self.target.Height = 400;
            wnd.SizeToContent = SizeToContent.WidthAndHeight;
            System.Windows.Controls.Grid grid = new System.Windows.Controls.Grid();
            grid.Children.Add(self.target);
            wnd.Content = grid;
            wnd.ShowDialog();
            return self;
        }, new List<Pair>());

        [FunctionExport("addPoint", "point:array")]
        public static Function addPoint = new Function((self, args) => {
            EllipseGeometry ellipse = new EllipseGeometry(new Point((args[0].raw[0]), (args[0].raw[1])), 0.2, 0.2);
            self.points.Add(ellipse);
            ellipse.Transform = self.transform;
            Path path = new Path();
            path.Data = ellipse;
            path.Fill = new SolidColorBrush(Color.FromArgb(128, 0, 1, 0));
            self.target.Children.Add(path);
            return self;
        }, new List<Pair>());

        [FunctionExport("addPoints", "points:array")]
        public static Function addPoints = new Function((self, args) => {
            EllipseGeometry ellipse = new EllipseGeometry(new Point((args[0].raw[0]), (args[0].raw[1])), 0.2, 0.2);
            self.points.Add(ellipse);
            ellipse.Transform = self.transform;
            Path path = new Path();
            path.Data = ellipse;
            path.Fill = new SolidColorBrush(Color.FromArgb(128, 0, 1, 0));
            self.target.Children.Add(path);
            return self;
        }, new List<Pair>());

        public new string type = "util.grid";
    }
}