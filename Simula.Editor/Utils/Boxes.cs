
namespace Simula.Editor.Utils
{
    /// <summary>
    /// Reuse the same instances for boxed booleans.
    /// </summary>
    internal static class Boxes
    {
        public static readonly object True = true;
        public static readonly object False = false;

        public static object Box(bool value)
        {
            return value ? True : False;
        }
    }
}
