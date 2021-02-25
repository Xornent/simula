using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Parser
{
    public class ParserState
    {
        public Stack<IVariableScope> VariableScopes = new Stack<IVariableScope>();
        public Stack<StackItem> Markers = new Stack<StackItem>();
        public Stack<Ast.IStatement> Sibling = new Stack<Ast.IStatement>();
    }

    public enum StackItem 
    {
        Block,
        If,
        Eif,
        Else,
        While,
        Iterate,
        Func,
        Data,
        Conditional,
        Configure,
        Match,
        Try,
        Catch,
        Bracket
    }
}
