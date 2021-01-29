using Simula.Scripting.Json;

namespace Simula.Scripting.Packaging
{

    [JsonObject(MemberSerialization.OptIn)]
    public class ServiceIndexContext
    {

        [JsonProperty("@vocab")]
        public string Vocab { get; set; }

        [JsonProperty("comment")]
        public string Comment { get; set; }
    }
}
