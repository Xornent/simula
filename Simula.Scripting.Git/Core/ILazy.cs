namespace Simula.Scripting.Git.Core
{
    internal interface ILazy<T>
    {
        T Value { get; }
    }
}
