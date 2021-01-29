using Simula.Scripting.Json;

namespace Simula.Scripting.Packaging
{

    [JsonObject]
    public class PackageMetadata
    {

        [JsonProperty("@id")]
        public string Id { get; set; }

        [JsonProperty("@type")]
        public string[] Type { get; set; }

        [JsonProperty("commitId")]
        public string CommitId { get; set; }

        [JsonProperty("commitTimeStamp")]
        public string CommitTimeStamp { get; set; }

        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("items")]
        public PackageMetadataLeaf[] Catalogues { get; set; }
    }
}
