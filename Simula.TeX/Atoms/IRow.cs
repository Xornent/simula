using Simula.TeX.Atoms;

namespace Simula.TeX
{
    // Atom consisting of child atoms displayed in horizontal row with glueElement between them.
    internal interface IRow
    {
        // Dummy atom representing atom just before first child atom.
        Atom WithPreviousAtom(DummyAtom? previousAtom);
    }
}
