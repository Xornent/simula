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
        public Operator(string symbol, OperatorType type = OperatorType.Binary)
        {
            Symbol = symbol;
            Type = type;
        }

        public string Symbol;
        public OperatorType Type;
    }
}