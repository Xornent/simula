using Simula.TeX.Rendering;
using System;

namespace Simula.TeX.Boxes
{
    // Box containing vertical stack of child boxes.
    internal class VerticalBox : Box
    {
        private double leftMostPos = double.MaxValue;
        private double rightMostPos = double.MinValue;

        public VerticalBox(Box box, double rest, TexAlignment alignment)
            : this()
        {
            Add(box);
            if (alignment == TexAlignment.Center) {
                var strutBox = new StrutBox(0, rest / 2, 0, 0);
                base.Add(0, strutBox);
                Height += rest / 2;
                Depth += rest / 2;
                base.Add(strutBox);
            } else if (alignment == TexAlignment.Top) {
                Depth += rest;
                base.Add(new StrutBox(0, rest, 0, 0));
            } else if (alignment == TexAlignment.Bottom) {
                Height += rest;
                base.Add(0, new StrutBox(0, rest, 0, 0));
            }
        }

        public VerticalBox()
            : base()
        {
        }

        public override void Add(Box box)
        {
            base.Add(box);

            if (Children.Count == 1) {
                Height = box.Height;
                Depth = box.Depth;
            } else {
                Depth += box.Height + box.Depth;
            }
            RecalculateWidth(box);
        }

        public override void Add(int position, Box box)
        {
            base.Add(position, box);

            if (position == 0) {
                Depth += box.Depth + Height;
                Height = box.Height;
            } else {
                Depth += box.Height + box.Depth;
            }
            RecalculateWidth(box);
        }

        private void RecalculateWidth(Box box)
        {
            leftMostPos = Math.Min(leftMostPos, box.Shift);
            rightMostPos = Math.Max(rightMostPos, box.Shift + (box.Width > 0 ? box.Width : 0));
            Width = rightMostPos - leftMostPos;
        }

        public override void RenderTo(IElementRenderer renderer, double x, double y)
        {
            var curY = y - Height;
            foreach (var child in Children) {
                curY += child.Height;
                renderer.RenderElement(child, x + child.Shift - leftMostPos, curY);
                curY += child.Depth;
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
