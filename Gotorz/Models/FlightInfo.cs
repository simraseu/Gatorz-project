namespace Gotorz.Models
{
    public class FlightInfo
    {
        public int Id { get; set; }
        public string FlightNumber { get; set; }
        public string Airline { get; set; }
        public string DepartureAirport { get; set; }
        public string ArrivalAirport { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public decimal Price { get; set; }
      
        public int TravelPackageId { get; set; } // Fremmednøgle
        


        public TravelPackage TravelPackage { get; set; }
    }
}