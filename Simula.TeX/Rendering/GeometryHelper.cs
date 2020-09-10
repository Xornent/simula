using System.Windows;
using System.Windows.Media;

namespace Simula.TeX.Rendering
{
    internal static class GeometryHelper
    {
        public static Rect ScaleRectangle(double scale, Rect rectangle) =>
            new Rect(rectangle.X * scale, rectangle.Y * scale, rectangle.Width * scale, rectangle.Height * scale);
    }
}
