using System.Windows;
using System.Windows.Media;

namespace Simula.TeX.Controls
{
    public class VisualContainerElement : FrameworkElement
    {
        private DrawingVisual? visual;

        public VisualContainerElement()
            : base()
        {
            visual = null;
        }

        public DrawingVisual? Visual {
            get { return visual; }
            set {
                RemoveVisualChild(visual);
                visual = value;
                AddVisualChild(visual);

                InvalidateMeasure();
                InvalidateVisual();
            }
        }

        protected override int VisualChildrenCount {
            get { return 1; }
        }

        protected override Visual? GetVisualChild(int index)
        {
            return visual;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (visual != null)
                return visual.ContentBounds.Size;
            return base.MeasureOverride(availableSize);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            return base.ArrangeOverride(finalSize);
        }

        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);
        }
    }
}
