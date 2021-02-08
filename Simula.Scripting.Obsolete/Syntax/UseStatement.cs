using Simula.Scripting.Token;

namespace Simula.Scripting.Syntax
{
    public class UseStatement : Statement
    {
        public TokenCollection Reference = new TokenCollection();
        public new void Parse(TokenCollection sentence)
        {
            this.RawToken.AddRange(sentence);
            if (sentence.Count <= 1) {
                sentence[0].Error = new Token.TokenizerException("SS0003");
            } 
            string fullName = "";
            for (int i = 1; i < sentence.Count; i++) {
                if (i % 2 == 1) {
                    if (sentence[i].IsValidNameBeginning() ||
                        sentence[i] == "*") {
                        fullName += (string)sentence[i];
                    } else sentence[i].Error = new Token.TokenizerException("SS0004");
                } else {
                    if (sentence[i] == ".")
                        fullName += ".";
                    else sentence[i].Error = new Token.TokenizerException("SS0004");
                }

                Reference.Add(sentence[i]);
            }
            FullName = fullName;
        }

        public string FullName = "";
    }
}
