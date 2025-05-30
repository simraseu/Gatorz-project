namespace Gotorz.ViewModel
{
    public class HotelPackageViewModel
    {
        public string HotelName { get; set; }
        public string HotelAddress { get; set; }
        public string Description { get; set; }
            public string RoomType { get; set; }

        public string price { get; set; }

        public List<string> PaymentDetails { get; set; }

        public string? ImageUrl { get; set; }

        // 👇 Disse to er nye:
        public decimal FromPrice { get; set; }     // Laveste pris på tværs af kilder
        public decimal? AgodaPrice { get; set; }   // Agoda-pris (hvis fundet)

        public List<string> Rooms { get; set; }

    }
}
