// Gotorz.DTOs/PriceInsightsDto.cs
using System.Text.Json.Serialization;

namespace Gotorz.DTOs
{
    public class PriceInsightsDto
    {
        [JsonPropertyName("lowest_price")]
        public int LowestPrice { get; set; }

        [JsonPropertyName("price_level")]
        public string PriceLevel { get; set; }

        [JsonPropertyName("typical_price_range")]
        public int[] TypicalPriceRange { get; set; }

        [JsonPropertyName("price_history")]
        public int[][] PriceHistory { get; set; }
    }
}


