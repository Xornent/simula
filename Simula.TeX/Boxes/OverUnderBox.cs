using Simula.TeX.Rendering;
using Simula.TeX.Rendering.Transformations;

namespace Simula.TeX.Boxes
{
    // Box representing other box with delimiter and script box over or under it.
    internal class OverUnderBox : Box
    {
        public OverUnderBox(Box baseBox, Box delimiterBox, Box? scriptBox, double kern, bool over)
            : base()
        {
            BaseBox = baseBox;
            DelimiterBox = delimiterBox;
            ScriptBox = scriptBox;
            Kern = kern;
            Over = over;

            // Calculate dimensions of box.
            Width = baseBox.Width;
            Height = baseBox.Height + (over ? delimiterBox.Width : 0.0) +
                (over && scriptBox != null ? scriptBox.Height + scriptBox.Depth + kern : 0.0);
            Depth = baseBox.Depth + (over ? 0.0 : delimiterBox.Width) +
                (!over && scriptBox != null ? scriptBox.Height + scriptBox.Depth + kern : 0.0);
        }

        public Box BaseBox {
            get;
            private set;
        }

        public Box DelimiterBox {
            get;
            private set;
        }

        public Box? ScriptBox {
            get;
            private set;
        }

        // Kern between delimeter and Script.
        public double Kern {
            get;
            private set;
        }

        // True to draw delimeter and script over base; false to draw under base.
        public bool Over {
            get;
            private set;
        }

        public override void RenderTo(IElementRenderer renderer, double x, double y)
        {
            renderer.RenderElement(BaseBox, x, y);

            if (Over) {
                // Draw delimeter and script boxes over base box.
                var centerY = y - BaseBox.Height - DelimiterBox.Width;
                var translationX = x + DelimiterBox.Width / 2;
                var translationY = centerY + DelimiterBox.Width / 2;

                RenderDelimiter(translationX, translationY);

                // Draw script box as superscript.
                RenderScriptBox(centerY - Kern - ScriptBox!.Depth); // Nullable TODO: This probably needs null checking
            } else {
                // Draw delimeter and script boxes under base box.
                var centerY = y + BaseBox.Depth + DelimiterBox.Width;
                var translationX = x + DelimiterBox.Width / 2;
                var translationY = centerY - DelimiterBox.Width / 2;

                RenderDelimiter(translationX, translationY);

                // Draw script box as subscript.
                RenderScriptBox(centerY + Kern + ScriptBox!.Height); // Nullable TODO: This probably needs null checking
            }

            void RenderDelimiter(double translationX, double translationY)
            {
                var transformations = new Transformation[]
                {
                    new Transformation.Translate(translationX, translationY),
                    new Transformation.Rotate(90)
                };

                renderer.RenderTransformed(
                    DelimiterBox,
                    transformations,
                    -DelimiterBox.Width / 2,
                    -DelimiterBox.Depth + DelimiterBox.Width / 2);
            }

            void RenderScriptBox(double yPosition)
            {
                if (ScriptBox != null) {
                    renderer.RenderElement(ScriptBox, x, yPosition);
                }
            }
        }

        public override int GetLastFontId()
        {
            return TexFontUtilities.NoFontId;
        }
    }
}
