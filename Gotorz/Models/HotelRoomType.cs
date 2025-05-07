namespace Gotorz.Models
{
    // Gotorz.Domain/HotelRoomType.cs

        public class HotelRoomType
        {
            public int Id { get; set; }
            public string RoomName { get; set; } // f.eks. "Standard Værelse", "Deluxe Suite"
            public decimal BasePrice { get; set; } // Fra-pris for dette værelse

            public int HotelId { get; set; }
            public Hotel Hotel { get; set; }
        }
    }
