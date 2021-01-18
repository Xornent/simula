namespace Simula.Scripting.Syntax
{

    public enum OperatorType
    {
        UnaryLeft,
        UnaryRight,
        Binary
    }

    public struct Operator
    {
        public Operator(Token.Token symbol, OperatorType type = OperatorType.Binary)
        {
            Symbol = symbol;
            Token = symbol;
            Type = type;
        }

        public string Symbol;
        public Token.Token Token;
        public OperatorType Type;
    }
}