using System.Text.Json.Serialization;
using System.Collections.Generic;
using Gotorz.Domain;
using Gotorz.DTOs;

namespace Gotorz.DTOs
{
    public class FlightResponseDto
    {
        [JsonPropertyName("search_metadata")]
        public SearchMetadataDto SearchMetadata { get; set; }

        [JsonPropertyName("search_parameters")]
        public SearchParametersDto SearchParameters { get; set; }

        [JsonPropertyName("other_flights")]
        public List<OtherFlightDto> OtherFlights { get; set; }

        [JsonPropertyName("price_insights")]
        public PriceInsightsDto PriceInsights { get; set; }


    }
}