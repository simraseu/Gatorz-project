namespace Gotorz.ViewModel
{
    public class HotelViewModel
    {
        public List<HotelViewModel> Hotels { get; set; } = new List<HotelViewModel>();
        public int Id { get; set; }               // Bruges ved redigering/valg
        public string Name { get; set; } = "";    // Hotelnavn
        public string Address { get; set; } = ""; // Adresse
        public string Description { get; set; } = ""; // Valgfri ekstra info
        public string ApiHotelId { get; set; } = ""; // ← Tilføj denne

        public string City { get; set; } = "";
        public string Country { get; set; } = string.Empty;

        public string ImageUrl { get; set; } = ""; // URL til hotelbillede

        public string FromPrice { get; set; } = ""; // Fra-pris fra API
        public string SelectedRoomType { get; set; } = ""; // Vælg værelsestype fra liste
    }
}

