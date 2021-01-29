using Simula.TeX.Utils;

namespace Simula.TeX.Atoms
{
    // Atom representing single character in specific text style.
    internal class CharAtom : CharSymbol
    {
        public CharAtom(SourceSpan? source, char character, string? textStyle = null)
            : base(source)
        {
            TextStyle = textStyle;
            Character = character;
        }

        public char Character { get; }

        // Null means default text style.
        public string? TextStyle { get; }

        public override ITeXFont GetStyledFont(TexEnvironment environment) =>
            TextStyle == TexUtilities.TextStyleName ? environment.TextFont : base.GetStyledFont(environment);

        protected override Result<CharInfo> GetCharInfo(ITeXFont texFont, TexStyle style) =>
            TextStyle == null
                ? texFont.GetDefaultCharInfo(Character, style)
                : texFont.GetCharInfo(Character, TextStyle, style);

        public override Result<CharFont> GetCharFont(ITeXFont texFont) =>
            // Style is irrelevant here.
            GetCharInfo(texFont, TexStyle.Display).Map(ci => ci.GetCharacterFont());
    }
}
