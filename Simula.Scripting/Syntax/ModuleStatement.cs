namespace Simula.Scripting.Syntax
{

    public class ModuleStatement : Statement
    {
        public new void Parse(Token.TokenCollection sentence)
        {
            this.RawToken.AddRange(sentence);
            if (sentence.Count <= 1) {
                sentence[0].Error = new Token.TokenizerException("SS0005");
            }
            string fullName = "";
            for (int i = 1; i < sentence.Count; i++) {
                if (i % 2 == 1) {
                    if (sentence[i].IsValidNameBeginning()) {
                        fullName += (string)sentence[i];
                    } else sentence[i].Error = new Token.TokenizerException("SS0006");
                } else {
                    if (sentence[i] == ".")
                        fullName += ".";
                    else sentence[i].Error = new Token.TokenizerException("SS0006");
                }
            }

            if (sentence.Count % 2 != 0) {
                sentence.Last().Error = new Token.TokenizerException("SS0006");
            }

            FullName = fullName;
        }

        public string FullName = "";
    }
}
