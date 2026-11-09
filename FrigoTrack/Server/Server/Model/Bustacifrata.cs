using System.Text.Json.Serialization;

namespace Server.Model
{
    public class BustaCifrata
    {
        [JsonPropertyName("session_id")]
        public string SessionId { get; set; } = "";

        [JsonPropertyName("ciphertext")]
        public string Ciphertext { get; set; } = "";

        [JsonPropertyName("tag")]
        public string Tag { get; set; } = "";
    }
}