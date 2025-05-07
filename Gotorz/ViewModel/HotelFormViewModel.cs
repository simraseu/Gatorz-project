using System.ComponentModel.DataAnnotations;

public class HotelFormViewModel
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Address { get; set; } = string.Empty;

    [Required]
    public string City { get; set; } = string.Empty;

    [Required]
    public string Country { get; set; } = string.Empty;

    [Required]
    public string ApiHotelId { get; set; } = string.Empty;
}