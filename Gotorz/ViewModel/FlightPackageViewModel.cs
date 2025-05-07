using Gotorz.DTOs;
namespace Gotorz.ViewModel
{
    public class FlightPackageViewModel
    {
        public AirportInfo DepartureAirport { get; set; }
        public AirportInfo ArrivalAirport { get; set; }

        public string DepartureTime { get; set; } // ✅ korrekt


        public string ArrivalTime { get; set; }
        public int Duration { get; set; }
        public string Airline { get; set; }
        public string AirlineLogo { get; set; }
        public string TravelClass { get; set; }
        public string FlightNumber { get; set; }

        public string Legroom { get; set; }
        public List<string> Extensions { get; set; } = new();
        public bool Overnight { get; set; }
        public string Airplane { get; set; }

        public List<string> TicketAlsoSoldBy { get; set; } = new();
        public string PlaneAndCrewBy { get; set; }

        public List<string> Flights { get; set; } = new();

        public List<string> Extras { get; set; } = new();

        public string Price { get; set; }
    }
}