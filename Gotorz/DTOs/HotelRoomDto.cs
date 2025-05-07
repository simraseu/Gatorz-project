using System.Text.Json.Serialization;

namespace Gotorz.DTOs
{
    public class HotelRoomDto
    {
        [JsonPropertyName("room")]
        public string Room { get; set; }

        [JsonPropertyName("price")]
        public string Price { get; set; }

        [JsonPropertyName("payment_details")]
        public List<string> PaymentDetails { get; set; }
    }
}
