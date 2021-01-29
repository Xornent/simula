using Simula.TeX.Exceptions;
using Simula.TeX.Utils;
using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace Simula.TeX
{
    // Specifies all information about single font.
    internal class TexFontInfo
    {
        public const int charCodesCount = 256;

        private readonly double[][] metrics;
        private readonly IDictionary<Tuple<char, char>, char> ligatures;
        private readonly IDictionary<Tuple<char, char>, double> kerns;
        private readonly CharFont[] nextLarger;
        private readonly int[][] extensions;

        public TexFontInfo(int fontId, GlyphTypeface font, double xHeight, double space, double quad)
        {
            metrics = new double[charCodesCount][];
            ligatures = new Dictionary<Tuple<char, char>, char>();
            kerns = new Dictionary<Tuple<char, char>, double>();
            nextLarger = new CharFont[charCodesCount];
            extensions = new int[charCodesCount][];

            FontId = fontId;
            Font = font;
            XHeight = xHeight;
            Space = space;
            Quad = quad;
            SkewCharacter = (char)1;
        }

        public int FontId {
            get;
            private set;
        }

        public GlyphTypeface Font {
            get;
            private set;
        }

        public double XHeight {
            get;
            private set;
        }

        public double Space {
            get;
            private set;
        }

        public double Quad {
            get;
            private set;
        }

        // Skew character (used for positioning accents).
        public char SkewCharacter {
            get;
            set;
        }

        public void AddKern(char leftChar, char rightChar, double kern)
        {
            kerns.Add(Tuple.Create(leftChar, rightChar), kern);
        }

        public void AddLigature(char leftChar, char rightChar, char ligatureChar)
        {
            ligatures.Add(Tuple.Create(leftChar, rightChar), ligatureChar);
        }

        public bool HasSpace()
        {
            return Space > TexUtilities.FloatPrecision;
        }

        public void SetExtensions(char character, int[] extensions)
        {
            this.extensions[character] = extensions;
        }

        public void SetMetrics(char character, double[] metrics)
        {
            this.metrics[character] = metrics;
        }

        public void SetNextLarger(char character, char largerCharacter, int largerFont)
        {
            nextLarger[character] = new CharFont(largerCharacter, largerFont);
        }

        public int[] GetExtension(char character)
        {
            return extensions[character];
        }

        public double GetKern(char leftChar, char rightChar, double factor)
        {
            Tuple<char, char> tpl = Tuple.Create(leftChar, rightChar);
            double kern = 0;
            kerns.TryGetValue(tpl, out kern);
            return kern * factor;
        }

        public CharFont? GetLigature(char left, char right)
        {
            Tuple<char, char> tpl = Tuple.Create(left, right);
            return ligatures.TryGetValue(tpl, out char ch) ? new CharFont(ch, FontId) : null;
        }

        public CharFont GetNextLarger(char character)
        {
            return nextLarger[character];
        }

        public double GetQuad(double factor)
        {
            return Quad * factor;
        }

        public double GetSpace(double factor)
        {
            return Space * factor;
        }

        public double GetXHeight(double factor)
        {
            return XHeight * factor;
        }

        public Result<double[]> GetMetrics(char character)
        {
            if (metrics.Length <= character || metrics[character] == null) {
                return Result.Error<double[]>(
                    new TexCharacterMappingNotFoundException(
                        $"Cannot determine metrics for '{character}' character in font {FontId}"));
            }

            return Result.Ok(metrics[character]);
        }
    }
}
