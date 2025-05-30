// ViewModels/HotelFormViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace Gotorz.ViewModel
{
    public class HotelFormViewModel
    {
        
        public string Name { get; set; } = string.Empty;

        
        public string Address { get; set; } = string.Empty;

        
        public string? ApiHotelId { get; set; } = string.Empty;

        
        public string City { get; set; } = string.Empty;

        
        public string Country { get; set; } = string.Empty;

        public Decimal FromPrice { get; set; }

        public string Description { get; set; }

        public Decimal ExtractedLowest { get; set; }

        public decimal? HotelApiPrice { get; set; } // fx i TravelPackageFormViewModel

    }
}
