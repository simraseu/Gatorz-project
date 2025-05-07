using System.Text.Json.Serialization;

namespace Gotorz.DTOs
{
    public class AirportDetailsDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
