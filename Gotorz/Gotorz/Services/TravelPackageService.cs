using Gotorz.Data;
using Gotorz.Models;
using Gotorz.DTOs;
using Gotorz.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace Gotorz.Services
{
    public class TravelPackageService
    {
        private readonly FlightApiService _flightService;
        private readonly HotelApiService _hotelService;

        private readonly ApplicationDbContext _dbContext;

       
        public TravelPackageService(FlightApiService flightService, HotelApiService hotelService,ApplicationDbContext context)
        {
            _flightService = flightService;
            _hotelService = hotelService;
            _dbContext = context;
        }
        public async Task<TravelPackageDto?> GetPackageByIdAsync(int id)
        {
            var package = await _dbContext.TravelPackages.FindAsync(id);

            if (package == null)
                return null;

            return new TravelPackageDto
            {
                Id = package.Id,
                Destination = package.Destination,
                DepartureDate = package.DepartureDate,
                ReturnDate = package.ReturnDate,
                Price = package.FinalPrice,
                ImageUrl = package.ImageUrl ?? ""
            };
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
                BasePrice = model.FinalPrice,
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

        public async Task<List<TravelPackageViewModel>> GetAllPackagesAsync()
        {
            var packages = await _dbContext.TravelPackages.ToListAsync();

            return packages.Select(p => new TravelPackageViewModel
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
                    DepartureAirport = new AirportInfo { Id = "", Name = "", Time = "" },
                    ArrivalAirport = new AirportInfo { Id = "", Name = "", Time = "" },
                    Flights = new(),
                    Extras = new()
                }
            }).ToList();
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



        public Task<TravelPackageFormViewModel?> GetPackageForEditAsync(int id)
        {
            return Task.FromResult<TravelPackageFormViewModel?>(null);
        }


        /*
        public Task DeletePackageAsync(int id)
        {
            return Task.CompletedTask;
        }

        public async Task<TravelPackageViewModel> CreatePackageAsync(
            string from, string to, DateTime departureDate, DateTime returnDate,
            string checkIn, string checkOut, string currency, string country,
            string hotelId, int adults, int kids, int rooms)
        {
            var flightResponse = await _flightService.SearchFlightsAsync(from, to, departureDate, returnDate, currency);
            if (flightResponse?.OtherFlights == null) return null;

            var selectedHotel = await _dbContext.Hotels.FirstOrDefaultAsync(h => h.ApiHotelId == hotelId);
            if (selectedHotel == null) return null;

            var firstFlight = flightResponse.OtherFlights.FirstOrDefault();
            if (firstFlight?.Flights == null || !firstFlight.Flights.Any()) return null;

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
                DepartureAirport = firstLeg.DepartureAirport.ToDomain(),
                ArrivalAirport = lastLeg.ArrivalAirport.ToDomain(),
                Duration = firstFlight.TotalDuration,
                Flights = firstFlight.Flights.Select(f => $"{f.DepartureAirport.Id} → {f.ArrivalAirport.Id} ({f.Duration} min)").ToList(),
                Extras = firstFlight.Flights.Where(f => f.Extensions != null).SelectMany(f => f.Extensions!).Distinct().ToList()
            };

            var hotelViewModel = new HotelPackageViewModel
            {
                HotelName = selectedHotel.Name,
                HotelAddress = selectedHotel.Address,
                RoomType = "Ikke valgt endnu",
                price = "Ikke angivet",
                PaymentDetails = new(),
                Rooms = new()
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
                            DepartureAirport = leg.DepartureAirport.ToDomain(),
                            ArrivalAirport = last.ArrivalAirport.ToDomain(),
                            Duration = f.TotalDuration,
                            Flights = f.Flights.Select(x => $"{x.DepartureAirport.Id} → {x.ArrivalAirport.Id} ({x.Duration} min)").ToList(),
                            Extras = f.Flights.Where(x => x.Extensions != null).SelectMany(x => x.Extensions!).Distinct().ToList(),
                            Price = f.Price.ToString()
                        };
                    }).ToList()
            };
        }
  /*
public async Task<List<TravelPackageViewModel>> SearchPackagesAsync(string origin, string destination, DateTime departureDate, DateTime returnDate)
        {
            // Search for flights and hotels simultaneously
            var flightsTask = _flightService.SearchFlightsAsync(origin, destination, departureDate);
            var hotelsTask = _hotelService.SearchHotelsAsync(destination, departureDate, returnDate);

            await Task.WhenAll(flightsTask, hotelsTask);

            var flights = await flightsTask;
            var hotels = await hotelsTask;

            // Calculate duration in days for the stay
            int stayDuration = (int)(returnDate - departureDate).TotalDays;

            // Combine flights and hotels into travel packages
            var packages = new List<TravelPackageViewModel>();

            // If there are no flights or hotels, return an empty list
            if (!flights.Any() || !hotels.Any())
            {
                return packages;
            }

            foreach (var flight in flights)
            {
                foreach (var hotel in hotels)
                {
                    // Calculate total price (flight price + (hotel price per night * number of nights))
                    decimal totalPrice = flight.Price + (hotel.PricePerNight * stayDuration);

                    // Create a combined package
                    var package = new TravelPackageViewModel
                    {
                        Id = $"{flight.Id}-{hotel.Id}", // Temporary ID structure
                        Destination = destination,
                        OriginCity = origin,
                        StartDate = departureDate,
                        EndDate = returnDate,
                        Price = totalPrice,
                        Airline = flight.Airline,
                        HotelName = hotel.HotelName,
                        HotelRating = hotel.StarRating,
                        FlightDepartureTime = flight.DepartureTime,
                        FlightArrivalTime = flight.ArrivalTime,
                        ReturnFlightIncluded = true, // In a real implementation, we would handle return flights properly
                        ImageUrl = $"/images/{destination.ToLower()}.jpg", // Assumes we have images named after the destination
                        Flight = flight,
                        Hotel = hotel
                    };

                    packages.Add(package);
                }
            }

            // Sort packages by price (lowest first)
            return packages.OrderBy(p => p.Price).ToList();
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
                BasePrice = model.FinalPrice,
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

        public async Task<TravelPackageViewModel> GetPackageByIdAsync(string packageId)
        {
            // In a real implementation, we would fetch from a database or API
            // For now, we'll split the composite ID
            var ids = packageId.Split('-');
            if (ids.Length != 2)
            {
                throw new ArgumentException("Invalid package ID format");
            }

            var flightId = ids[0];
            var hotelId = ids[1];

            var flight = await _flightService.GetFlightDetailAsync(flightId);
            var hotel = await _hotelService.GetHotelDetailAsync(hotelId);

            // Calculate duration in days for the stay
            int stayDuration = (int)(hotel.CheckOutDate - hotel.CheckInDate).TotalDays;

            // Calculate total price
            decimal totalPrice = flight.Price + (hotel.PricePerNight * stayDuration);

            return new TravelPackageViewModel
            {
                Id = packageId,
                Destination = hotel.City,
                OriginCity = flight.DepartureAirport, // This is a simplified approach
                StartDate = hotel.CheckInDate,
                EndDate = hotel.CheckOutDate,
                Price = totalPrice,
                Airline = flight.Airline,
                HotelName = hotel.HotelName,
                HotelRating = hotel.StarRating,
                FlightDepartureTime = flight.DepartureTime,
                FlightArrivalTime = flight.ArrivalTime,
                ReturnFlightIncluded = true,
                ImageUrl = $"/images/{hotel.City.ToLower()}.jpg",
                Flight = flight,
                Hotel = hotel
            };
  */
        }
    }
