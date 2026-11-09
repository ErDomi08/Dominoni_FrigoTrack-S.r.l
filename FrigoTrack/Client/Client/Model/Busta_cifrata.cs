using System.Text.Json.Serialization;

namespace Client.Model
{
    public class Busta_cifrata
    {
        [JsonPropertyName("session_id")]
        public string SessionId { get; set; } = "";

        [JsonPropertyName("ciphertext")]
        public string Ciphertext { get; set; } = "";

        [JsonPropertyName("tag")]
        public string Tag { get; set; } = "";
    }
}