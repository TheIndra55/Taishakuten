using Newtonsoft.Json;

namespace Kurisu.Models
{
    class Guild
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("welcome")]
        public Welcome Welcome { get; set; } = new Welcome();
    }
}
