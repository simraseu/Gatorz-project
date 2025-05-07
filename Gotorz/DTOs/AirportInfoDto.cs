using System.Text.Json.Serialization;

namespace Gotorz.DTOs
{
    public class AirportInfoDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("time")]
        public string Time { get; set; }

        [JsonPropertyName("departure_airport")]
        public AirportInfoDto DepartureAirport { get; set; }

    }
}
