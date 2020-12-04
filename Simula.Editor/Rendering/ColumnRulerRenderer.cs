
using Simula.Editor.Utils;
using System;
using System.Windows;
using System.Windows.Media;

namespace Simula.Editor.Rendering
{
    /// <summary>
    /// Renders a ruler at a certain column.
    /// </summary>
    internal sealed class ColumnRulerRenderer : IBackgroundRenderer
    {
        private Pen pen;
        private int column;
        private readonly TextView textView;

        public static readonly Color DefaultForeground = Colors.LightGray;

        public ColumnRulerRenderer(TextView textView)
        {
            if (textView == null)
                throw new ArgumentNullException("textView");

            pen = new Pen(new SolidColorBrush(DefaultForeground), 1);
            pen.Freeze();
            this.textView = textView;
            this.textView.BackgroundRenderers.Add(this);
        }

        public KnownLayer Layer {
            get { return KnownLayer.Background; }
        }

        public void SetRuler(int column, Pen pen)
        {
            if (this.column != column) {
                this.column = column;
                textView.InvalidateLayer(Layer);
            }
            if (this.pen != pen) {
                this.pen = pen;
                textView.InvalidateLayer(Layer);
            }
        }

        public void Draw(TextView textView, System.Windows.Media.DrawingContext drawingContext)
        {
            if (column < 1) return;
            double offset = textView.WideSpaceWidth * column;
            Size pixelSize = PixelSnapHelpers.GetPixelSize(textView);
            double markerXPos = PixelSnapHelpers.PixelAlign(offset, pixelSize.Width);
            markerXPos -= textView.ScrollOffset.X;
            Point start = new Point(markerXPos, 0);
            Point end = new Point(markerXPos, Math.Max(textView.DocumentHeight, textView.ActualHeight));

            drawingContext.DrawLine(pen, start, end);
        }
    }
}
