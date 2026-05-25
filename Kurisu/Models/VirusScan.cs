using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;

namespace Kurisu.Models
{
    /// <summary>
    /// Options for the uploads virus scan
    /// </summary>
    class VirusScan
    {
        /// <summary>
        /// If the bot should scan uploads in this guild
        /// </summary>
        [JsonProperty("enabled")]
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// List of file extensions it should scan uploads for
        /// </summary>
        [JsonProperty("extensions")]
        public List<string> Extensions { get; set; } = new List<string> { ".exe", ".dll" };
    }
}
