using System.Windows.Media;

namespace Simula.TeX
{
    // Specifies current graphical parameters used to create boxes.
    internal class TexEnvironment
    {
        // ID of font that was last used.
        private int lastFontId = TexFontUtilities.NoFontId;

        public TexEnvironment(TexStyle style, ITeXFont mathFont, ITeXFont textFont)
            : this(style, mathFont, textFont, null, null)
        {
        }

        private TexEnvironment(TexStyle style, ITeXFont mathFont, ITeXFont textFont, Brush? background, Brush? foreground)
        {
            if (style == TexStyle.Display || style == TexStyle.Text ||
                style == TexStyle.Script || style == TexStyle.ScriptScript)
                Style = style;
            else
                Style = TexStyle.Display;

            MathFont = mathFont;
            TextFont = textFont;
            Background = background;
            Foreground = foreground;
        }

        public TexStyle Style {
            get;
            private set;
        }

        public ITeXFont MathFont {
            get;
            private set;
        }

        public ITeXFont TextFont { get; }

        public Brush? Background {
            get;
            set;
        }

        public Brush? Foreground {
            get;
            set;
        }

        public int LastFontId {
            get { return lastFontId == TexFontUtilities.NoFontId ? MathFont.GetMuFontId() : lastFontId; }
            set { lastFontId = value; }
        }

        public TexEnvironment GetCrampedStyle()
        {
            var newEnvironment = Clone();
            newEnvironment.Style = (int)Style % 2 == 1 ? Style : Style + 1;
            return newEnvironment;
        }

        public TexEnvironment GetNumeratorStyle()
        {
            var newEnvironment = Clone();
            newEnvironment.Style = Style + 2 - 2 * ((int)Style / 6);
            return newEnvironment;
        }

        public TexEnvironment GetDenominatorStyle()
        {
            var newEnvironment = Clone();
            newEnvironment.Style = (TexStyle)(2 * ((int)Style / 2) + 1 + 2 - 2 * ((int)Style / 6));
            return newEnvironment;
        }

        public TexEnvironment GetRootStyle()
        {
            var newEnvironment = Clone();
            newEnvironment.Style = TexStyle.ScriptScript;
            return newEnvironment;
        }

        public TexEnvironment GetSubscriptStyle()
        {
            var newEnvironment = Clone();
            newEnvironment.Style = (TexStyle)(2 * ((int)Style / 4) + 4 + 1);
            return newEnvironment;
        }

        public TexEnvironment GetSuperscriptStyle()
        {
            var newEnvironment = Clone();
            newEnvironment.Style = (TexStyle)(2 * ((int)Style / 4) + 4 + ((int)Style % 2));
            return newEnvironment;
        }

        public TexEnvironment Clone()
        {
            return new TexEnvironment(Style, MathFont, TextFont, Background, Foreground);
        }

        public void Reset()
        {
            Background = null;
            Foreground = null;
        }
    }
}
