using Gatorz.Models;

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
        }
    }
}