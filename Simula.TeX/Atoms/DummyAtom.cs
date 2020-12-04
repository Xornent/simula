using Simula.TeX.Boxes;
using Simula.TeX.Utils;

namespace Simula.TeX.Atoms
{
    // Dummy atom representing atom whose type can change or which can be replaced by a ligature.
    internal class DummyAtom : Atom
    {
        public DummyAtom(TexAtomType type, Atom atom, bool isTextSymbol)
            : base(atom.Source, type)
        {
            Atom = atom;
            IsTextSymbol = isTextSymbol;
        }

        public DummyAtom(Atom atom)
            : this(TexAtomType.None, atom, false)
        {
        }

        public Atom WithPreviousAtom(DummyAtom? previousAtom)
        {
            if (Atom is IRow row) {
                return new DummyAtom(Type, row.WithPreviousAtom(previousAtom), IsTextSymbol);
            }

            return this;
        }

        public static DummyAtom CreateLigature(FixedCharAtom ligatureAtom) =>
            new DummyAtom(TexAtomType.None, ligatureAtom, false);

        public Atom Atom { get; }

        public bool IsTextSymbol { get; }

        public DummyAtom WithType(TexAtomType type) =>
            new DummyAtom(type, Atom, IsTextSymbol);

        public DummyAtom AsTextSymbol() =>
            IsTextSymbol ? this : new DummyAtom(Type, Atom, true);

        public bool IsKern {
            get { return Atom is SpaceAtom; }
        }

        public Result<CharFont> GetCharFont(ITeXFont texFont) =>
            ((CharSymbol)Atom).GetCharFont(texFont);

        protected override Box CreateBoxCore(TexEnvironment environment) =>
            Atom.CreateBox(environment);

        public override TexAtomType GetLeftType()
        {
            return Type == TexAtomType.None ? Atom.GetLeftType() : Type;
        }

        public override TexAtomType GetRightType()
        {
            return Type == TexAtomType.None ? Atom.GetRightType() : Type;
        }
    }
}
