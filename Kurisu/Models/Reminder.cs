using System;
using Newtonsoft.Json;

namespace Kurisu.Models
{
    class Reminder
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty("channel_id")]
        public string ChannelId { get; set; }

        [JsonProperty("guild_id")]
        public string GuildId { get; set; }

        [JsonProperty("user_id")]
        public string UserId { get; set; }

        [JsonProperty("remind_at")]
        public DateTime At { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("is_fired")]
        public bool Fired { get; set; }
    }
}
