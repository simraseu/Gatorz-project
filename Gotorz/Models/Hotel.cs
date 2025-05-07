using System.ComponentModel.DataAnnotations;

public class Hotel
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; }

    [Required]
    public string Address { get; set; } = "";

    [Required]
    public string City { get; set; }

    [Required]
    public string Country { get; set; }

    [Required]
    public string ApiHotelId { get; set; } = "";

    public string FromPrice { get; set; } = string.Empty;
    public string RoomType { get; set; } = string.Empty;

    // Disse felter ser ud til at skabe forvirring - overvej at fjerne dem
    // eller tilføj dem til din database, hvis de skal bruges
    public string HotelName { get; set; }
    public int StarRating { get; set; }
}