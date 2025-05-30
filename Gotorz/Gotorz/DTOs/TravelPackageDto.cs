namespace Gotorz.DTOs;

public class TravelPackageDto
{
    public int Id { get; set; }
    public string Destination { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }

    public string? TotalPrice { get; set; }

    public string HotelName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
    
    public DateTime DepartureDate { get; set; }

    public DateTime ReturnDate { get; set; }
}
