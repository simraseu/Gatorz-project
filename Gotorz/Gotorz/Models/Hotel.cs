public class Hotel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
    public string? ApiHotelToken { get; set; }

    public string? ImageUrl { get; set; }

    public string ApiHotelId { get; set; } = string.Empty;
    public decimal FromPrice { get; set; }

    public decimal ExtractedLowest { get; set; }
}
