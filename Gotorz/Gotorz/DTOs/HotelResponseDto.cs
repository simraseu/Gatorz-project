using System.Text.Json.Serialization;


namespace Gotorz.DTOs
{

    public class HotelResponseDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("rate_per_night")]
        public RateDto RatePerNight { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; }

        [JsonPropertyName("image_urls")]
        public List<string> ImageUrls { get; set; }

        [JsonPropertyName("extracted_lowest")]
        public decimal ExtractedLowest { get; set; }

        [JsonPropertyName("lowest")]
        public decimal Lowest { get; set; }

        [JsonPropertyName("booking_link")]
        public string BookingLink { get; set; }
    }


}





