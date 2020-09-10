using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Token {

    public class TokenizerException : ScriptException {
        public TokenizerException(string id) : base(id) { }
    }
}
