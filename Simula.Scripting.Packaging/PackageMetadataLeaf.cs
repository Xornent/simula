using Simula.Scripting.Json;

namespace Simula.Scripting.Packaging
{

    [JsonObject]
    public class PackageMetadataLeaf
    {

        [JsonProperty("@id")]
        public string Id { get; set; }

        [JsonProperty("@type")]
        public string Type { get; set; }

        [JsonProperty("commitId")]
        public string CommitId { get; set; }

        [JsonProperty("commitTimeStamp")]
        public string CommitTimeStamp { get; set; }

        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("parent")]
        public string Parent { get; set; }

        [JsonProperty("lower")]
        public string Lower { get; set; }

        [JsonProperty("upper")]
        public string Upper { get; set; }

        [JsonProperty("items")]
        public PackageMetadataVersion[] Items { get; set; }
    }
}
