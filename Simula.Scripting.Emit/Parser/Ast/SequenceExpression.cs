using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Parser.Ast
{
    public class SequenceExpression : BlockStatement
    {
        public int Length { get { return this.Statements.Count; } }
        public BracketType Bracket { get; set; }
    }

    public enum BracketType
    {
        Parenthesis,
        Bracket,
        Brace
    }

    public class TupleExpression : SequenceExpression
    {
        public List<IExpression> DataTypes { get; set; } = new List<IExpression>();
        public List<Literal> Identifers { get; set; } = new List<Literal>();
    }

    public class Matrix2DExpression : SequenceExpression
    {
        public int Width { get; set; } = 0;
        public int Height { get; set; } = 0;
        public IExpression[,] Expressions { get; set; } = new IExpression[0, 0];
    }
}
