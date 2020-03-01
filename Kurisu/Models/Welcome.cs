using Newtonsoft.Json;

namespace Kurisu.Models
{
    class Welcome
    {
        [JsonProperty("header")] 
        public string Header { get; set; } = "Welcome {0}";

        [JsonProperty("body")]
        public string Body { get; set; } = "";

        [JsonProperty("channel")]
        public string Channel { get; set; }

        [JsonProperty("color")]
        public int Color { get; set; }

        [JsonProperty("mention")]
        public bool Mention { get; set; }
    }
}
