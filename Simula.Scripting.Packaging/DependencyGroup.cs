using Simula.Scripting.Json;

namespace Simula.Scripting.Packaging
{
    [JsonObject]
    public class DependencyGroup
    {
        [JsonProperty("@id")]
        public string Id { get; set; }

        [JsonProperty("@type")]
        public string Type { get; set; }

        [JsonProperty("targetFramework")]
        public string TargetFramework { get; set; }

        [JsonProperty("dependencies")]
        public Dependency[] Dependencies { get; set; }
    }
}
