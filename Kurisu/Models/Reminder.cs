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

        [JsonProperty("message"), JsonConverter(typeof(SanitizeStringConverter))]
        public string Message { get; set; }

        [JsonProperty("is_fired")]
        public bool Fired { get; set; }

        [JsonProperty("last_error")]
        public string LastError { get; set; } = null;
    }

    sealed class SanitizeStringConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object value, JsonSerializer serializer)
        {
            return Util.RemoveMentions((string) reader.Value);
        }

        public override bool CanConvert(Type objectType)
        {
            return true;
        }
    }
}
