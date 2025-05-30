using System.Text.Json.Serialization;

namespace Gotorz.DTOs
{
    public class LayoverDto
    {
        [JsonPropertyName("duration")]
        public int Duration { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("overnight")]
        public bool Overnight { get; set; }
    }
}

