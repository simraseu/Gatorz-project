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
}