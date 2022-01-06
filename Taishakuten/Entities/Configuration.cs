using System.Text.Json.Serialization;

namespace Taishakuten.Entities
{
    class Configuration
    {
        [JsonPropertyName("token")]
        public string Token { get; set; }

        [JsonPropertyName("version")]
        public string Version { get; set; }

        [JsonPropertyName("connection_string")]
        public string ConnectionString { get; set; }

        // don't set this property to register commands globally
        [JsonPropertyName("commands_guild")]
        public ulong? Guild { get; set; }
    }
}
