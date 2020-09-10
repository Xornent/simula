
namespace Simula.Scripting.Json.Serialization
{
    public interface IValueProvider
    {
        void SetValue(object target, object? value);
        object? GetValue(object target);
    }
}