using System.Windows.Media;

namespace Simula.TeX
{
    // Single character togeter with information about font and metrics.
    internal class CharInfo
    {
        public CharInfo(char character, GlyphTypeface font, double size, int fontId, TexFontMetrics metrics)
        {
            Character = character;
            Font = font;
            Size = size;
            FontId = fontId;
            Metrics = metrics;
        }

        public char Character {
            get;
            set;
        }

        public GlyphTypeface Font {
            get;
            set;
        }

        public double Size {
            get;
            set;
        }

        public TexFontMetrics Metrics {
            get;
            set;
        }

        public int FontId {
            get;
        }

        public CharFont GetCharacterFont()
        {
            return new CharFont(Character, FontId);
        }
    }
}
