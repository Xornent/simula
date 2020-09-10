using System;

namespace Simula.Scripting.Json
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class JsonExtensionDataAttribute : Attribute
    {
        public bool WriteData { get; set; }
        public bool ReadData { get; set; }
        public JsonExtensionDataAttribute()
        {
            WriteData = true;
            ReadData = true;
        }
    }
}