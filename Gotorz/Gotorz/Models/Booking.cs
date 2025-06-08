public class Booking
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime BookingDate { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } // Confirmed, Pending, Cancelled
    public List<TravelPackage> TravelPackages { get; set; } = new List<TravelPackage>();
}