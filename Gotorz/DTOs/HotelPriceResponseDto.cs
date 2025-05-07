using System.Text.Json.Serialization;

namespace Gotorz.DTOs
{
    public class HotelPriceResponseDto
    {
        [JsonPropertyName("hotel")]
        public HotelInfoDto Hotel { get; set; }

        [JsonPropertyName("rooms")]
        public List<HotelRoomDto> Rooms { get; set; } = new();

        [JsonPropertyName("price")]
        public string Price { get; set; }

        public List<List<Dictionary<string, string>>> Comparison { get; set; } = new();

    }
}




