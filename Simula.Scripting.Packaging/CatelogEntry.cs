using Simula.Scripting.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Packaging {

    [JsonObject]
    public class CatelogEntry {

        [JsonProperty("@id")]
        public string Id { get; set; } = "";

        [JsonProperty("@type")]
        public string Type { get; set; } = "";

        [JsonProperty("authors")]
        public string Authors { get; set; } = "";

        [JsonProperty("description")]
        public string Description { get; set; } = "";

        [JsonProperty("iconUrl")]
        public string IconUrl { get; set; } = "";

        [JsonProperty("id")]
        public string PackageId { get; set; } = "";

        [JsonProperty("language")]
        public string Language { get; set; } = "";

        [JsonProperty("licenseExpression")]
        public string LicenseExpression { get; set; } = "";

        [JsonProperty("licenseUrl")]
        public string LicenseUrl { get; set; } = "";

        [JsonProperty("listed")]
        public bool Listed { get; set; } = false;

        [JsonProperty("minClientVersion")]
        public string MinimalClientVersion { get; set; } = "";

        [JsonProperty("packageContent")]
        public string PackageContent { get; set; } = "";

        [JsonProperty("projectUrl")]
        public string ProjectUrl { get; set; } = "";

        [JsonProperty("published")]
        public string Published { get; set; } = "";

        [JsonProperty("requireLicenseAcceptance")]
        public bool RequireLicenseAcceptance { get; set; } = true;

        [JsonProperty("summary")]
        public string Summary { get; set; } = "";

        [JsonProperty("tags")]
        public string[] Tags { get; set; } = new string[] { };

        [JsonProperty("title")]
        public string Title { get; set; } = "";

        [JsonProperty("version")]
        public string Version { get; set; } = "";

        [JsonProperty("dependencyGroups")]
        public DependencyGroup[] DependencyGroups { get; set; } = new DependencyGroup[] { };
    }
}
