using Simula.TeX.Boxes;

namespace Simula.TeX.Atoms
{
    // Atom (smallest unit) of TexFormula.
    internal abstract class Atom
    {
        protected Atom(SourceSpan? source, TexAtomType type = TexAtomType.Ordinary)
        {
            Source = source;
            Type = type;
        }

        public TexAtomType Type { get; }

        public SourceSpan? Source { get; }

        public Box CreateBox(TexEnvironment environment)
        {
            var box = CreateBoxCore(environment);
            if (box.Source == null) {
                box.Source = Source;
            }

            return box;
        }

        protected abstract Box CreateBoxCore(TexEnvironment environment);

        // Gets type of leftmost child item.
        public virtual TexAtomType GetLeftType()
        {
            return Type;
        }

        // Gets type of leftmost child item.
        public virtual TexAtomType GetRightType()
        {
            return Type;
        }
    }
}
