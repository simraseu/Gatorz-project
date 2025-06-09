using Gotorz.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text;

namespace Gotorz.Services
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

                    // 🔧 NEW: Encode flight and hotel data directly in the package ID
                    var packageId = EncodePackageData(origin, destination, departureDate, returnDate, flight, hotel);

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
            try
            {
                // 🔧 NEW: Decode flight and hotel data from package ID - NO API CALLS!
                var packageData = DecodePackageData(packageId);

                var origin = packageData.Origin;
                var destination = packageData.Destination;
                var departureDate = packageData.DepartureDate;
                var returnDate = packageData.ReturnDate;
                var flight = packageData.Flight;
                var hotel = packageData.Hotel;

                // Calculate stay duration and total price
                int stayDuration = (int)(returnDate - departureDate).TotalDays;
                decimal totalPrice = flight.Price + (hotel.PricePerNight * stayDuration);

                // Create and return the package using decoded data
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
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Unable to decode package data from ID: {packageId}. Error: {ex.Message}");
            }
        }

        // 🔧 NEW: Helper method to encode all package data into the ID
        private string EncodePackageData(string origin, string destination, DateTime departureDate, DateTime returnDate, FlightInfo flight, HotelInfo hotel)
        {
            var packageData = new
            {
                Origin = origin,
                Destination = destination,
                DepartureDate = departureDate,
                ReturnDate = returnDate,
                Flight = flight,
                Hotel = hotel
            };

            // Serialize to JSON then encode to Base64 to create a compact package ID
            var json = JsonSerializer.Serialize(packageData);
            var bytes = Encoding.UTF8.GetBytes(json);
            return Convert.ToBase64String(bytes);
        }

        // 🔧 NEW: Helper method to decode package data from the ID  
        private dynamic DecodePackageData(string packageId)
        {
            try
            {
                // Decode from Base64 then deserialize from JSON
                var bytes = Convert.FromBase64String(packageId);
                var json = Encoding.UTF8.GetString(bytes);

                // Parse as dynamic object
                var jsonDoc = JsonDocument.Parse(json);
                var root = jsonDoc.RootElement;

                return new
                {
                    Origin = root.GetProperty("Origin").GetString(),
                    Destination = root.GetProperty("Destination").GetString(),
                    DepartureDate = root.GetProperty("DepartureDate").GetDateTime(),
                    ReturnDate = root.GetProperty("ReturnDate").GetDateTime(),
                    Flight = JsonSerializer.Deserialize<FlightInfo>(root.GetProperty("Flight").GetRawText()),
                    Hotel = JsonSerializer.Deserialize<HotelInfo>(root.GetProperty("Hotel").GetRawText())
                };
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Invalid package ID format: {ex.Message}", nameof(packageId));
            }
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