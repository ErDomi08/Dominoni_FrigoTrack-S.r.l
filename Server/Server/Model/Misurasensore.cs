using System.Text.Json.Serialization;

namespace Server.Model
{

    public class MisuraSensore
    {
        [JsonPropertyName("sensor_id")]
        public string SensorId { get; set; } = "";

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonPropertyName("cargo_type")]
        public string CargoType { get; set; } = "";

        [JsonPropertyName("temperature_c")]
        public double TemperatureC { get; set; }

        [JsonPropertyName("humidity")]
        public double? Humidity { get; set; }

        [JsonPropertyName("battery")]
        public int? Battery { get; set; }
    }
}