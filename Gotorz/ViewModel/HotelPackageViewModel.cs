namespace Gotorz.ViewModel
{
    public class HotelPackageViewModel
    {
        public string HotelName { get; set; }
        public string HotelAddress { get; set; }
        public string HotelDescription { get; set; }
        public string RoomType { get; set; }

        public string price { get; set; }

        public List<string> PaymentDetails { get; set; }

        public List<string> Rooms { get; set; }

    }
}
