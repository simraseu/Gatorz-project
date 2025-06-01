public class Booking
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public DateTime BookingDate { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } // Confirmed, Pending, Cancelled
    public List<TravelPackage> TravelPackages { get; set; } = new List<TravelPackage>();
}