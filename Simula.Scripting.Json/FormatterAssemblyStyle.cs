
#if HAVE_OBSOLETE_FORMATTER_ASSEMBLY_STYLE

namespace System.Runtime.Serialization.Formatters
{
    [Obsolete("FormatterAssemblyStyle is obsolete. Use TypeNameAssemblyFormatHandling instead.")]
    public enum FormatterAssemblyStyle
    {
        Simple = 0,
        Full = 1
    }
}

#endif