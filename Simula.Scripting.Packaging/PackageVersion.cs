using Simula.Scripting.Json;
using System.IO;
using System.Net;

namespace Simula.Scripting.Packaging
{
    [JsonObject(MemberSerialization.OptIn)]
    public class PackageVersion
    {
        [JsonProperty("data")]
        public string[] Versions;

        public PackageVersion Create(string id, string baseUrl,
            bool prerelease = false)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(baseUrl +
                "?" + "id=" + id + "&prerelease=" + prerelease.ToString().ToLower());
            request.Method = "GET";
            using (var response = request.GetResponse()) {
                JsonSerializerSettings settings = new JsonSerializerSettings();
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string json = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<PackageVersion>(json);
            }
        }
    }
}
