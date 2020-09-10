using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Syntax {

    public class CommentBlock : BlockStatement {
        public List<string> Lines = new List<string>();
    }
}
