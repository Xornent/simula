using Simula.Scripting.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Packaging {

    [JsonObject(MemberSerialization.OptIn)]
    public class ServiceIndexResource {

        [JsonProperty("@id")]
        public string Id { get; set; }

        [JsonProperty("@type")]
        public string Type { get; set; }

        [JsonProperty("comment")]
        public string Comment { get; set; }
    }
}
