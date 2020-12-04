using Simula.TeX.Boxes;

namespace Simula.TeX.Atoms
{
    // Atom representing other atom that is not rendered.
    internal class PhantomAtom : Atom, IRow
    {
        private readonly bool useWidth;
        private readonly bool useHeight;
        private readonly bool useDepth;

        public PhantomAtom(
            SourceSpan? source,
            Atom? baseAtom,
            bool useWidth = true,
            bool useHeight = true,
            bool useDepth = true)
            : base(source)
        {
            RowAtom = baseAtom == null ? new RowAtom(null) : new RowAtom(null, baseAtom);
            this.useWidth = useWidth;
            this.useHeight = useHeight;
            this.useDepth = useDepth;
        }

        public Atom WithPreviousAtom(DummyAtom? previousAtom) =>
            new PhantomAtom(
                Source,
                RowAtom.WithPreviousAtom(previousAtom),
                useWidth,
                useHeight,
                useDepth);

        public RowAtom RowAtom { get; }

        protected override Box CreateBoxCore(TexEnvironment environment)
        {
            var resultBox = RowAtom.CreateBox(environment);
            return new StrutBox((useWidth ? resultBox.Width : 0), (useHeight ? resultBox.Height : 0),
                (useDepth ? resultBox.Depth : 0), resultBox.Shift);
        }

        public override TexAtomType GetLeftType()
        {
            return RowAtom.GetLeftType();
        }

        public override TexAtomType GetRightType()
        {
            return RowAtom.GetRightType();
        }
    }
}
