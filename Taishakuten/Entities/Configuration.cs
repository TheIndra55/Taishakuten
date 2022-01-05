using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Taishakuten.Entities
{
    class Configuration
    {
        [JsonPropertyName("token")]
        public string Token { get; set; }

        [JsonPropertyName("version")]
        public string Version { get; set; }
    }
}
