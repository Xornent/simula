using System.Windows;
using System.Windows.Media;
using Simula.TeX.Rendering;

namespace Simula.TeX.Boxes
{
    // Box representing single character.
    internal class CharBox : Box
    {
        public CharBox(TexEnvironment environment, CharInfo charInfo)
            : base(environment)
        {
            this.Character = charInfo;
            this.Width = charInfo.Metrics.Width;
            this.Height = charInfo.Metrics.Height;
            this.Depth = charInfo.Metrics.Depth;
            this.Italic = charInfo.Metrics.Italic;
        }

        public CharInfo Character
        {
            get;
            private set;
        }

        internal GlyphRun GetGlyphRun(double scale, double x, double y)
        {
            var typeface = this.Character.Font;
            var glyphIndex = typeface.CharacterToGlyphMap[this.Character.Character];
#pragma warning disable CS0618
            var glyphRun = new GlyphRun(typeface, 0, false, this.Character.Size * scale,
                new ushort[] { glyphIndex }, new Point(x * scale, y * scale),
                new double[] { typeface.AdvanceWidths[glyphIndex] }, null, null, null, null, null, null);
#pragma warning restore 
            return glyphRun;
        }

        public override void RenderTo(IElementRenderer renderer, double x, double y)
        {
            var color = this.Foreground ?? Brushes.Black;
            renderer.RenderGlyphRun(scale => this.GetGlyphRun(scale, x, y), x, y, color);
        }

        public override int GetLastFontId()
        {
            return this.Character.FontId;
        }
    }
}
