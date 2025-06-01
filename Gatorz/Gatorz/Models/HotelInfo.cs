namespace Gatorz.Models
{
    public class HotelInfo
    {
        public int Id { get; set; }
        public string HotelName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public int StarRating { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public string RoomType { get; set; }
        public decimal PricePerNight { get; set; }

        // Relation til TravelPackage
        public int TravelPackageId { get; set; }
        public TravelPackage TravelPackage { get; set; }
    }
}