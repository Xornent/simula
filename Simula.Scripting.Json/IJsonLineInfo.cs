
namespace Simula.Scripting.Json
{
    public interface IJsonLineInfo
    {
        bool HasLineInfo();
        int LineNumber { get; }
        int LinePosition { get; }
    }
}