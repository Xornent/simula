
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

namespace Simula.Editor.Rendering
{
    /// <summary>
    /// Base class for known layers.
    /// </summary>
    internal class Layer : UIElement
    {
        protected readonly TextView textView;
        protected readonly KnownLayer knownLayer;

        public Layer(TextView textView, KnownLayer knownLayer)
        {
            Debug.Assert(textView != null);
            this.textView = textView;
            this.knownLayer = knownLayer;
            Focusable = false;
        }

        protected override GeometryHitTestResult HitTestCore(GeometryHitTestParameters hitTestParameters)
        {
            return null;
        }

        protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
        {
            return null;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            textView.RenderBackground(drawingContext, knownLayer);
        }
    }
}
