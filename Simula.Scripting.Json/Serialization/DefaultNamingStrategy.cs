namespace Simula.Scripting.Json.Serialization
{
    public class DefaultNamingStrategy : NamingStrategy
    {
        protected override string ResolvePropertyName(string name)
        {
            return name;
        }
    }
}