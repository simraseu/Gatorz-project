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

                    // Create a package
                    packages.Add(new TravelPackageViewModel
                    {
                        Id = $"{flight.Id}-{hotel.Id}",
                        Destination = destination,
                        OriginCity = origin,
                        StartDate = departureDate,
                        EndDate = returnDate,
                        Price = totalPrice,
                        Description = $"Enjoy a {stayDuration}-night stay in {destination} at the {hotel.StarRating}-star {hotel.HotelName}, with direct flights via {flight.Airline}.",
                        Airline = flight.Airline,
                        HotelName = hotel.HotelName,
                        HotelRating = hotel.StarRating,
                        FlightDepartureTime = flight.DepartureTime,
                        FlightArrivalTime = flight.ArrivalTime,
                        ReturnFlightIncluded = true,
                        ImageUrl = $"/images/{destination.ToLower()}.jpg", // You mentioned we can ignore images
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
            // Parse the package ID to get flight and hotel IDs
            var parts = packageId.Split('-');
            if (parts.Length != 2 || !int.TryParse(parts[0], out var flightId) || !int.TryParse(parts[1], out var hotelId))
            {
                throw new ArgumentException("Invalid package ID format", nameof(packageId));
            }

            // For simplicity, we'll search for all flights and hotels and then filter
            // In a real application, you would have a database to store and retrieve packages
            var flights = await _flightService.SearchFlightsAsync("", "", DateTime.Now);
            var hotels = await _hotelService.SearchHotelsAsync("", DateTime.Now, DateTime.Now);

            // Find the matching flight and hotel
            var flight = flights.FirstOrDefault(f => f.Id == flightId);
            var hotel = hotels.FirstOrDefault(h => h.Id == hotelId);

            if (flight == null || hotel == null)
            {
                throw new InvalidOperationException("Could not find the specified flight or hotel");
            }

            // Calculate stay duration
            int stayDuration = (int)(hotel.CheckOutDate - hotel.CheckInDate).TotalDays;

            // Create and return the package
            return new TravelPackageViewModel
            {
                Id = packageId,
                Destination = hotel.City,
                OriginCity = flight.DepartureAirport,
                StartDate = hotel.CheckInDate,
                EndDate = hotel.CheckOutDate,
                Price = flight.Price + (hotel.PricePerNight * stayDuration),
                Description = $"Enjoy a {stayDuration}-night stay in {hotel.City} at the {hotel.StarRating}-star {hotel.HotelName}, with direct flights via {flight.Airline}.",
                Airline = flight.Airline,
                HotelName = hotel.HotelName,
                HotelRating = hotel.StarRating,
                FlightDepartureTime = flight.DepartureTime,
                FlightArrivalTime = flight.ArrivalTime,
                ReturnFlightIncluded = true,
                ImageUrl = $"/images/{hotel.City.ToLower()}.jpg", // You mentioned we can ignore images
                Flight = flight,
                Hotel = hotel
            };
        }
    }
}