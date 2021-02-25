using System;

namespace Simula.Scripting.Json
{
    /// <summary>
    /// Instructs the <see cref="JsonSerializer"/> how to serialize the collection.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false)]
    public sealed class JsonDictionaryAttribute : JsonContainerAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonDictionaryAttribute"/> class.
        /// </summary>
        public JsonDictionaryAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonDictionaryAttribute"/> class with the specified container Id.
        /// </summary>
        /// <param name="id">The container Id.</param>
        public JsonDictionaryAttribute(string id)
            : base(id)
        {
        }
    }
}