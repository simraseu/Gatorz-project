// Gotorz.DTOs/OtherFlightDto.cs
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Gotorz.DTOs
{
    public class OtherFlightDto
    {
        [JsonPropertyName("flights")]
        public List<FlightDto> Flights { get; set; }

        [JsonPropertyName("layovers")]
        public List<LayoverDto>? Layovers { get; set; }


        [JsonPropertyName("total_duration")]
        public int TotalDuration { get; set; }

        [JsonPropertyName("price")]
        public int Price { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("airline_logo")]
        public string AirlineLogo { get; set; }

        [JsonPropertyName("departure_token")]
        public string DepartureToken { get; set; }

        [JsonPropertyName("carbon_emissions")]
        public CarbonEmissionsDto CarbonEmissions { get; set; }
    }

    public class CarbonEmissionsDto
    {
        [JsonPropertyName("this_flight")]
        public int ThisFlight { get; set; }

        [JsonPropertyName("typical_for_this_route")]
        public int TypicalForThisRoute { get; set; }

        [JsonPropertyName("difference_percent")]
        public int DifferencePercent { get; set; }
    }
}

