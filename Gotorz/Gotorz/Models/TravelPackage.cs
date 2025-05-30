using Gotorz.Models;

public class TravelPackage
{
    public int Id { get; set; }
    public string Destination { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal Price { get; set; }
    public string Description { get; set; }
    public FlightInfo Flight { get; set; }
    public HotelInfo Hotel { get; set; }
 
    public string HotelName { get; set; } = string.Empty;
    public string HotelAddress { get; set; } = string.Empty;
    public string FlightInfo { get; set; } = string.Empty;
    
    public DateTime DepartureDate { get; set; }
    public DateTime ReturnDate { get; set; }

    public decimal BasePrice { get; set; }
    public int PriceMarkupPercentage { get; set; } = 10;
    public decimal? ManualPrice { get; set; }

    public string? ImageUrl { get; set; }
   
    // 🛠️ NYE FELTER:
    public string DepartureAirport { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public int Adults { get; set; }
    public int Kids { get; set; }
    public int Rooms { get; set; }

    public decimal FinalPrice => ManualPrice ?? Math.Round(BasePrice * (1 + PriceMarkupPercentage / 100m), 2);

    public List<Booking> Bookings { get; set; } = new();
}