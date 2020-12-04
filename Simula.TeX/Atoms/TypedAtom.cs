using Simula.TeX.Boxes;

namespace Simula.TeX.Atoms
{
    // Atom representing other atom with custom left and right types.
    internal class TypedAtom : Atom
    {
        public TypedAtom(SourceSpan? source, Atom? atom, TexAtomType leftType, TexAtomType rightType)
            : base(source)
        {
            Atom = atom;
            LeftType = leftType;
            RightType = rightType;
        }

        public Atom? Atom { get; }

        public TexAtomType LeftType { get; }

        public TexAtomType RightType { get; }

        protected override Box CreateBoxCore(TexEnvironment environment) =>
            Atom!.CreateBox(environment); // Nullable TODO: This probably needs null checking

        public override TexAtomType GetLeftType()
        {
            return LeftType;
        }

        public override TexAtomType GetRightType()
        {
            return RightType;
        }
    }
}
