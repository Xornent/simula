using Simula.TeX.Rendering;

namespace Simula.TeX.Boxes
{
    // Box representing whitespace.
    internal class StrutBox : Box
    {
        public static StrutBox Empty { get; } = new StrutBox(0, 0, 0, 0);

        public StrutBox(double width, double height, double depth, double shift)
        {
            Width = width;
            Height = height;
            Depth = depth;
            Shift = shift;
        }

        public override void RenderTo(IElementRenderer renderer, double x, double y)
        {
        }

        public override int GetLastFontId()
        {
            return TexFontUtilities.NoFontId;
        }
    }
}
