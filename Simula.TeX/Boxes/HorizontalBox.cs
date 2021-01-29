using Simula.TeX.Rendering;
using System;
using System.Windows.Media;

namespace Simula.TeX.Boxes
{
    // Box containing horizontal stack of child boxes.
    internal class HorizontalBox : Box
    {
        private double childBoxesTotalWidth = 0.0;

        public HorizontalBox(Box box, double width, TexAlignment alignment)
            : this()
        {
            var extraWidth = width - box.Width;
            if (alignment == TexAlignment.Center) {
                var strutBox = new StrutBox(extraWidth / 2, 0, 0, 0);
                Add(strutBox);
                Add(box);
                Add(strutBox);
            } else if (alignment == TexAlignment.Left) {
                Add(box);
                Add(new StrutBox(extraWidth, 0, 0, 0));
            } else if (alignment == TexAlignment.Right) {
                Add(new StrutBox(extraWidth, 0, 0, 0));
                Add(box);
            }
        }

        public HorizontalBox(Box box)
            : this()
        {
            Add(box);
        }

        public HorizontalBox(Brush? foreground, Brush? background)
            : base(foreground, background)
        {
        }

        public HorizontalBox()
            : base()
        {
        }

        public override void Add(Box box)
        {
            base.Add(box);

            childBoxesTotalWidth += box.Width;
            Width = Math.Max(Width, childBoxesTotalWidth);
            Height = Math.Max((Children.Count == 0 ? double.NegativeInfinity : Height), box.Height - box.Shift);
            Depth = Math.Max((Children.Count == 0 ? double.NegativeInfinity : Depth), box.Depth + box.Shift);
            Italic = Math.Max((Children.Count == 0 ? double.NegativeInfinity : Italic), box.Italic);
        }

        public override void RenderTo(IElementRenderer renderer, double x, double y)
        {
            var curX = x;
            foreach (var box in Children) {
                renderer.RenderElement(box, curX, y + box.Shift);
                curX += box.Width;
            }
        }

        public override int GetLastFontId()
        {
            var fontId = TexFontUtilities.NoFontId;
            foreach (var child in Children) {
                fontId = child.GetLastFontId();
                if (fontId == TexFontUtilities.NoFontId)
                    break;
            }
            return fontId;
        }
    }
}
