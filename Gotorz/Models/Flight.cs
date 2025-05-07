namespace Gotorz.Domain
{
    public class Flight
    {
        public int Id { get; set; }
        public AirportInfo DepartureAirport { get; set; }
        public AirportInfo ArrivalAirport { get; set; }
        public int Duration { get; set; }
        public string Airline { get; set; }
        public string AirlineLogo { get; set; }
        public string TravelClass { get; set; }
        public string FlightNumber { get; set; }
        public string Legroom { get; set; }
        public List<string> Extensions { get; set; }
        public bool Overnight { get; set; }
        public string Airplane { get; set; }
        public List<string> TicketAlsoSoldBy { get; set; }
        public string PlaneAndCrewBy { get; set; }
        // Sikrer at Flight-klassen har Airline som string
    
          
            public DateTime ArrivalTime { get; set; }
            // Øvrige properties...
        }
    }

