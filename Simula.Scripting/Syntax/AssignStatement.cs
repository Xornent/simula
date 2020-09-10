using System;
using System.Collections.Generic;
using System.Text;
using Simula.Scripting.Token;

namespace Simula.Scripting.Syntax {

    public class AssignStatement : Statement {
        public EvaluationStatement? Left = null;
        public EvaluationStatement? Right = null;

        public new void Parse(Token.TokenCollection token) {
            List<TokenCollection> lr = token.Split(new Token.Token("="));
            if(lr.Count!=2) {
                Left = null;
                Right = null;
                foreach (var item in token) {
                    if(item == "=") {
                        item.Error = new TokenizerException("SS0002");
                    }
                }
                return;
            }
            Left = new EvaluationStatement();
            Left.Parse(lr[0]);
            Right = new EvaluationStatement();
            Right.Parse(lr[1]);
        }
     }
}
