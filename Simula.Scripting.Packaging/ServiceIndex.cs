using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using Simula.Scripting.Json;

namespace Simula.Scripting.Packaging {

    [JsonObject(MemberSerialization.OptIn)]
    public class ServiceIndex {

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("resources")]
        public ServiceIndexResource[] Resources { get; set; }

        [JsonProperty("@context")]
        public ServiceIndexContext Context { get; set; }
    }
}
