using System.Text.Json.Serialization;

namespace Gotorz.DTOs
{
    public class FlightDto
    {
        [JsonPropertyName("departure_airport")]
        public AirportInfoDto DepartureAirport { get; set; }

        [JsonPropertyName("arrival_airport")]
        public AirportInfoDto ArrivalAirport { get; set; }

        [JsonPropertyName("departure_time")]
        public string DepartureTime { get; set; }

        [JsonPropertyName("arrival_time")]
        public string ArrivalTime { get; set; }



        [JsonPropertyName("duration")]
        public int Duration { get; set; }

        [JsonPropertyName("airline")]
        public string Airline { get; set; }

        [JsonPropertyName("airline_logo")]
        public string AirlineLogo { get; set; }

        [JsonPropertyName("travel_class")]
        public string TravelClass { get; set; }

        [JsonPropertyName("flight_number")]
        public string FlightNumber { get; set; }

        [JsonPropertyName("legroom")]
        public string Legroom { get; set; }

        [JsonPropertyName("extensions")]
        public string[]? Extensions { get; set; }

        [JsonPropertyName("overnight")]
        public bool Overnight { get; set; }

        [JsonPropertyName("airplane")]
        public string Airplane { get; set; }

        [JsonPropertyName("ticket_also_sold_by")]
        public string[]? TicketAlsoSoldBy { get; set; }

        [JsonPropertyName("plane_and_crew_by")]
        public string PlaneAndCrewBy { get; set; }
    }
}

