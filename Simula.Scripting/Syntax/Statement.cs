using Simula.Scripting.Debugging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Syntax {

    // 以下是 Simula.Scripting.Syntax 空间内的继承关系表:

    // Statement |- BlockStatement |- IfBlock
    //           |                 |- ElseIfBlock
    //           |                 |- ElseBlock
    //           |                 |- WhileBlock
    //           |                 |- CommentBlock *
    //           |                 |- EnumerableBlock
    //           |                 |- DefinitionBlock
    //           |- AssignStatement
    //           |- EvaluationStatement |- OperatorStatement |- *Operation
    //           |- UseStatement
    //           |- ModuleStatement

    public class Statement {
        public virtual void Parse(Token.TokenCollection sentence) {
            return;
        }

        public virtual ExecutionResult Execute(Compilation.RuntimeContext ctx) {
            return new ExecutionResult();
        }
    }
}
