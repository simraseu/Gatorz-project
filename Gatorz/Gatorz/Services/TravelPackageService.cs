using Gatorz.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gatorz.Services
{
    public class TravelPackageService : ITravelPackageService
    {
        private readonly IFlightService _flightService;
        private readonly IHotelService _hotelService;

        public TravelPackageService(IFlightService flightService, IHotelService hotelService)
        {
            _flightService = flightService;
            _hotelService = hotelService;
        }

        public async Task<List<TravelPackageViewModel>> SearchPackagesAsync(string origin, string destination, DateTime departureDate, DateTime returnDate)
        {
            // Validate input parameters
            if (string.IsNullOrWhiteSpace(origin) || string.IsNullOrWhiteSpace(destination))
            {
                return new List<TravelPackageViewModel>();
            }

            // Search for flights and hotels in parallel
            var flightsTask = _flightService.SearchFlightsAsync(origin, destination, departureDate);
            var hotelsTask = _hotelService.SearchHotelsAsync(destination, departureDate, returnDate);
            await Task.WhenAll(flightsTask, hotelsTask);

            var flights = flightsTask.Result;
            var hotels = hotelsTask.Result;

            // If we don't have any flights or hotels, return an empty list
            if (!flights.Any() || !hotels.Any())
            {
                return new List<TravelPackageViewModel>();
            }

            int stayDuration = (int)(returnDate - departureDate).TotalDays;
            var packages = new List<TravelPackageViewModel>();

            // Create travel packages by combining flights and hotels
            foreach (var flight in flights)
            {
                foreach (var hotel in hotels)
                {
                    // Calculate total price
                    decimal totalPrice = flight.Price + (hotel.PricePerNight * stayDuration);

                    // Create a package with encoded parameters in the ID for later reconstruction
                    var packageId = $"{origin}_{destination}_{departureDate:yyyyMMdd}_{returnDate:yyyyMMdd}_{flight.Id}_{hotel.Id}";

                    packages.Add(new TravelPackageViewModel
                    {
                        Id = packageId,
                        Destination = destination,
                        OriginCity = origin,
                        StartDate = departureDate,
                        EndDate = returnDate,
                        Price = totalPrice,
                        Description = $"Enjoy a {stayDuration}-night stay in {destination} at the {hotel.StarRating}-star {hotel.HotelName}, with flights via {flight.Airline}.",
                        Airline = flight.Airline,
                        HotelName = hotel.HotelName,
                        HotelRating = hotel.StarRating,
                        FlightDepartureTime = flight.DepartureTime,
                        FlightArrivalTime = flight.ArrivalTime,
                        ReturnFlightIncluded = true,
                        ImageUrl = GetDestinationImageUrl(destination),
                        Flight = flight,
                        Hotel = hotel
                    });
                }
            }

            // Order packages by price
            return packages.OrderBy(p => p.Price).ToList();
        }

        public async Task<TravelPackageViewModel> GetPackageByIdAsync(string packageId)
        {
            // Parse the package ID to extract search parameters and IDs
            var parts = packageId.Split('_');
            if (parts.Length != 6)
            {
                throw new ArgumentException("Invalid package ID format", nameof(packageId));
            }

            var origin = parts[0];
            var destination = parts[1];

            if (!DateTime.TryParseExact(parts[2], "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out var departureDate))
            {
                throw new ArgumentException("Invalid departure date in package ID", nameof(packageId));
            }

            if (!DateTime.TryParseExact(parts[3], "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out var returnDate))
            {
                throw new ArgumentException("Invalid return date in package ID", nameof(packageId));
            }

            if (!int.TryParse(parts[4], out var flightId) || !int.TryParse(parts[5], out var hotelId))
            {
                throw new ArgumentException("Invalid flight or hotel ID in package ID", nameof(packageId));
            }

            // Re-run the search to get the same results (this ensures consistency)
            var flightsTask = _flightService.SearchFlightsAsync(origin, destination, departureDate);
            var hotelsTask = _hotelService.SearchHotelsAsync(destination, departureDate, returnDate);
            await Task.WhenAll(flightsTask, hotelsTask);

            var flights = flightsTask.Result;
            var hotels = hotelsTask.Result;

            // Find the matching flight and hotel by ID
            var flight = flights.FirstOrDefault(f => f.Id == flightId);
            var hotel = hotels.FirstOrDefault(h => h.Id == hotelId);

            if (flight == null)
            {
                throw new InvalidOperationException($"Could not find flight with ID {flightId} for route {origin} to {destination}");
            }

            if (hotel == null)
            {
                throw new InvalidOperationException($"Could not find hotel with ID {hotelId} in {destination}");
            }

            // Calculate stay duration and total price
            int stayDuration = (int)(returnDate - departureDate).TotalDays;
            decimal totalPrice = flight.Price + (hotel.PricePerNight * stayDuration);

            // Create and return the package
            return new TravelPackageViewModel
            {
                Id = packageId,
                Destination = destination,
                OriginCity = origin,
                StartDate = departureDate,
                EndDate = returnDate,
                Price = totalPrice,
                Description = $"Enjoy a {stayDuration}-night stay in {destination} at the {hotel.StarRating}-star {hotel.HotelName}, with flights via {flight.Airline}.",
                Airline = flight.Airline,
                HotelName = hotel.HotelName,
                HotelRating = hotel.StarRating,
                FlightDepartureTime = flight.DepartureTime,
                FlightArrivalTime = flight.ArrivalTime,
                ReturnFlightIncluded = true,
                ImageUrl = GetDestinationImageUrl(destination),
                Flight = flight,
                Hotel = hotel
            };
        }

        private string GetDestinationImageUrl(string destination)
        {
            // Map destination codes to image filenames
            var imageMap = new Dictionary<string, string>
            {
                { "BCN", "/images/barcelona.jpg" },
                { "ROM", "/images/rome.jpg" },
                { "FCO", "/images/rome.jpg" },
                { "PAR", "/images/paris.jpg" },
                { "CDG", "/images/paris.jpg" },
                { "LON", "/images/london.jpg" },
                { "LHR", "/images/london.jpg" },
                { "DXB", "/images/dubai.jpg" },
                { "NYC", "/images/newyork.jpg" },
                { "JFK", "/images/newyork.jpg" }
            };

            return imageMap.TryGetValue(destination.ToUpper(), out var imageUrl)
                ? imageUrl
                : "/images/default-destination.jpg";
        }
    }
}