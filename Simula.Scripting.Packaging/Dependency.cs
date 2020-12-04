using Simula.Scripting.Json;

namespace Simula.Scripting.Packaging
{

    [JsonObject]
    public class Dependency
    {

        [JsonProperty("@id")]
        public string Id { get; set; }

        [JsonProperty("@type")]
        public string Type { get; set; }

        [JsonProperty("id")]
        public string PackageId { get; set; }

        [JsonProperty("range")]
        public string VersionRange { get; set; }

        [JsonProperty("registration")]
        public string Registration { get; set; }
    }
}
