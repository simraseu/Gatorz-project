namespace Gatorz.Models
{
    public class TravelPackageViewModel
    {
        public string Id { get; set; }
        public string Destination { get; set; }
        public string OriginCity { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Price { get; set; }
        public string Airline { get; set; }
        public string HotelName { get; set; }
        public int HotelRating { get; set; }
        public DateTime FlightDepartureTime { get; set; }
        public DateTime FlightArrivalTime { get; set; }
        public bool ReturnFlightIncluded { get; set; }
        public string ImageUrl { get; set; }

        // Reference til de underliggende objekter (skjult fra brugergrænsefladen, men bruges internt)
        public FlightInfo Flight { get; set; }
        public HotelInfo Hotel { get; set; }

        // Hjælpemetoder
        public string GetFormattedPrice() => $"{Price:N0}"; // Formatteret pris uden decimaler

        public string GetDuration()
        {
            int days = (EndDate - StartDate).Days;
            return $"{days} {(days == 1 ? "night" : "nights")}";
        }

        public string GetStars()
        {
            return new string('★', HotelRating) + new string('☆', 5 - HotelRating);
        }
    }
}