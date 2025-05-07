using Gotorz.Domain;  // Importér Domain versionen
using Gotorz.DTOs; // Importér DTO versionen
using Gotorz.ViewModel;
using Gotorz.Data;
using Microsoft.EntityFrameworkCore;

namespace Gotorz.Services
{
    public class TravelpackageService
    {
        private readonly FlightApiService _flightService;
        private readonly ApplicationDbContext _dbContext;
        private readonly HotelApiService _hotelService;
        private readonly ApplicationDbContext _context; // ← RIGTIGT


        public TravelpackageService(FlightApiService flightService, HotelApiService hotelService, ApplicationDbContext dbContext)
        {
            _flightService = flightService;
            _hotelService = hotelService;
            _dbContext = dbContext;
        }

        public async Task SaveTravelPackageAsync(TravelPackageFormViewModel model)
        {
            var selectedHotel = await _dbContext.Hotels.FindAsync(model.SelectedHotelId);

            var travelPackage = new TravelPackage
            {
                Destination = model.Destination,
                DepartureDate = model.DepartureDate,
                ReturnDate = model.ReturnDate,
                Description = model.Description,
                ImageUrl = model.ImageUrl,
                BasePrice = model.FinalPrice, // 💥 Brug FinalPrice
                ManualPrice = model.ManualPriceOverride,
                PriceMarkupPercentage = (int)model.MarkupPercent,
                HotelName = selectedHotel?.Name,
                HotelAddress = selectedHotel?.Address,
                FlightInfo = model.FlightPackage != null
                    ? $"{model.FlightPackage.Airline} ({model.FlightPackage.FlightNumber})"
                    : null
            };

            _dbContext.TravelPackages.Add(travelPackage);
            await _dbContext.SaveChangesAsync();
        }
        public async Task UpdateTravelPackageAsync(int id, TravelPackageFormViewModel model)
        {
            var travelPackage = await _dbContext.TravelPackages.FindAsync(id);
            if (travelPackage == null)
                return;

            travelPackage.Destination = model.Destination;
            travelPackage.DepartureDate = model.DepartureDate;
            travelPackage.ReturnDate = model.ReturnDate;
            travelPackage.Description = model.Description;
            travelPackage.ImageUrl = model.ImageUrl;
            travelPackage.BasePrice = model.FinalPrice;
            travelPackage.ManualPrice = model.ManualPriceOverride;
            travelPackage.PriceMarkupPercentage = (int)model.MarkupPercent;

            if (model.SelectedHotelId != null)
            {
                var selectedHotel = await _dbContext.Hotels.FindAsync(model.SelectedHotelId);
                travelPackage.HotelName = selectedHotel?.Name;
                travelPackage.HotelAddress = selectedHotel?.Address;
            }

            await _dbContext.SaveChangesAsync();
        }
        public async Task<bool> DeletePackageAsync(int packageId)
        {
            var package = await _context.TravelPackages.FindAsync(packageId);
            if (package == null)
                return false;

            _context.TravelPackages.Remove(package);
            await _context.SaveChangesAsync();

            return true;
        }



        public async Task<List<TravelPackageViewModel>> GetAllPackagesAsync()
        {
            var packages = await _dbContext.TravelPackages.ToListAsync();

            return packages.Select(static p => new TravelPackageViewModel
            {
                Destination = p.Destination,
                TotalPrice = p.FinalPrice.ToString("C"),
                TotalNights = (p.ReturnDate - p.DepartureDate).Days,
                ImageUrl = p.ImageUrl ?? "",
                Description = p.Description ?? "",
                HotelPackage = new HotelPackageViewModel
                {
                    HotelName = p.HotelName ?? "",
                    HotelAddress = p.HotelAddress ?? "",
                    price = p.FinalPrice.ToString("C")
                },
                FlightPackage = new FlightPackageViewModel
                {
                    Airline = "Ukendt",
                    DepartureAirport = new Gotorz.DTOs.AirportInfoDto(), // Brug Domain versionen her
                    ArrivalAirport = new Gotorz.DTOs.AirportInfoDto(),   // Brug Domain versionen her
                    Flights = new(),
                    Extras = new()
                }
            }).ToList();
        }
        public Task<TravelPackageFormViewModel?> GetPackageForEditAsync(int id)
        {
            return Task.FromResult<TravelPackageFormViewModel?>(null);
        }
        public async Task<List<FlightPackageViewModel>> CreateFlightOptionsAsync(
    string from, string to, DateTime departureDate, DateTime returnDate, string currency)
        {
            try
            {
                // Kald FlightApiService for at søge efter fly
                var flightResponse = await _flightService.SearchFlightsAsync(from, to, departureDate, returnDate, currency);

                if (flightResponse == null || flightResponse.OtherFlights == null || !flightResponse.OtherFlights.Any())
                    return new List<FlightPackageViewModel>();

                // Opret listen af FlightPackageViewModel direkte
                return flightResponse.OtherFlights
                    .Where(f => f.Flights != null && f.Flights.Any())
                    .Select(f =>
                    {
                        var firstLeg = f.Flights.First();
                        var lastLeg = f.Flights.Last();

                        return new FlightPackageViewModel
                        {
                            Airline = firstLeg.Airline,
                            AirlineLogo = firstLeg.AirlineLogo,
                            FlightNumber = firstLeg.FlightNumber,
                            TravelClass = firstLeg.TravelClass,
                            Legroom = firstLeg.Legroom,
                            Airplane = firstLeg.Airplane,
                            PlaneAndCrewBy = firstLeg.PlaneAndCrewBy,
                            Overnight = firstLeg.Overnight,
                            Extensions = firstLeg.Extensions?.ToList() ?? new List<string>(),
                            TicketAlsoSoldBy = firstLeg.TicketAlsoSoldBy?.ToList() ?? new List<string>(),
                            DepartureTime = firstLeg.DepartureAirport.Time,
                            ArrivalTime = lastLeg.ArrivalAirport.Time,
                            DepartureAirport = new AirportInfo
                            {
                                Id = firstLeg.DepartureAirport.Id,
                                Name = firstLeg.DepartureAirport.Name,
                                Time = firstLeg.DepartureAirport.Time
                            },
                            ArrivalAirport = new AirportInfo
                            {
                                Id = lastLeg.ArrivalAirport.Id,
                                Name = lastLeg.ArrivalAirport.Name,
                                Time = lastLeg.ArrivalAirport.Time
                            },
                            Duration = f.TotalDuration,
                            Flights = f.Flights
                                .Select(x => $"{x.DepartureAirport.Id} → {x.ArrivalAirport.Id} ({x.Duration} min)")
                                .ToList(),
                            Extras = f.Flights
                                .Where(x => x.Extensions != null)
                                .SelectMany(x => x.Extensions!)
                                .Distinct()
                                .ToList(),
                            Price = f.Price.ToString()
                        };
                    }).ToList();
            }
            catch (Exception ex)
            {
                // Log fejl hvis nødvendigt
                Console.WriteLine($"Fejl ved søgning efter fly: {ex.Message}");
                return new List<FlightPackageViewModel>();
            }
        }

        public async Task<TravelPackageViewModel> CreatePackageAsync(
            string from, string to, DateTime departureDate, DateTime returnDate,
            string checkIn, string checkOut, string currency, string country,
            string hotelId, int adults, int kids, int rooms)
        {
            var flightResponse = await _flightService.SearchFlightsAsync(from, to, departureDate, returnDate, currency);
            if (flightResponse == null || flightResponse.OtherFlights == null)
                return null;

            var selectedHotel = await _dbContext.Hotels.FirstOrDefaultAsync(h => h.ApiHotelId == hotelId);
            if (selectedHotel == null)
                return null;

            var firstFlight = flightResponse.OtherFlights.FirstOrDefault();
            if (firstFlight == null || firstFlight.Flights == null || !firstFlight.Flights.Any())
                return null;

            var firstLeg = firstFlight.Flights.First();
            var lastLeg = firstFlight.Flights.Last();

            var flightViewModel = new FlightPackageViewModel
            {
                Airline = firstLeg.Airline,
                AirlineLogo = firstLeg.AirlineLogo,
                FlightNumber = firstLeg.FlightNumber,
                TravelClass = firstLeg.TravelClass,
                Legroom = firstLeg.Legroom,
                Airplane = firstLeg.Airplane,
                PlaneAndCrewBy = firstLeg.PlaneAndCrewBy,
                Overnight = firstLeg.Overnight,
                Extensions = firstLeg.Extensions?.ToList() ?? new(),
                TicketAlsoSoldBy = firstLeg.TicketAlsoSoldBy?.ToList() ?? new(),
                DepartureTime = firstLeg.DepartureAirport.Time,
                ArrivalTime = lastLeg.ArrivalAirport.Time,
                DepartureAirport = new Gotorz.DTOs.AirportInfoDto // Brug DTO versionen her
                {
                    Id = firstLeg.DepartureAirport.Id,
                    Name = firstLeg.DepartureAirport.Name,
                    Time = firstLeg.DepartureAirport.Time
                },
                ArrivalAirport = new Gotorz.DTOs.AirportInfoDto // Brug DTO versionen her
                {
                    Id = lastLeg.ArrivalAirport.Id,
                    Name = lastLeg.ArrivalAirport.Name,
                    Time = lastLeg.ArrivalAirport.Time
                },
                Duration = firstFlight.TotalDuration,
                Flights = firstFlight.Flights
                    .Select(f => $"{f.DepartureAirport.Id} → {f.ArrivalAirport.Id} ({f.Duration} min)")
                    .ToList(),
                Extras = firstFlight.Flights
                    .Where(f => f.Extensions != null)
                    .SelectMany(f => f.Extensions!)
                    .Distinct()
                    .ToList()
            };

            var hotelViewModel = new HotelPackageViewModel
            {
                HotelName = selectedHotel.Name,
                HotelAddress = selectedHotel.Address,
                RoomType = "Ikke valgt endnu",
                price = "Ikke angivet",
                PaymentDetails = new List<string>(),
                Rooms = new List<string>()
            };

            return new TravelPackageViewModel
            {
                FlightPackage = flightViewModel,
                HotelPackage = hotelViewModel,
                Destination = to,
                TotalNights = (returnDate - departureDate).Days,
                TotalPrice = "Ukendt",
                AvailableFlights = flightResponse.OtherFlights
                    .Where(f => f.Flights.Any())
                    .Select(f =>
                    {
                        var leg = f.Flights.First();
                        var last = f.Flights.Last();

                        return new FlightPackageViewModel
                        {
                            Airline = leg.Airline,
                            AirlineLogo = leg.AirlineLogo,
                            FlightNumber = leg.FlightNumber,
                            TravelClass = leg.TravelClass,
                            Legroom = leg.Legroom,
                            Airplane = leg.Airplane,
                            PlaneAndCrewBy = leg.PlaneAndCrewBy,
                            Overnight = leg.Overnight,
                            Extensions = leg.Extensions?.ToList() ?? new(),
                            TicketAlsoSoldBy = leg.TicketAlsoSoldBy?.ToList() ?? new(),
                            DepartureTime = leg.DepartureAirport.Time,
                            ArrivalTime = last.ArrivalAirport.Time,
                            DepartureAirport = new Gotorz.DTOs.AirportInfoDto // Brug DTO versionen her
                            {
                                Id = leg.DepartureAirport.Id,
                                Name = leg.DepartureAirport.Name,
                                Time = leg.DepartureAirport.Time
                            },
                            ArrivalAirport = new Gotorz.DTOs.AirportInfoDto // Brug DTO versionen her
                            {
                                Id = last.ArrivalAirport.Id,
                                Name = last.ArrivalAirport.Name,
                                Time = last.ArrivalAirport.Time
                            },
                            Duration = f.TotalDuration,
                            Flights = f.Flights
                                .Select(x => $"{x.DepartureAirport.Id} → {x.ArrivalAirport.Id} ({x.Duration} min)")
                                .ToList(),
                            Extras = f.Flights
                                .Where(x => x.Extensions != null)
                                .SelectMany(x => x.Extensions!)
                                .Distinct()
                                .ToList(),
                            Price = f.Price.ToString()
                        };
                    }).ToList()
            };
        }

       
    }
}
