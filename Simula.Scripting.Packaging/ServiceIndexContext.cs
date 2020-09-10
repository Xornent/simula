using Simula.Scripting.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Packaging {

    [JsonObject(MemberSerialization.OptIn)]
    public class ServiceIndexContext {

        [JsonProperty("@vocab")]
        public string Vocab { get; set; }

        [JsonProperty("comment")]
        public string Comment { get; set; }
    }
}
