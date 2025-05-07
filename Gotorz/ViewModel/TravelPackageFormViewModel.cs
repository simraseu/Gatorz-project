namespace Gotorz.ViewModel
{
   
        public class TravelPackageFormViewModel
        {
            // Rejseperiode og destination
            public string Destination { get; set; } = string.Empty;
            public DateTime DepartureDate { get; set; }
            public DateTime ReturnDate { get; set; }

            public List<FlightPackageViewModel> AvailableFlights { get; set; } = new();
            public int SelectedFlightIndex { get; set; }


            // Info fra API
            public FlightPackageViewModel? FlightPackage { get; set; }
            public HotelPackageViewModel? HotelPackage { get; set; }

            // Brugerinput
            public string? Description { get; set; }
            public string? ImageUrl { get; set; }
            public int SelectedHotelId { get; set; }

            public string OriginAirport { get; set; }
            public string Currency { get; set; }
            public int Adults { get; set; }
            public int Kids { get; set; }
            public int Rooms { get; set; }

            public string Id { get; set; }

            public string City { get; set; } = string.Empty; // fx København

            public string Country { get; set; } = string.Empty; // fx Danmark


            // Prisfelter
            public decimal AutoCalculatedPrice { get; set; } // fx fly + hotel


            public string? SelectedRoomType { get; set; } // fx "Standard Room" eller "Deluxe Room"
            public HotelViewModel? SelectedHotel { get; set; }
            public decimal? HotelPrice { get; set; }

            public string FlightPrice { get; set; } = "0";

            public List<string> AvailableRooms { get; set; } = new();


            public decimal ManualPriceOverride { get; set; } = 0;
            public decimal MarkupPercent { get; set; } = 0;

            // Privat til intern udregning
            private decimal ParsedHotelPrice => HotelPrice ?? 0;

            private decimal ParsedFlightPrice => ParsePrice(FlightPrice);

            private decimal ParsePrice(string? priceString)
            {
                if (string.IsNullOrWhiteSpace(priceString))
                    return 0;

                priceString = priceString.Replace("USD", "").Replace(",", "").Trim();

                return decimal.TryParse(priceString, out var price) ? price : 0;
            }

            // FinalPrice regner altid automatisk
            public decimal FinalPrice =>
                ManualPriceOverride > 0
                    ? ManualPriceOverride
                    : (ParsedHotelPrice + ParsedFlightPrice) * (1 + MarkupPercent / 100m);


            public List<HotelViewModel> AvailableHotels { get; set; } = new(); // vises som liste



        }
    }
