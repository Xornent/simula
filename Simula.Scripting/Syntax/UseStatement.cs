using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Syntax {

    public class UseStatement : Statement {
        public new void Parse(Token.TokenCollection sentence) {
            if (sentence.Count <= 1) {
                sentence[0].Error = new Token.TokenizerException("SS0003");
            }
            string fullName = "";
            for(int i = 1; i< sentence.Count; i++) {
                if(i%2 == 1) {
                    if (sentence[i].IsValidNameBeginning() ||
                        sentence[i] == "*") {
                        fullName += (string)sentence[i];
                    } else sentence[i].Error = new Token.TokenizerException("SS0004");
                } else {
                    if (sentence[i] == ".")
                        fullName += ".";
                    else sentence[i].Error = new Token.TokenizerException("SS0004");
                }
            }
            this.FullName = fullName;
        }

        public string FullName = "";
    }
}
