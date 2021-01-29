using Simula.Scripting.Json;

namespace Simula.Scripting.Packaging
{

    [JsonObject]
    public class PackageMetadataVersion
    {

        [JsonProperty("@id")]
        public string Id { get; set; }

        [JsonProperty("@type")]
        public string Type { get; set; }

        [JsonProperty("commitId")]
        public string CommitId { get; set; }

        [JsonProperty("commitTimeStamp")]
        public string CommitTimeStamp { get; set; }

        [JsonProperty("catalogEntry")]
        public CatelogEntry Entry { get; set; }

        [JsonProperty("packageContent")]
        public string Download { get; set; }

        [JsonProperty("registration")]
        public string Registration { get; set; }
    }
}
