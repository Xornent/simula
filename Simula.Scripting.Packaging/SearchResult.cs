using Simula.Scripting.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Simula.Scripting.Packaging {

    [JsonObject(MemberSerialization.OptIn)]
    public class SearchResult {

        [JsonProperty("totalHits")]
        public int Hit { get; set; }

        [JsonProperty("data")]
        public string[] Data { get; set; }

        public int MaximumPage {
            get {
                return (int) Math.Ceiling(Hit / 20d);
            }
        }

        public static SearchResult Create(string query, string baseUrl,
            int skip = 0, bool prerelease = false) {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(baseUrl +
                "?" + "q=" + query + "&skip=" + skip + "&take=20&prerelease=" + prerelease.ToString().ToLower());
            request.Method = "GET";
            using (var response = request.GetResponse()) {
                JsonSerializerSettings settings = new JsonSerializerSettings();
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string json = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<SearchResult>(json);
            }
        }
    }
}
