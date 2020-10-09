using Newtonsoft.Json;

namespace Kurisu.External.VirusTotal
{
    public class Scan
    {
        [JsonProperty("detected")]
        public bool Detected { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("result")]
        public string Result { get; set; }

        [JsonProperty("update")]
        public string Update { get; set; }
    }
}
