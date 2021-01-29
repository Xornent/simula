using Simula.TeX.Boxes;
using System.Windows.Media;

namespace Simula.TeX.Atoms
{
    // Atom specifying graphical style.
    internal class StyledAtom : Atom, IRow
    {
        public StyledAtom(SourceSpan? source, Atom? atom, Brush? backgroundColor, Brush? foregroundColor)
            : base(source)
        {
            RowAtom = new RowAtom(source, atom);
            Background = backgroundColor;
            Foreground = foregroundColor;
        }

        // RowAtom to which colors are applied.
        public RowAtom RowAtom { get; }

        public Brush? Background { get; }

        public Brush? Foreground { get; }

        public Atom WithPreviousAtom(DummyAtom? previousAtom)
        {
            var rowAtom = RowAtom.WithPreviousAtom(previousAtom);
            return new StyledAtom(Source, rowAtom, Background, Foreground);
        }

        protected override Box CreateBoxCore(TexEnvironment environment)
        {
            var newEnvironment = environment.Clone();
            if (Foreground != null)
                newEnvironment.Foreground = Foreground;
            var childBox = RowAtom.CreateBox(newEnvironment);
            if (Background != null)
                childBox.Background = Background;
            return childBox;
        }

        public override TexAtomType GetLeftType()
        {
            return RowAtom.GetLeftType();
        }

        public override TexAtomType GetRightType()
        {
            return RowAtom.GetRightType();
        }

        public StyledAtom Clone(
            RowAtom? rowAtom = null,
            Brush? background = null,
            Brush? foreground = null)
        {
            return new StyledAtom(
                Source,
                rowAtom ?? RowAtom,
                background ?? Background,
                foreground ?? Foreground);
        }
    }
}
