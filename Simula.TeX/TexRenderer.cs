using Simula.TeX.Boxes;
using Simula.TeX.Rendering;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Simula.TeX
{
    public class TexRenderer
    {
        /// <summary>Default DPI for WPF.</summary>
        private const int DefaultDpi = 96;

        internal TexRenderer(Box box, double scale)
        {
            Box = box;
            Scale = scale;
        }

        public Box Box {
            get;
            set;
        }

        public double Scale {
            get;
            private set;
        }

        public Size RenderSize {
            get {
                return new Size(Box.TotalWidth * Scale, Box.TotalHeight * Scale);
            }
        }

        public double Baseline {
            get {
                return Box.Height / Box.TotalHeight * Scale;
            }
        }

        public void RenderFormulaTo(IElementRenderer renderer, double x, double y)
        {
            renderer.RenderElement(Box, x / Scale, y / Scale + Box.Height);
            renderer.FinishRendering();
        }

        public Geometry RenderToGeometry(double x, double y)
        {
            var geometry = new GeometryGroup();
            var renderer = new GeometryElementRenderer(geometry, Scale);
            RenderFormulaTo(renderer, x, y);
            return geometry;
        }

        private void RenderWithPositiveCoordinates(DrawingVisual visual, double x, double y)
        {
            using (var drawingContext = visual.RenderOpen())
                Render(drawingContext, x, y);

            var bounds = visual.ContentBounds;
            if (bounds.X >= 0 && bounds.Y >= 0) return;

            using (var drawingContext = visual.RenderOpen()) {
                drawingContext.PushTransform(
                    new TranslateTransform(Math.Max(0.0, -bounds.X), Math.Max(0.0, -bounds.Y)));
                Render(drawingContext, x, y);
            }
        }

        public BitmapSource RenderToBitmap(double x, double y, double dpi)
        {
            var visual = new DrawingVisual();
            RenderWithPositiveCoordinates(visual, x, y);

            var bounds = visual.ContentBounds;
            var width = (int)Math.Ceiling((bounds.Right + x) * dpi / DefaultDpi);
            var height = (int)Math.Ceiling((bounds.Bottom + y) * dpi / DefaultDpi);
            var bitmap = new RenderTargetBitmap(width, height, dpi, dpi, PixelFormats.Default);
            bitmap.Render(visual);

            return bitmap;
        }

        public BitmapSource RenderToBitmap(double x, double y) => RenderToBitmap(x, y, DefaultDpi);

        public void Render(DrawingContext drawingContext, double x, double y) =>
            RenderFormulaTo(new WpfElementRenderer(drawingContext, Scale), x, y);
    }
}
