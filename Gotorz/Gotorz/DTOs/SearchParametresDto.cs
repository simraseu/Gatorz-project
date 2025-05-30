// Gotorz.DTOs/SearchParametersDto.cs
using System.Text.Json.Serialization;

namespace Gotorz.DTOs
{
    public class SearchParametersDto
    {
        [JsonPropertyName("engine")]
        public string Engine { get; set; }

        [JsonPropertyName("hl")]
        public string Hl { get; set; }

        [JsonPropertyName("gl")]
        public string Gl { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("departure_id")]
        public string DepartureId { get; set; }

        [JsonPropertyName("arrival_id")]
        public string ArrivalId { get; set; }

        [JsonPropertyName("outbound_date")]
        public string OutboundDate { get; set; }

        [JsonPropertyName("return_date")]
        public string ReturnDate { get; set; }

        [JsonPropertyName("travel_class")]
        public int TravelClass { get; set; }

        [JsonPropertyName("adults")]
        public int Adults { get; set; }

        [JsonPropertyName("children")]
        public int Children { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; }

        [JsonPropertyName("sort_by")]
        public string SortBy { get; set; }
    }
}

