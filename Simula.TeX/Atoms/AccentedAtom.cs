using Simula.TeX.Boxes;
using System;

namespace Simula.TeX.Atoms
{
    // Atom representing base atom with accent above it.
    internal class AccentedAtom : Atom
    {
        public AccentedAtom(SourceSpan? source, Atom? baseAtom, string accentName)
            : base(source)
        {
            BaseAtom = baseAtom;
            AccentAtom = SymbolAtom.GetAtom(accentName, null);

            if (AccentAtom.Type != TexAtomType.Accent)
                throw new ArgumentException("The specified symbol name is not an accent.", "accent");
        }

        public AccentedAtom(SourceSpan? source, Atom? baseAtom, TexFormula accent)
            : base(source)
        {
            if (!(accent.RootAtom is SymbolAtom rootSymbol))
                throw new ArgumentException("The formula for the accent is not a single symbol.", nameof(accent));

            BaseAtom = baseAtom;
            AccentAtom = rootSymbol;

            if (AccentAtom.Type != TexAtomType.Accent)
                throw new ArgumentException("The specified symbol name is not an accent.", "accent");
        }

        // Atom over which accent symbol is placed.
        public Atom? BaseAtom { get; }

        // Atom representing accent symbol to place over base atom.
        public SymbolAtom AccentAtom { get; }

        protected override Box CreateBoxCore(TexEnvironment environment)
        {
            CharSymbol? GetBaseChar()
            {
                var baseAtom = BaseAtom;
                while (baseAtom is AccentedAtom a) {
                    baseAtom = a.BaseAtom;
                }

                return baseAtom as CharSymbol;
            }

            var texFont = environment.MathFont;
            var style = environment.Style;

            // Create box for base atom.
            var baseBox = BaseAtom == null ? StrutBox.Empty : BaseAtom.CreateBox(environment.GetCrampedStyle());
            var baseCharFont = GetBaseChar()?.GetCharFont(texFont).Value;
            var skew = baseCharFont == null ? 0.0 : texFont.GetSkew(baseCharFont, style);

            // Find character of best scale for accent symbol.
            var accentChar = texFont.GetCharInfo(AccentAtom.Name, style).Value;
            while (texFont.HasNextLarger(accentChar)) {
                var nextLargerChar = texFont.GetNextLargerCharInfo(accentChar, style);
                if (nextLargerChar.Metrics.Width > baseBox.Width)
                    break;
                accentChar = nextLargerChar;
            }

            var resultBox = new VerticalBox();

            // Create and add box for accent symbol.
            Box accentBox;
            var accentItalicWidth = accentChar.Metrics.Italic;
            if (accentItalicWidth > TexUtilities.FloatPrecision) {
                accentBox = new HorizontalBox(new CharBox(environment, accentChar));
                accentBox.Add(new StrutBox(accentItalicWidth, 0, 0, 0));
            } else {
                accentBox = new CharBox(environment, accentChar);
            }
            resultBox.Add(accentBox);

            var delta = Math.Min(baseBox.Height, texFont.GetXHeight(style, accentChar.FontId));
            resultBox.Add(new StrutBox(0, -delta, 0, 0));

            // Centre and add box for base atom. Centre base box and accent box with respect to each other.
            var boxWidthsDiff = (baseBox.Width - accentBox.Width) / 2;
            accentBox.Shift = skew + Math.Max(boxWidthsDiff, 0);
            if (boxWidthsDiff < 0)
                baseBox = new HorizontalBox(baseBox, accentBox.Width, TexAlignment.Center);
            resultBox.Add(baseBox);

            // Adjust height and depth of result box.
            var depth = baseBox.Depth;
            var totalHeight = resultBox.Height + resultBox.Depth;
            resultBox.Depth = depth;
            resultBox.Height = totalHeight - depth;

            return resultBox;
        }
    }
}
