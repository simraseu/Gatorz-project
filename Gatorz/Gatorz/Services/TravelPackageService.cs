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
            var flightsTask = _flightService.SearchFlightsAsync(origin, destination, departureDate);
            var hotelsTask = _hotelService.SearchHotelsAsync(destination, departureDate, returnDate);
            await Task.WhenAll(flightsTask, hotelsTask);

            var flights = flightsTask.Result;
            var hotels = hotelsTask.Result;
            int stayDuration = (int)(returnDate - departureDate).TotalDays;
            var packages = new List<TravelPackageViewModel>();

            foreach (var flight in flights)
            {
                foreach (var hotel in hotels)
                {
                    packages.Add(new TravelPackageViewModel
                    {
                        Id = $"{flight.Id}-{hotel.Id}",
                        Destination = destination,
                        OriginCity = origin,
                        StartDate = departureDate,
                        EndDate = returnDate,
                        Price = flight.Price + (hotel.PricePerNight * stayDuration),
                        Description = $"{flight.Airline} flight and stay at {hotel.HotelName} for {stayDuration} nights",
                        Airline = flight.Airline,
                        HotelName = hotel.HotelName,
                        HotelRating = hotel.StarRating,
                        FlightDepartureTime = flight.DepartureTime,
                        FlightArrivalTime = flight.ArrivalTime,
                        ReturnFlightIncluded = true,
                        ImageUrl = $"/images/{destination.ToLower()}.jpg",
                        Flight = flight,
                        Hotel = hotel
                    });
                }
            }

            return packages.OrderBy(p => p.Price).ToList();
        }

        public async Task<TravelPackageViewModel> GetPackageByIdAsync(string packageId)
        {
            var parts = packageId.Split('-');
            if (parts.Length != 2)
                throw new ArgumentException("Invalid package ID format", nameof(packageId));

            var flights = await _flightService.SearchFlightsAsync("", "", DateTime.Now);
            var hotels = await _hotelService.SearchHotelsAsync("", DateTime.Now, DateTime.Now);

            int flightIdx = int.Parse(parts[0]);
            int hotelIdx = int.Parse(parts[1]);
            var flight = flights.ElementAtOrDefault(flightIdx) ?? throw new InvalidOperationException("Flight not found");
            var hotel = hotels.ElementAtOrDefault(hotelIdx) ?? throw new InvalidOperationException("Hotel not found");

            int stayDuration = (int)(hotel.CheckOutDate - hotel.CheckInDate).TotalDays;
            return new TravelPackageViewModel
            {
                Id = packageId,
                Destination = hotel.City,
                OriginCity = flight.DepartureAirport,
                StartDate = hotel.CheckInDate,
                EndDate = hotel.CheckOutDate,
                Price = flight.Price + (hotel.PricePerNight * stayDuration),
                Description = $"{flight.Airline} flight departing {flight.DepartureTime} and {stayDuration}-night stay at {hotel.HotelName}",
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
        }
    }
}