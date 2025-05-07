using System.Text.Json.Serialization;

namespace Gotorz.DTOs
{
    public class DepartureAirportDetailsDto
    {
        [JsonPropertyName("airport")]
        public AirportDetailsDto Airport { get; set; }

        [JsonPropertyName("city")]
        public string City { get; set; }

        [JsonPropertyName("country")]
        public string Country { get; set; }

        [JsonPropertyName("country_code")]
        public string CountryCode { get; set; }

        [JsonPropertyName("image")]
        public string Image { get; set; }

        [JsonPropertyName("thumbnail")]
        public string Thumbnail { get; set; }
    }
}

