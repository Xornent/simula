namespace Simula.TeX
{
    // Specifies font metrics for single character.
    internal class TexFontMetrics
    {
        public TexFontMetrics(double width, double height, double depth, double italicWidth, double scale)
        {
            Width = width * scale;
            Height = height * scale;
            Depth = depth * scale;
            Italic = italicWidth * scale;
        }

        public double Width {
            get;
            set;
        }

        public double Height {
            get;
            set;
        }

        public double Depth {
            get;
            set;
        }

        public double Italic {
            get;
            set;
        }
    }
}
