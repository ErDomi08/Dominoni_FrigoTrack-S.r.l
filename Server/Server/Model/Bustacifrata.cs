using System.Text.Json.Serialization;

namespace Server.Model
{
    public class BustaCifrata
    {
        [JsonPropertyName("session_id")]
        public string SessionId { get; set; } = "";

        [JsonPropertyName("testo_cifrato")]
        public string TestoCifrato { get; set; } = "";

        [JsonPropertyName("tag")]
        public string Tag { get; set; } = "";
    }
}