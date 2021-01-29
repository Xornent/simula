namespace Simula.TeX
{
    // Single character together with specific font.
    internal class CharFont
    {
        public CharFont(char character, int fontId)
        {
            Character = character;
            FontId = fontId;
        }

        public char Character {
            get;
            private set;
        }

        public int FontId {
            get;
            private set;
        }
    }
}
