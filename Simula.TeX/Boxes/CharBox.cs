using Simula.TeX.Exceptions;
using Simula.TeX.Rendering;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Simula.TeX.Boxes
{
    // Box representing single character.
    internal class CharBox : Box
    {
        public CharBox(TexEnvironment environment, CharInfo charInfo)
            : base(environment)
        {
            Character = charInfo;
            Width = charInfo.Metrics.Width;
            Height = charInfo.Metrics.Height;
            Depth = charInfo.Metrics.Depth;
            Italic = charInfo.Metrics.Italic;
        }

        public CharInfo Character {
            get;
            private set;
        }

        internal GlyphRun GetGlyphRun(double scale, double x, double y)
        {
            var typeface = Character.Font;
            var characterInt = (int)Character.Character;
            if (!typeface.CharacterToGlyphMap.TryGetValue(characterInt, out var glyphIndex)) {
                var fontName = typeface.FamilyNames.Values.First();
                var characterHex = characterInt.ToString("X4");
                throw new TexCharacterMappingNotFoundException(
                    $"The {fontName} font does not support '{Character.Character}' (U+{characterHex}) character.");
            }
#pragma warning disable CS0618
            var glyphRun = new GlyphRun(typeface, 0, false, Character.Size * scale,
                new ushort[] { glyphIndex }, new Point(x * scale, y * scale),
                new double[] { typeface.AdvanceWidths[glyphIndex] }, null, null, null, null, null, null);
            return glyphRun;
        }

        public override void RenderTo(IElementRenderer renderer, double x, double y)
        {
            var color = Foreground ?? Brushes.Black;
            renderer.RenderGlyphRun(scale => GetGlyphRun(scale, x, y), x, y, color);
        }

        public override int GetLastFontId()
        {
            return Character.FontId;
        }
    }
}
